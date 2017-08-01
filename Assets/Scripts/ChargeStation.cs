using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Recharges any power sources that it comes in contact with
/// </summary>
public class ChargeStation : MonoBehaviour
{
    /// <summary>
    /// A reference to the power source being charged
    /// </summary>
    PowerSource source;

    /// <summary>
    /// Recharges all power sources within range
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerStay(Collider other)
    {
        PowerSource source = other.GetComponent<PowerSource>();
        if(source != null && this.source == null) {
            this.source = source;
            source.IsCharging = true;
        } 

        // Updates the player's checkpoint to have them restart here
        if(other.tag == "Player") {
            other.GetComponent<Player>().UpdateCheckpoint(this.transform.position);
        }
    }

    /// <summary>
    /// No longer charging the power supply
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerExit(Collider other)
    {
        PowerSource source = other.GetComponent<PowerSource>();
        if(source != null && source == this.source) {
            this.source.IsCharging = false;
            this.source = null;
        }
    }
}
