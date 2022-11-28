using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hazard : MonoBehaviour
{
    public int damage = 1;
    private void OnCollisionEnter2D(Collision2D collision)
    {
        PlayerCombat player = GetComponent<PlayerCombat>();

        if(player != null)
        {
            player.TakeDamage(damage);
        }
    }
}
