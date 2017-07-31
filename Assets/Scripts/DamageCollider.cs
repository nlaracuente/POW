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
            Player player = other.GetComponent<Player>();
            player.DisablePlayerControl();

            // Snap to this tile 
            player.transform.position = this.transform.position;
            
            player.PlayerDamaged();
        }
    }
}
