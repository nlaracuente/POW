using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Triggers the player to teleport to the associated teleporter
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
    bool isTeleporting = false;

    /// <summary>
    /// Notifes the connected teleported that the target is on its way
    /// so as to prevent the other teleporter from immediatly sending the 
    /// target back to this teleporter on arrival
    /// </summary>
    protected bool waitingForTarget = false;

    /// <summary>
    /// Triggers teleportation if not already teleporting
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerStay(Collider other)
    {
        // Only if not waiting for the target 
        if(!this.waitingForTarget && other.tag == "Player") {

            if(!this.isTeleporting) {
                StartCoroutine("Teleport", other.GetComponent<Player>());
            }
        }
    }

    /// <summary>
    /// Target has left the building
    /// Reset the teleporter's state
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerExit(Collider other)
    {
        if(other.tag == "Player") {
            this.isTeleporting = false;
            this.waitingForTarget = false;
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
        player.DisablePlayerControl();

        // Snap to this tile 
        player.transform.position = this.transform.position;

        this.isTeleporting = true;
        this.associatedTeleporter.waitingForTarget = true;

        player.transform.position = this.associatedTeleporter.transform.position;
        yield return new WaitForSeconds(this.waitTime);

        player.EnablePlayerControl();
    }
}
