using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A consumable object is an object that consumes power from a power source
/// to remain active. As long as the power source is withing range then it 
/// can remain activate 
/// </summary>
public class Consumable : MonoBehaviour, IConsumable
{
    /// <summary>
    /// A reference to the animator component that holds the
    /// Activate/Deactivate states
    /// </summary>
    [SerializeField]
    protected Animator animator;

    /// <summary>
    /// True when the object is actively consuming power
    /// </summary>
    [SerializeField]
    bool isActivated = false;

    /// <summary>
    /// True when the consumable is no longer active
    /// Default is the consumable is always inactive
    /// </summary>
    [SerializeField]
    bool isDeactivated = true;

    /// <summary>
    /// Total power needed to activate this consumable
    /// </summary>
    [SerializeField]
    protected int powerCost = 1;

    /// <summary>
    /// How many seconds to wait before consuming power again
    /// Half a minute 
    /// </summary>
    [SerializeField]
    [Range(0.5f, 60f)]
    protected float drainDelay = 1;

    /// <summary>
    /// The current power source in range powering this consumable
    /// </summary>
    PowerSource powerSource;

    /// <summary>
    /// Powers on this consumable
    /// </summary>
    /// <param name="source"></param>
    public void Activate()
    {
        // Play the animation only once
        if(!this.isActivated) {
            this.isActivated = true;
            this.isDeactivated = false;

            if(this.animator != null) {
                this.animator.SetTrigger("Activate");
            }
        }
    }

    /// <summary>
    /// Powers down this consumable
    /// </summary>
    public void Deactivate()
    {
        if(!this.isDeactivated ) {
            this.isActivated = false;
            this.isDeactivated = true;

            if(this.animator != null) {
                this.animator.SetTrigger("Deactivate");
            }
        }
    }

    /// <summary>
    /// If the given source has enough power to activate this consumable
    /// then it consumes it and powers itself, otherwise, it deactivates itself
    /// Triggers the DrainPowerSupply coroutine to consume power again when it needs to
    /// </summary>
    /// <param name="source"></param>
    protected void ConsumePowerSource()
    {
        // Still has juice
        if(this.powerSource.CanBeConsumed(this.powerCost)) {
            this.Activate();
            this.powerSource.ConsumePower(this.powerCost);
            StartCoroutine("DrainPowerSupply");

        // No enough to power up this consumable
        } else {
            this.Deactivate();
        }
    }

    /// <summary>
    /// Waits for the time defined in <see cref="this.drainDelay"/> to pass
    /// before attempting to consume power again
    /// If the power source is no longer available then it deactivates itself
    /// </summary>
    /// <returns></returns>
    IEnumerator DrainPowerSupply()
    {
        yield return new WaitForSeconds(this.drainDelay);
        
        // Still have a power source
        if(this.powerSource != null) {
            this.ConsumePowerSource();

        // No more power
        } else {
            this.Deactivate();
        }
    }

    /// <summary>
    /// If a power source is within range then it attempts to consume it
    /// </summary>
    void OnTriggerStay(Collider other)
    {
        PowerSource source = other.GetComponent<PowerSource>();

        if(source != null && !this.isActivated) {
            this.powerSource = source;
            this.ConsumePowerSource();
        }        
    }

    /// <summary>
    /// Checks if the power source is no longer in range
    /// Removes the reference to the power source if this is the case
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerExit(Collider other)
    {
        PowerSource source = other.GetComponent<PowerSource>();

        // Power down immedeatly 
        if(source == this.powerSource) {
            StopCoroutine("DrainPowerSupply");
            this.Deactivate();
            this.powerSource = null;
            
        }     
    }
}
