using UnityEngine;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float speed = 4.0f;
    [SerializeField] private float jumpForce = 7.5f;
    [SerializeField] private float rollForce = 6.0f;
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private Transform attackSensor;
    [SerializeField] private float blockDuration = 1.0f; 
    [SerializeField] private float perfectBlockWindow = 0.1f; 
    [SerializeField] private float stunDuration = 2.0f; 
    [SerializeField] private Transform groundSensor; 

    public float attackRange = 2f;      
    public int attackDamage = 25;

    public LayerMask groundLayer;
    public LayerMask enemyLayer;
    public HealthBar healtbar;

    private float blockStartTime;
    private bool isDead = false;
    private Animator animator;
    private Rigidbody2D body2d;
    private bool grounded = false;
    private bool rolling = false;
    private int facingDirection = 1;
    private int currentAttack = 0;
    private float timeSinceAttack = 0.0f;
    private float delayToIdle = 0.0f;
    private float rollDuration = 8.0f / 14.0f;
    private float rollCurrentTime;
    private bool isAttacking = false;
    private bool isBlocking = false;
    private bool blockedAttack = false;
    private int health = 0;

    private const int totalAttacks = 3;
    private const float attackInterval = 0.25f;
    private const float totalComboInterval = totalAttacks * attackInterval;
    private enum Action
    {
        MoveLeft,
        MoveRight,
        Jump,
        Roll,
        Attack,
        Block,
    }

    private Dictionary<Action, KeyCode> actionKeys = new Dictionary<Action, KeyCode>
    {
        { Action.MoveLeft, KeyCode.A },
        { Action.MoveRight, KeyCode.D },
        { Action.Jump, KeyCode.Space },
        { Action.Roll, KeyCode.LeftShift },
        { Action.Attack, KeyCode.Mouse0 }, 
        { Action.Block, KeyCode.F }
    };

    void Start()
    {
        health = maxHealth;
        healtbar.SetMaxHealth(maxHealth);
        animator = GetComponent<Animator>();
        body2d = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        ProcessInput();
        HandleRolling();
        PerformBlock();
        CheckGroundedState();
    }
    private void HandleMovement()
    {
        float inputX = Input.GetAxis("Horizontal");

        float currentSpeed = speed;

        if (isAttacking || isBlocking)
        {
            currentSpeed /= 4f; 
        }

        if (Mathf.Abs(inputX) > Mathf.Epsilon)
        {
            body2d.velocity = new Vector2(inputX * currentSpeed, body2d.velocity.y);

            int scale = inputX > 0 ? 1 : -1;

            transform.localScale = new Vector3(scale, 1, 1);

            animator.SetInteger("AnimState", 1);
        }
        else
        {
            animator.SetInteger("AnimState", 0);
        }

        animator.SetFloat("AirSpeedY", body2d.velocity.y);
    }


    private void ProcessInput()
    {
        timeSinceAttack += Time.deltaTime;
        float remainingAttacks = totalAttacks - currentAttack;
        float attackResetTime = 1 - 1 / remainingAttacks;

        if (isAttacking && timeSinceAttack > attackResetTime)
        {
            isAttacking = false;
        }

        foreach (var actionKey in actionKeys)
        {
            if (Input.GetKey(actionKey.Value))
            {
                HandleAction(actionKey.Key);
            }
        }

        if (Mathf.Abs(Input.GetAxis("Horizontal")) > Mathf.Epsilon)
        {
            delayToIdle = 0.05f;
            animator.SetInteger("AnimState", 1);
        }
        else
        {
            delayToIdle -= Time.deltaTime;
            if (delayToIdle < 0)
                animator.SetInteger("AnimState", 0);
        }
    }

    private void HandleAction(Action action)
    {
        switch (action)
        {
            case Action.MoveRight:
            case Action.MoveLeft:
                HandleMovement();
                break;
            case Action.Jump:
                Jump();
                break;
            case Action.Roll:
                StartRolling();
                break;
            case Action.Attack:
                HandleAttack();
                break;
        }
    }

    private void HandleAttack()
    {
        if(timeSinceAttack > attackInterval && !rolling)
        {
            isAttacking = true;
            currentAttack++;

            if (currentAttack >= totalAttacks)
                currentAttack = 0;

            if (timeSinceAttack >= totalComboInterval)
                currentAttack = 0;

            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackSensor.position, attackRange, enemyLayer);

            foreach (Collider2D enemy in hitEnemies)
            {
                BaseEnemy enemyHealth = enemy.GetComponent<BaseEnemy>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(attackDamage);
                }
            }

            animator.SetTrigger("Attack" + (currentAttack + 1));

            timeSinceAttack = 0.0f; 
        } 
    }

    private void CheckGroundedState()
    {
        grounded = Physics2D.OverlapCircle(groundSensor.position, 0.25f, groundLayer);
        animator.SetBool("Grounded", grounded);
    }
    private void HandleRolling()
    {
        if (rolling)
        {
            rollCurrentTime += Time.deltaTime;

            if (rollCurrentTime > rollDuration)
            {
                rolling = false;
                rollCurrentTime = 0;
            }
        }
    }

    private void StartRolling()
    {
        if (!rolling)
        {
            rolling = true;
            TriggerAnimation("Roll");
            body2d.velocity = new Vector2(facingDirection * rollForce, body2d.velocity.y);
        }
    }

    private void Jump()
    {
        if (grounded)  
        {
            TriggerAnimation("Jump");
            grounded = false;
            animator.SetBool("Grounded", grounded);
            body2d.velocity = new Vector2(body2d.velocity.x, jumpForce);
        }
    }

    private void PerformBlock()
    {
        if (grounded)
        {
            if (Input.GetKey(actionKeys[Action.Block]))
            {
                if (!isBlocking)
                {
                    isBlocking = true;
                    blockStartTime = Time.time;
                }
                animator.SetBool("IdleBlock", true);
            }
            else
            {
                isBlocking = false;
                animator.SetBool("IdleBlock", false);
            }
        }
    }

    private void TriggerAnimation(string trigger)
    {
        animator.SetTrigger(trigger);
    }

    public bool IsPerfectBlock()
    {
        float elapsed = Time.time - blockStartTime;
        return isBlocking && elapsed <= perfectBlockWindow;
    }

    public void TakeDamage(int damage, GameObject attacker)
    {
        if (!isDead)
        {
            if (isBlocking)
            {
                if (IsPerfectBlock())
                {
                    TriggerAnimation("PerfectBlock");
                    BaseEnemy enemyController = attacker.GetComponent<BaseEnemy>();
                    if (enemyController != null)
                    {
                        enemyController.Stun(stunDuration);
                    }
                }
                else
                {
                    //TriggerAnimation("Block");
                    health -= damage / 2; 
                }
            }
            else
            {
                TriggerAnimation("Hurt");
                health -= damage;
            }

            healtbar.SetHealth(health);

            if (health <= 0)
            {
                TriggerAnimation("Death");
                isDead = true;
                this.enabled = false;
            }
        }
    }

    //private void OnCollisionEnter2D(Collision2D collision)
    //{
    //    if (collision.gameObject.CompareTag("Ground"))
    //    {
    //        foreach (ContactPoint2D contact in collision.contacts)
    //        {
    //            if (contact.normal.y > 0.5f)
    //            {
    //                grounded = true;
    //                animator.SetBool("Grounded", grounded);
    //                return; 
    //            }
    //        }
    //    }

    //    grounded = false;
    //    animator.SetBool("Grounded", grounded);
    //}
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackSensor.position, attackRange);
        Gizmos.DrawWireSphere(groundSensor.position, 0.1f);
    }
}
