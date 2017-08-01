using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This trigger occurs when the player walks into the bomb 
/// when the bomb is not active because the companion does 
/// not have any power or the companion is not with the player
/// </summary>
public class PlayerTriggeredBomb : MonoBehaviour
{
    /// <summary>
    /// A reference to the parent bomb
    /// </summary>
    [SerializeField]
    BombTile bomb;

    /// <summary>
    /// Stores a reference to the parent bomb
    /// </summary>
    void Start()
    {
        this.bomb = GetComponentInParent<BombTile>();
    }

    /// <summary>
    /// Kaboom! baby
    /// </summary>
    void OnTriggerEnter(Collider other)
    {
        // Already went off - don't retrigger
        if(this.bomb.isTriggered) {
            return;
        }

        if(other.tag == "Player") {
            this.bomb.isTriggered = true;
            bomb.Activate();
        }
    }
	
}
