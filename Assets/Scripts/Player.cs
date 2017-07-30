using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls the player movement, rotation, and actions
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour, IRespawnable
{
    /// <summary>
    /// The scale of the floor tile in unity's unit
    /// This is to control the distance the player moves
    /// </summary>
    [SerializeField]
    float tileScale = 1f;

    /// <summary>
    /// How fast the player moves
    /// </summary>
    [SerializeField]
    float moveSpeed = 5f;

    /// <summary>
    /// How fast the player rotates
    /// </summary>
    [SerializeField]
    float rotateSpeed = 8f;

    /// <summary>
    /// How close to the destination before the snapping into place
    /// </summary>
    [SerializeField]
    float distancePad = 0.05f;

    /// <summary>
    /// How close to the desired angle of rotation before snapping to the angle
    /// </summary>
    [SerializeField]
    float anglePad = 5f;

    /// <summary>
    /// Where to position the ray when checking for obstacles
    /// </summary>
    [SerializeField]
    float rayHeight = 1.5f;

    /// <summary>
    /// Where to position the ray when checking for floor
    /// </summary>
    [SerializeField]
    float feetHeight = .25f;

    /// <summary>
    /// LayerMask for objects the player cannot walk through
    /// </summary>
    [SerializeField]
    LayerMask obstacleMask;

    /// <summary>
    /// The layer associated with the floor
    /// </summary>
    [SerializeField]
    LayerMask floorMask;

    /// <summary>
    /// The direction the player wants to move
    /// </summary>
    Vector3 inputVector = Vector3.zero;

    /// <summary>
    /// The last position where the player was not falling
    /// </summary>
    [SerializeField]
    Vector3 lastSafePosition = Vector3.zero;

    /// <summary>
    /// When false prevents the move logic from triggering
    /// </summary>
    [SerializeField]
    bool canMove = true;

    /// <summary>
    /// When false prevents the rotate logic from triggering
    /// </summary>
    [SerializeField]
    bool canRotate = true;

    /// <summary>
    /// True when the player is falling
    /// </summary>
    [SerializeField]
    bool isFalling = false;

    /// <summary>
    /// A reference to the rigidbody component
    /// </summary>
    new Rigidbody rigidbody;

    /// <summary>
    /// How far to cast the ray when checking for the floor
    /// </summary>
    [SerializeField]
    float distanceToFloor;

    /// <summary>
    /// Keeps a list of buttons being pressed
    /// Used to trigger events only once per presses
    /// </summary>
    List<string> buttonsPressed = new List<string>();

    /// <summary>
    /// True when the player is carrying the companion
    /// </summary>
    bool isCarryingCompanion = false;

    /// <summary>
    /// The speed to play the player's hover animation while carrying the companion
    /// </summary>
    [SerializeField]
    float carryingAnimSpeed = 0.5f;

    /// <summary>
    /// A reference to the companion power supply object the player can carry around
    /// </summary>
    Companion companion;

    /// <summary>
    /// Where to child the companion to when it is picked up
    /// </summary>
    [SerializeField]
    Transform companionParent;

    /// <summary>
    /// A reference to the levelController component
    /// </summary>
    LevelController levelController;

    /// <summary>
    /// A reference to the animator component
    /// </summary>
    [SerializeField]
    Animator animator;

    /// <summary>
    /// True when the player has taken damage
    /// This helps stop movement
    /// </summary>
    bool isDamaged;

    /// <summary>
    /// Initialize
    /// </summary>
    void Start()
    {
        this.animator = this.transform.FindChild("Model").GetComponent<Animator>();
        this.levelController = FindObjectOfType<LevelController>();
        this.companion = FindObjectOfType<Companion>();
        this.rigidbody = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Store the player's input
    /// </summary>
    void Update()
    {
        this.SavePlayerInput();

        if(this.isCarryingCompanion) {
            this.animator.speed = this.carryingAnimSpeed;
        } else {
            this.animator.speed = 1f;
        }
    }

    /// <summary>
    /// Update the rigidbody's position and rotation
    /// </summary>
    void LateUpdate()
    {
        if(this.canRotate) {
            this.Rotate();
        }

        // Player can move which means they can pickup/drop off item now
        if(this.canMove) {
            if(this.IsButtonPressed("Action")) {
                this.ToggleCompanionAction();
            }
            this.Move();
        }

        // Always check the player is grounded
        this.CheckIsGrounded();
    }

    /// <summary>
    /// Checks if the player is still grounded triggering a "fall" when they are not
    /// </summary>
    void CheckIsGrounded()
    {
        // Check if the new position is a save place to stand 
        // otherwise trigger a fall
        GameObject GOUnderneath = this.levelController.GetObjectUnderPosition(this.transform.position, 
                                                                              this.feetHeight, 
                                                                              this.distanceToFloor);

        if( GOUnderneath != null ) {
            // Only save it if it is a floor
            if(GOUnderneath.GetComponent<FloorTile>() != null) {
                
                // Ground found while falling
                // Stop falling and set the player on the tile
                if(this.isFalling) {
                    this.isFalling = false;
                    this.rigidbody.useGravity = false;
                    this.transform.position = GOUnderneath.transform.position;
                }

                this.lastSafePosition = this.transform.position;
            }

        // Begin fall if the player is done moving
        // As this is a grid-based movement we want the player to "snap" into place before falling
        } else if(!this.isFalling && this.canMove){
            Debug.Log("Fall");
            this.isFalling = true;
            this.TriggerFall();
        }
    }

    /// <summary>
    /// Calculate the player's input
    /// Save them into the input vector
    /// </summary>
    void SavePlayerInput()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // Prioritize horizontal
        if(h != 0f && v != 0f) {
            v = 0f;
        }

        Vector3 targetDirection = new Vector3(h, 0f, v);
        this.inputVector = targetDirection;
    }

    /// <summary>
    /// Returns True if the butons is pressed but only the frist time
    /// </summary>
    /// <param name="buttonName"></param>
    /// <returns></returns>
    bool IsButtonPressed(string buttonName)
    {
        bool isPressed = false;

        if(Input.GetAxisRaw(buttonName) > 0) {
            // Not already pressed, trigger action
            if(!this.buttonsPressed.Contains(buttonName)) {
                isPressed = true;
                this.buttonsPressed.Add(buttonName);
            }
        } else {
            this.buttonsPressed.Remove(buttonName);
        }

        return isPressed;
    }

    /// <summary>
    /// Calculates the target rotation based on player input
    /// Triggers the SmoothRotate coroutine
    /// </summary>
    void Rotate()
    {
        // Cannot rotate when the input vector is Zero
        if(this.inputVector == Vector3.zero) {
            return;
        }

        // Calculate and trigger smooth rotate to target
        Quaternion targetRotation = Quaternion.LookRotation(this.inputVector, Vector3.up);
        StartCoroutine("SmoothRotate", targetRotation);
    }

    /// <summary>
    /// Toggles between dropping or picking up the companion
    /// </summary>
    void ToggleCompanionAction()
    {
        // Player's current position
        Vector3 playerPosition = new Vector3(
            this.rigidbody.position.x,
            0f,
            this.rigidbody.position.z
        );

        // Try to pick up the companion
        if(!this.isCarryingCompanion) {
            // Companion's current position
            Vector3 companionPosition = new Vector3(
                this.companion.transform.position.x,
                0f,
                this.companion.transform.position.z
            );
            
            // Player is on top of the companion 
            // which means they can pick it up
            if(companionPosition == playerPosition) {
                this.isCarryingCompanion = true;
                this.companion.PickedUp(this.transform, this.companionParent.position);
            }

        // Player dropped the companion
        } else {
            this.isCarryingCompanion = false;
            this.companion.Dropped(playerPosition);
        }
    }

    /// <summary>
    /// Checks if the position in the desired input vector is available
    /// as in there's no obstacle in the way and triggers a smooth move
    /// towards the destination
    /// </summary>
    void Move()
    {
        // No movement required
        if(this.inputVector == Vector3.zero) {
            return;
        }        
              
        // Where the player wants to go
        Vector3 desiredPosition = this.transform.position + this.inputVector * this.tileScale;

        // As long as the path is cleared then the player can move
        bool isAvailable = this.levelController.IsDestinationAvailable(this.transform.position,
                                                                       desiredPosition,
                                                                       this.inputVector,
                                                                       this.tileScale,
                                                                       this.rayHeight,
                                                                       this.obstacleMask);
        // Move
        if(isAvailable) {
            StartCoroutine("SmoothMove", desiredPosition);
        }
    }

    /// <summary>
    /// Continues to moves towards the given target rotation until close enough
    /// Snaps into the desire rotation once reached
    /// </summary>
    /// <param name="targetRotation"></param>
    /// <returns></returns>
    IEnumerator SmoothRotate(Quaternion targetRotation)
    {
        // Must wait until rotation is done to move and/or change rotation
        this.canMove = false;
        this.canRotate = false;

        while(Quaternion.Angle(this.rigidbody.rotation, targetRotation) > this.anglePad) {

            Quaternion newRotation = Quaternion.Lerp(
                this.rigidbody.rotation, 
                targetRotation, 
                this.rotateSpeed * Time.fixedDeltaTime
            );

            // Apply the rotation
            this.rigidbody.MoveRotation(newRotation);
            yield return new WaitForFixedUpdate();
        }

        // Snap into place
        this.rigidbody.rotation = targetRotation;
        this.canMove = true;
        this.canRotate = true;
    }

    /// <summary>
    /// Moves the rigidbody to the target position
    /// </summary>
    /// <param name="targetPosition"></param>
    /// <returns></returns>
    IEnumerator SmoothMove(Vector3 targetPosition)
    {
        // Must wait until movement is done to move and/or change rotation
        this.canMove = false;
        this.canRotate = false;
        
        // Continue to move until destination is reached
        while(Vector3.Distance(this.transform.position, targetPosition) > this.distancePad) {
            Vector3 destination = Vector3.MoveTowards(this.transform.position, targetPosition, this.moveSpeed * Time.deltaTime);
            this.transform.position = destination;
            yield return new WaitForEndOfFrame();
        }

        // Snap into position
        this.transform.position = targetPosition;
        this.canMove = true;
        this.canRotate = true;

        // Make sure that the player is still grounded
        this.CheckIsGrounded();
    }

    /// <summary>
    /// Causes the player to fall down
    /// Stops all coroutine to ensure no movement is happening other than falling
    /// </summary>
    void TriggerFall()
    {
        this.StopAllCoroutines();
        this.canMove = false;
        this.canRotate = false;
        this.rigidbody.useGravity = true;

        // Apply the same to the companion
        if(this.isCarryingCompanion) {
            this.isCarryingCompanion = false;
            this.companion.TriggerFall();
        }
    }

    /// <summary>
    /// Called by any object that can damage the player
    /// Cancels player movement and triggers a respawn
    /// </summary>
    public void PlayerDamaged()
    {
        StopAllCoroutines();
        this.canMove = false;
        this.canRotate = false;
        this.Respawn();
    }

    /// <summary>
    /// Player enter a condition that calls for a resapwn
    /// Place the player on the last known "safe" position
    /// </summary>
    public void Respawn()
    {
        // Reset position
        this.rigidbody.useGravity = false;
        this.rigidbody.velocity = Vector3.zero;
        this.rigidbody.position = this.lastSafePosition;

        // Restore control
        this.canMove = true;
        this.canRotate = true;
    }
}
