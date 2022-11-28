using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public Transform attackPoint;
    public float attackRadius = 0.5f;
    public LayerMask enemyLayers;
    public int attackPower = 1;
    public static int maxHP = 5;
    public int currentHP;

    public void Start()
    {
        currentHP = maxHP;
    }
    public void TakeDamage(int attackPower)
    {
        currentHP -= attackPower;
        if(currentHP <= 0)
        {
            Debug.Log("HP <= 0, resetting hp and changing pos to (0,0)");
            transform.position = new Vector2(0, 0);
            currentHP = maxHP;
        }
    }
    void Attack()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, enemyLayers);
        foreach(Collider2D hit in hits)
        {
            Debug.Log(hit.name);
            hit.GetComponent<Damageable>().TakeDamage(attackPower);
        }
    }

    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            Attack();
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
}
