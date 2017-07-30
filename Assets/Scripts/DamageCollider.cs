using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Damages the player on trigger stay
/// </summary>
public class DamageCollider : MonoBehaviour
{
    void OnTriggerStay(Collider other)
    {
        if(other.tag == "Player") {
            other.GetComponent<Player>().PlayerDamaged();
        }
    }
}
