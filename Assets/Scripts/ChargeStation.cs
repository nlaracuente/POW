using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Recharges any power sources that it comes in contact with
/// </summary>
public class ChargeStation : MonoBehaviour
{
    /// <summary>
    /// Recharges all power sources within range
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerStay(Collider other)
    {
        PowerSource source = other.GetComponent<PowerSource>();
        if(source != null) {
            source.Recharge();
        }
    }
}
