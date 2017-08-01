using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Detects when the player is within the portal to allow or prevent
/// teleportation to happen. 
/// Reason to prevent teleportation:
///     - Player arrived to this teleported (with or without the companion)
/// Trigger detects when the player is on the portal, checks if the portal was waiting for a target
/// and disables teleportation for as long as the player is on the tile
/// </summary>
public class PortalPlayerDetection : MonoBehaviour
{
    /// <summary>
    /// A reference to this parent's teleporter
    /// </summary>
    [SerializeField]
    TeleporterTile portal;

    /// <summary>
    /// Stores a reference to the parent portal
    /// </summary>
    void Start()
    {
        this.portal = GetComponentInParent<TeleporterTile>();
    }

    /// <summary>
    /// Player has stepped off the teleporter
    /// That means we are no longer waiting for the targer
    /// </summary>
    void OnTriggerExit(Collider other)
    {
        if(other.tag != "Player") {
            return;
        }
        
        this.portal.isWaitingForTarget = false;
    }
}
