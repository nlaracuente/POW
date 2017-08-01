using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Triggers the player to teleport to the associated teleporter
/// Teleporting happens only once each time the player steps into the portal
/// </summary>
public class TeleportTrigger : MonoBehaviour
{
    /// <summary>
    /// The teleporter this teleporter connects to 
    /// </summary>
    [SerializeField]
    TeleportTrigger associatedTeleporter;

    /// <summary>
    /// How long, in seconds, to wait before giving control back to the player
    /// once they have stepped into the teleporter
    /// </summary>
    [SerializeField]
    [Range(0, 5)]
    float waitTime = .5f;

    /// <summary>
    /// True when the teleportation has been triggered
    /// </summary>
    [SerializeField]
    bool isTeleporting = false;

    /// <summary>
    /// Notifes the connected teleported that the target is on its way
    /// so as to prevent the other teleporter from immediatly sending the 
    /// target back to this teleporter on arrival
    /// </summary>
    internal bool WaitForTarget
    {
        get
        {
            return this.portal.isWaitingForTarget;
        }
        set
        {
            this.portal.isWaitingForTarget = value;
        }
    }

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
    /// Notifies the connecting teleported we are sending a target over there
    /// Sets the "teleporting" flag ON to avoid double teleporting since we use a coroutine
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerStay(Collider other)
    {
        // Cannot teleport if waiting for the target
        if(this.WaitForTarget) {
            return;
        }
        
        if(other.tag == "Player" && !this.isTeleporting) {
            this.isTeleporting = true;
            this.associatedTeleporter.WaitForTarget = true;
            StartCoroutine("Teleport", other.GetComponent<Player>());
        }
    }

    /// <summary>
    /// Sends the player over to the connecting teleporter
    /// Notifies the teleporter of the incoming player to prevent sending them right back
    /// The wait time here is to allow the camera time to catch up with the player's new position
    /// </summary>
    /// <param name="target"></param>
    IEnumerator Teleport(Player player)
    {
        AudioManager.instance.PlaySound(AudioManager.SoundName.TeleportalUsed);
        player.DisablePlayerControl();

        // Snap to this tile and notify the target portal the player arrived
        player.transform.position = this.transform.position;
        player.TeleportTo(this.associatedTeleporter.transform.position);

        // Now that the player is no longer on the teleporter
        // We can disabling isTeleporting
        this.isTeleporting = false;

        // Wait before returning control
        yield return new WaitForSeconds(this.waitTime);
        player.EnablePlayerControl();
    }
}
