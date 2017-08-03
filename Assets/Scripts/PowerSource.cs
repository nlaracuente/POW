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
    protected int maxPower = 30;

    /// <summary>
    /// The current power the companion
    /// </summary>
    [SerializeField]
    protected int currentPower = 30;

    /// <summary>
    /// How many seconds to wait before taking power away
    /// </summary>
    [SerializeField]
    protected float drainDelay = 1f;

    /// <summary>
    /// True while the 
    /// </summary>
    [SerializeField]
    bool isCharging = false;
    public virtual bool IsCharging
    {
        get { return this.isCharging; }

        // Initiate/Stop drain coroutine based on whether or not it is being charged
        set
        {
            if(!value) {
                StartCoroutine("DrainPowerSupply");
            } else {
                StopCoroutine("DrainPowerSupply");
            }            
            this.isCharging = value;
        } 
    }

    /// <summary>
    ///  Start draining energy
    /// </summary>
    void Start()
    {
        StartCoroutine("DrainPowerSupply");
    }

    /// <summary>
    /// Check if this is recharging and calls the recharge
    /// </summary>
    void LateUpdate()
    {
        if(this.isCharging) {
            this.Recharge();
        }
    }

    /// <summary>
    /// Returns true while there is still power
    /// </summary>
    public bool HasPower
    {
        get { return this.currentPower > 0; }
    }

    
    /// <summary>
    /// Total amount of power to consume
    /// </summary>
    /// <param name="total"></param>
    public virtual void ConsumePower(int total)
    {
        this.currentPower = Mathf.Max(0, this.currentPower - total);
    }

    /// <summary>
    /// Restores power by the given wuantity
    /// </summary>
    public virtual void Recharge()
    {
        this.currentPower = this.maxPower;
    }

    /// <summary>
    /// For as long as the power supply is not being charged then it will drain
    /// one power every -n- number of seconds
    /// </summary>
    /// <returns></returns>
    IEnumerator DrainPowerSupply()
    {
        int drainCost = 1;
        while(this.HasPower) {
            yield return new WaitForSeconds(this.drainDelay);
            this.ConsumePower(drainCost);
        }
    } 
}
