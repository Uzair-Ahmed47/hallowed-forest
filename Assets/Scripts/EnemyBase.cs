using System.Collections;
using UnityEngine;
using System;
using UnityEngine.UIElements;

public abstract class BaseEnemy : MonoBehaviour
{
    [SerializeField] protected float detectionRange = 10.0f;
    [SerializeField] protected float attackRange = 2f;
    [SerializeField] protected float attackCooldown = 1.0f;
    [SerializeField] protected int attackDamage = 10;
    [SerializeField] protected Transform attackPoint;

    protected int facingDirection = 1;
    protected Rigidbody2D body2d;
    protected PlayerController playerController;
    protected Transform player;
    protected bool isAttacking = false;
    protected float attackTimer = 0.0f;

    public string enemyName = "Enemy";
    public int health = 100;
    public float speed = 2.0f;
    protected bool isDead = false;
    public bool isStunned = false;

    protected Animator animator;

    public event Action<BaseEnemy> OnEnemyDied;
    virtual protected void Start()
    {
        
    }

    virtual protected void Update()
    {
        if (attackTimer > 0)
            attackTimer -= Time.deltaTime;

        if (player == null) return;

        if (!isStunned)
        {
            HandleMovement();
            HandleAttack();
        }
    }

    public virtual void TakeDamage(int damage)
    {
        if (!isDead)
        {
            health -= damage;
            animator.SetTrigger("TakeHit");
            if (health <= 0)
            {
                Die();
            }
        }
    }

    protected virtual void Die()
    {
        isDead = true;
        animator.SetTrigger("Death");
        animator.SetBool("Dead", true);

        OnEnemyDied?.Invoke(this); 
        Destroy(gameObject, 2f);
    }


    public virtual void Stun(float duration)
    {
        if (!isStunned)
        {
            isStunned = true;
            animator.SetBool("Stunned", true);
            StartCoroutine(StunCoroutine(duration));
        }
    }

    private IEnumerator StunCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        isStunned = false;
        animator.SetBool("Stunned", false);
    }

    protected virtual void HandleAttack()
    {
        float distanceToPlayer = Vector2.Distance(attackPoint.position, player.position);

        if (distanceToPlayer <= attackRange && attackTimer <= 0)
        {
            StartAttack();
        }
    }

    protected virtual void HandleMovement()
    {
        float distanceToPlayer = Vector2.Distance(attackPoint.position, player.position);

        if (distanceToPlayer < detectionRange && distanceToPlayer > attackRange)
        {
            MoveTowardPlayer();
        }
        else
        {
            animator.SetInteger("AnimState", 0);
        }
    }
    protected virtual void StartAttack()
    {
        animator.SetTrigger("Attack");
        isAttacking = true;
        attackTimer = attackCooldown;
        body2d.velocity = Vector2.zero;
    }
    protected virtual void MoveTowardPlayer()
    {
        int direction = player.position.x > transform.position.x ? 1 : -1;

        facingDirection = direction;

        body2d.velocity = new Vector2(facingDirection * speed, body2d.velocity.y);

        transform.localScale = new Vector3(facingDirection, 1, 1);

        animator.SetInteger("AnimState", 1);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
    public abstract void AttackPlayer();
}
