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
    /// True if this object consumes power once and it is done
    /// It cannot be deactivated or reactivated
    /// </summary>
    [SerializeField]
    bool isOneTimeConsumable = false;

    /// <summary>
    /// True when the object is actively consuming power
    /// </summary>
    [SerializeField]
    protected bool isActivated = false;

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
    /// Checks if there's a power source nearby and tries to consume it
    /// </summary>
    public void Update()
    {
        if(this.powerSource != null) {
            this.ConsumePowerSource();
        } else {
            this.Deactivate();
        }
    }

    /// <summary>
    /// Activates itself so long as the power source has power
    /// </summary>
    /// <param name="source"></param>
    protected void ConsumePowerSource()
    {
        // Still has juice
        if(this.powerSource.HasPower) {
            this.Activate();

        // No enough to power up this consumable
        } else {
            this.Deactivate();
        }
    }

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
        // Can't do this for single use consumables
        if(this.isOneTimeConsumable) {
            return;
        }

        if(!this.isDeactivated ) {
            this.isActivated = false;
            this.isDeactivated = true;

            if(this.animator != null) {
                this.animator.SetTrigger("Deactivate");
            }
        }
    }    

    /// <summary>
    /// Grabs the nearest powersource that it comes in conctact 
    /// </summary>
    void OnTriggerStay(Collider other)
    {
        PowerSource source = other.GetComponent<PowerSource>();
        if(source != null) {
            this.powerSource = source;
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
        
        if(source == this.powerSource) {
            this.powerSource = null;
        }     
    }

    /// <summary>
    /// Destroys this consumable in half a second
    /// Called by the animator controller when the consumable is done being active
    /// </summary>
    public void DestroyConsumable()
    {
        Destroy(this.gameObject, 0.5f);
    }
}
