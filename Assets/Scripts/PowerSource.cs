using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A PowerSource is what gives power to consumables in order for them to become
/// activated for as long as the source has enough power and is within range
/// </summary>
public class PowerSource : MonoBehaviour
{
    /// <summary>
    /// Total power the companion can have
    /// </summary>
    [SerializeField]
    protected int maxPower = 10;

    /// <summary>
    /// The current power the companion
    /// </summary>
    [SerializeField]
    protected int currentPower = 10;

    /// <summary>
    /// Returns true when the powersupply has enough power to consume the cost
    /// </summary>
    /// <param name="cost"></param>
    /// <returns></returns>
    public bool CanBeConsumed(int cost)
    {
        // Reducing the power source to 0 without going under
        // means it had enough to power it
        return this.currentPower - cost > -1;
    }

    /// <summary>
    /// Total amount of power to consume
    /// </summary>
    /// <param name="total"></param>
    public void ConsumePower(int total)
    {
        this.currentPower = Mathf.Max(0, this.currentPower - total);
    }

    /// <summary>
    /// Restores power by the given wuantity
    /// </summary>
    /// <param name="total"></param>
    public void RestorePower(int total)
    {
        this.currentPower = Mathf.Min(this.maxPower, this.currentPower + total);
    }
}
