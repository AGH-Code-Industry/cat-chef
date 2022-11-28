using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damageable : MonoBehaviour
{
    public int hp = 5;
    public int shakeDuration = 10;
    public float shakeForce = 0.1f;

    public void TakeDamage(int attackPower)
    {
        hp -= attackPower;
        if(hp <= 0)
        {
            Debug.Log(gameObject.name + " destroyed by attack");
            Destroy(gameObject);
        }
    }
}
