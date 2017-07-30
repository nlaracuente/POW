using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The companion is the power source the player gets to pickup,
/// carry around, and drop to activate consumables
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class Companion : PowerSource, IRespawnable
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
    /// A reference to the rigidbody component
    /// </summary>
    new Rigidbody rigidbody;

    /// <summary>
    /// Where the companion was first placed
    /// </summary>
    [SerializeField]
    Vector3 origin;

    /// <summary>
    /// A reference to the level controller
    /// </summary>
    LevelController levelController;

    /// <summary>
    /// Where the ray start when checking for ground
    /// </summary>
    [SerializeField]
    float rayStart = 1.5f;

    /// <summary>
    /// How far to cast the ray when checking for ground
    /// </summary>
    [SerializeField]
    float rayEnd = 2f;
    private bool isFalling;

    /// <summary>
    /// Initialize
    /// </summary>
    void Start()
    {
        this.rigidbody = GetComponent<Rigidbody>();
        this.origin = this.rigidbody.position;
        this.levelController = FindObjectOfType<LevelController>();
    }

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
    /// If not currently being carried and there's no floor beneath
    /// then trigger a fall
    /// </summary>
    void FixedUpdate()
    {
        if(this.transform.parent == null) {
            bool isGrounded = this.levelController.GetObjectUnderPosition(this.rigidbody.position, this.rayStart, this.rayEnd);

            if(!isGrounded && !this.isFalling) {
                this.TriggerFall();
            }
        }
    }

    /// <summary>
    /// Player picked up the companion
    /// Companion child itself to the given parent
    /// </summary>
    public void PickedUp(Transform parent)
    {
        this.transform.SetParent(parent, true);
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

    /// <summary>
    /// Deattaches itself rom the player
    /// Enables gravity to make the companion fall
    /// </summary>
    public void TriggerFall()
    {
        this.isFalling = true;
        this.transform.SetParent(null);
        this.rigidbody.useGravity = true;
    }

    /// <summary>
    /// Places the companion back to its original place
    /// Disables gravity to prevent falling as this has no solid colliders
    /// </summary>
    public void Respawn()
    {
        // Reset position
        this.isFalling = false;
        this.rigidbody.useGravity = false;
        this.rigidbody.velocity = Vector3.zero;
        this.rigidbody.position = this.origin;
        this.transform.position = this.origin;
    }
}
