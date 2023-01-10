using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, IDamageDealer
{
    [SerializeField] private HealthBar healthBar;
    [SerializeField] private float maxHealth = 100f;
    public float damage { get; set; } = 20f;


    private float health;

    private void Awake() {
        health = maxHealth;
        healthBar.SetMaxHealth(maxHealth);
    }

    private void OnTriggerEnter2D(Collider2D other) {
        MeleeWeapon meleeWeapon = other.transform.GetComponentInParent<MeleeWeapon>();
        if (!meleeWeapon) return;
        TakeDamage(meleeWeapon.damage);
    }

    private void TakeDamage(float amount) {
        health -= amount;
        health = Mathf.Clamp(health, 0, maxHealth);
        if (health == 0) {
            Die();
        }
        healthBar.SetHealth(health);
    }

    private void Die() {
        Destroy(gameObject);
    }
}
