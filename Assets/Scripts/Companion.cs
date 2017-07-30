using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The companion is the power source the player gets to pickup,
/// carry around, and drop to activate consumables
/// </summary>
public class Companion : PowerSource
{
    /// <summary>
    /// How much power is required to recall the companion
    /// </summary>
    [SerializeField]
    [Range(1, 100)]
    int recallCost = 1;

    /// <summary>
    /// How fast to spin the companion when it has power
    /// </summary>
    [SerializeField]
    [Range(1, 100)]
    float rotationSpeed = 10f;

    /// <summary>
    /// Spins the companion based on the current power to indicate  
    /// how much power it has left
    /// </summary>
    void Update()
    {
        if(this.currentPower > 0) {
            this.transform.Rotate(new Vector3(0f, this.rotationSpeed * this.currentPower * Time.deltaTime, 0f));
        } else {
            this.transform.rotation = Quaternion.identity;
        }
        
    }

    /// <summary>
    /// Player picked up the companion
    /// Companion child itself to the given parent
    /// </summary>
    public void PickedUp(Transform parent)
    {
        this.transform.SetParent(parent, false);
    }

    /// <summary>
    /// Player has dropped the companion
    /// Companion positions itself in the given position
    /// </summary>
    /// <param name="position"></param>
    public void Dropped(Vector3 position)
    {
        this.transform.SetParent(null);
        this.transform.position = position;
    }

    /// <summary>
    /// Companion is being recalled to the given destination
    /// Companion will move to destination only if the amount 
    /// of power needed to recall
    /// </summary>
    /// <param name="destination"></param>
    public void Recalled(Vector3 destination)
    {

    }

}
