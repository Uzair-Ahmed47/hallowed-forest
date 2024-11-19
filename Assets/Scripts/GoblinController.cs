using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoblinController : BaseEnemy
{
    protected override void Start()
    {
        enemyName = "Goblin";
        animator = gameObject.GetComponent<Animator>();
        body2d = gameObject.GetComponent<Rigidbody2D>();

        player = GameObject.FindWithTag("Player").transform;
        playerController = player.GetComponent<PlayerController>();
    }
    protected override void Die()
    {
        base.Die();
        this.enabled = false;
    }

    public override void AttackPlayer()
    {
        playerController.TakeDamage(attackDamage, gameObject);
    }
}
