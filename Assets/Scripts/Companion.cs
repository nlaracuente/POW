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
    float rotationSpeed = -30f;

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
    /// The body collider is the collider that detects collision
    /// We need to disable this when the companion is attached to the player
    /// so that the player does not think it is touching "solid" ground
    /// </summary>
    [SerializeField]
    Collider bodyCollider;

    /// <summary>
    /// Contains a queue of all the list that represents the total
    /// amount of power the companion has left
    /// </summary>
    Queue<GameObject> lights = new Queue<GameObject>();

    /// <summary>
    /// Initialize
    /// </summary>
    void Awake()
    {
        this.QueueLights();
        this.maxPower = this.currentPower = this.lights.Count;
        this.rigidbody = GetComponent<Rigidbody>();
        this.origin = this.rigidbody.position;
        this.levelController = FindObjectOfType<LevelController>();
    }

    /// <summary>
    /// Finds all the "lights" the companion uses to represent 
    /// </summary>
    void QueueLights()
    {
        GameObject powerGO = this.transform.FindChild("Power").gameObject;
        for(int i = 0; i < powerGO.transform.childCount; i++) {
            GameObject light = powerGO.transform.GetChild(i).gameObject;

            // Already queued
            if(this.lights.Contains(light)) {
                continue;
            }

            // Make sure it is turned on
            light.GetComponent<MeshRenderer>().enabled = true;
            this.lights.Enqueue(light);
        }
    }

    /// <summary>
    /// Spins when the is being carried to match the player's hover spin
    /// </summary>
    void Update()
    {
        // Rotate only 
        if(this.transform.parent != null) {
            this.transform.Rotate(new Vector3(0f, this.rotationSpeed * Time.deltaTime, 0f));
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
    public void PickedUp(Transform parent, Vector3 position)
    {
        this.bodyCollider.enabled = false;
        this.rigidbody.useGravity = false;
        this.transform.SetParent(parent, true);
        this.transform.position = position;
    }

    /// <summary>
    /// Player has dropped the companion
    /// Companion positions itself in the given position
    /// </summary>
    /// <param name="position"></param>
    public void Dropped(Vector3 position)
    {
        this.transform.SetParent(null);
        this.rigidbody.useGravity = true;
        this.bodyCollider.enabled = true;
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
        this.bodyCollider.enabled = true;
        this.rigidbody.velocity = Vector3.zero;
        this.rigidbody.position = this.origin;
        this.transform.position = this.origin;
    }

    /// <summary>
    /// Overrides parent so that we can disable one of the lights
    /// </summary>
    /// <param name="total"></param>
    public override void ConsumePower(int total)
    {
        base.ConsumePower(total);
        GameObject light = this.lights.Dequeue();
        light.GetComponent<MeshRenderer>().enabled = false;
    }

    /// <summary>
    /// Re-queues the lights 
    /// </summary>
    public override void Recharge()
    {
        base.Recharge();
        this.QueueLights();
    }
}
