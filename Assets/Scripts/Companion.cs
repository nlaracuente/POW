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
    [Range(0, 100)]
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
    Vector3 respawnPoint;

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
    /// A reference to the fan game object which spins
    /// based on the total current power the companion has
    /// </summary>
    [SerializeField]
    GameObject fanGO;

    /// <summary>
    /// Material color to use when the light is on
    /// </summary>
    [SerializeField]
    Material lightOnMaterial;

    /// <summary>
    /// Material color to use when the light off
    /// </summary>
    [SerializeField]
    Material lightOffMaterial;

    /// <summary>
    /// Contains a queue of all the list that represents the total
    /// amount of power the companion has left
    /// </summary>
    Queue<GameObject> lights = new Queue<GameObject>();

    /// <summary>
    /// A reference to the "transform" to follow
    /// This is so that the companion can "follow" the player
    /// when picked up
    /// </summary>
    Transform targetToFollow;

    /// <summary>
    /// How fast to follow the target
    /// </summary>
    [SerializeField]
    float followSpeed = 8f;

    /// <summary>
    /// How close to get to the target before snapping into place
    /// </summary>
    [SerializeField]
    float distanceToTarget = 0.05f;

    /// <summary>
    /// A reference to the player script
    /// </summary>
    Player player;

    /// <summary>
    /// Holds a reference to the menu
    /// </summary>
    MenuCanvas menu;

    /// <summary>
    /// Pitch level to use when draining
    /// </summary>
    [SerializeField]
    float drainPitch = 1f;

    /// <summary>
    /// Initialize
    /// </summary>
    void Awake()
    {
        this.menu = FindObjectOfType<MenuCanvas>();
        this.player = FindObjectOfType<Player>();
        this.QueueLights(false);
        this.maxPower = this.lights.Count;
        this.rigidbody = GetComponent<Rigidbody>();
        this.respawnPoint = this.rigidbody.position;
        this.levelController = FindObjectOfType<LevelController>();
    }

    /// <summary>
    /// Finds all the "lights" the companion uses to represent 
    /// </summary>
    void QueueLights(bool turnedOn = true)
    {
        // This list is used to reverse the order before creating the queue
        List<GameObject> lightsGO = new List<GameObject>();

        GameObject powerGO = this.transform.Find("Power").gameObject;
        for(int i = 0; i < powerGO.transform.childCount; i++) {
            GameObject light = powerGO.transform.GetChild(i).gameObject;

            // Make sure it is turned on
            if(turnedOn) {
                light.GetComponent<MeshRenderer>().material = this.lightOnMaterial;
            } else {
                light.GetComponent<MeshRenderer>().material = this.lightOffMaterial;
            }
            
            lightsGO.Add(light);
        }

        lightsGO.Reverse();
        this.lights = new Queue<GameObject>(lightsGO);
    }

    /// <summary>
    /// Spins when the is being carried to match the player's hover spin
    /// </summary>
    void Update()
    {
        // Rotate only when there's power left
        if(this.HasPower) {
            this.fanGO.transform.Rotate(new Vector3(0f, this.rotationSpeed * this.currentPower * Time.deltaTime, 0f));
        }
    }

    /// <summary>
    /// If not currently being carried and there's no floor beneath
    /// then trigger a fall
    /// </summary>
    void FixedUpdate()
    {
        if(this.targetToFollow == null) {
            bool isGrounded = this.levelController.GetObjectUnderPosition(this.rigidbody.position, this.rayStart, this.rayEnd, 0);

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
        AudioManager.instance.PlaySound(AudioManager.SoundName.CompanionPickedUp);
        this.bodyCollider.enabled = false;
        this.rigidbody.useGravity = false;
        this.targetToFollow = parent;
        this.player.isCarryingCompanion = true;
        this.player.companionHasBeenPickedup = true;
        StartCoroutine("AnimatePickup");
    }

    /// <summary>
    /// Moves the companion until it is close enough to the targe to follow
    /// When close enough, sets the target as the parent
    /// </summary>
    /// <returns></returns>
    IEnumerator AnimatePickup()
    {
        while(Vector3.Distance(this.targetToFollow.position, this.transform.position) > this.distanceToTarget) {
            this.transform.position = Vector3.MoveTowards(this.transform.position, targetToFollow.position, this.followSpeed * Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
        }

        this.player.companionIsAttached = true;        
        //this.transform.SetParent(this.targetToFollow);        
    }

    /// <summary>
    /// Player has dropped the companion
    /// Companion positions itself in the given position
    /// </summary>
    /// <param name="position"></param>
    public void Dropped()
    {
        StopCoroutine("AnimatePickup");
        this.player.companionIsAttached = false;
        AudioManager.instance.PlaySound(AudioManager.SoundName.CompanionDropped);
        this.rigidbody.useGravity = true;
        this.bodyCollider.enabled = true;
        this.targetToFollow = null;
        this.player.isCarryingCompanion = false;
        this.transform.SetParent(null);
    }

    /// <summary>
    /// Player is recalling the companion which consumes one power point
    /// and acts as if the player has picked up the companion
    /// </summary>
    /// <param name="destination"></param>
    public void Recalled(Transform parent)
    {
        AudioManager.instance.PlaySound(AudioManager.SoundName.CompanionRecall);
        this.transform.position = this.player.transform.position;
    }

    /// <summary>
    /// Deattaches itself rom the player
    /// Enables gravity to make the companion fall
    /// </summary>
    public void TriggerFall()
    {
        StopCoroutine("AnimatePickup");
        this.isFalling = true;
        this.targetToFollow = null;
        this.rigidbody.useGravity = true;
    }

    /// <summary>
    /// Places the companion back to its original place
    /// Disables gravity to prevent falling as this has no solid colliders
    /// </summary>
    public void Respawn()
    {
        // Reset position
        StopCoroutine("AnimatePickup");
        this.targetToFollow = null;
        this.isFalling = false;
        this.rigidbody.useGravity = false;
        this.bodyCollider.enabled = true;
        this.rigidbody.velocity = Vector3.zero;
        this.rigidbody.position = this.respawnPoint;
        this.transform.position = this.respawnPoint;
    }

    /// <summary>
    /// Turns off all the lights and drains the reamining power
    /// </summary>
    public override void DrainAllPower()
    {
        base.DrainAllPower();
        while(this.lights.Count > 0) {
            GameObject lightGO = this.lights.Dequeue();
            MeshRenderer lightRenderer = lightGO.GetComponent<MeshRenderer>();
            lightRenderer.material = this.lightOffMaterial;
        }
    }

    /// <summary>
    /// Overrides parent so that we can disable one of the lights
    /// </summary>
    /// <param name="total"></param>
    public override void ConsumePower(int total)
    {
        // Don't consume while the menu is opened
        if(this.menu.isMenuOpened) {
            return;
        }

        base.ConsumePower(total);

        // There may be a chance that the recall and the last drain supply
        // coroutine drained the last light already so we will not try to
        // drain unless we still have lights
        if(this.lights.Count > 0) {            
            // Determine the pitch based on the total remaining lights
            float percent = ((float)this.lights.Count / (float)this.maxPower) * 100f;
            float pitch = 1f;

            // Set the pitch based on the percentage of "lights" left
            if(percent < 75f) {
                pitch = 1.05f;
            }

            if(percent < 50f) {
                pitch = 1.10f;
            }

            if(percent < 40f) {
                pitch = 1.15f;
            }

            if(percent < 25f) {
                pitch = 1.20f;
            }

            if(percent < 15f) {
                pitch = 1.25f;
            }

            AudioManager.instance.PlaySound(
                AudioManager.SoundName.CompanionDrain,
                pitch
            );
            GameObject lightGO = this.lights.Dequeue();
            MeshRenderer lightRenderer = lightGO.GetComponent<MeshRenderer>();
            lightRenderer.material = this.lightOffMaterial;
        }

        // Out of energy power down
        if(!this.HasPower) { 
            AudioManager.instance.PlaySound(AudioManager.SoundName.CompanionPowerDown);
        }
    }

    /// <summary>
    /// Re-queues the lights 
    /// </summary>
    public override void Recharge()
    {
        // Only when not following a target
        // Already full
        if(this.targetToFollow != null || this.currentPower == this.maxPower) {
            return;
        }

        AudioManager.instance.PlaySound(AudioManager.SoundName.CompanionCharge);
        base.Recharge();
        this.QueueLights();
    }

    /// <summary>
    /// Updates the respawn point so that next time the respawn is called
    /// this is where the companion will respawn
    /// </summary>
    public void UpdateRespawnPoint(Vector3 point)
    {
        this.respawnPoint = point;
    }
}
