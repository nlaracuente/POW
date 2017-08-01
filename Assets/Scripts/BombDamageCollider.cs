using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Checks if the player is within range and triggers damage
/// </summary>
public class BombDamageCollider : MonoBehaviour
{
    /// <summary>
    /// Bomb is at a state where it can inflict 
    /// </summary>
    public bool canInflictDamage = false;

    /// <summary>
    /// Damage has been inflicted, prevents this from inflicting damage again and again
    /// </summary>
    [SerializeField]
    bool damageInflicted = false;

    /// <summary>
    /// While the player is on the damage trigger, waits until we can inflict damage
    /// and inflicts damage once
    /// </summary>
    /// <param name="other"></param>
	void OnTriggerStay(Collider other)
    {
        if(!this.canInflictDamage || this.damageInflicted) {
            return;
        }

        if(other.tag == "Player") {
            Player player = other.GetComponent<Player>();
            player.PlayerDamaged();
            this.damageInflicted = false;
            this.canInflictDamage = false;
        }
    }
}
