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
    /// The direction the player is currently moving
    /// </summary>
    Vector3 moveDirection = Vector3.zero;

    /// <summary>
    /// Where the player wants to move
    /// </summary>
    Vector3 targetPosition = Vector3.zero;

    /// <summary>
    /// The last position where the player was not falling
    /// </summary>
    [SerializeField]
    Vector3 lastSafePosition = Vector3.zero;

    /// <summary>
    /// The desired rotation to apply
    /// </summary>
    Quaternion targetRotation = Quaternion.identity;

    /// <summary>
    /// When false prevents the move logic from triggering
    /// </summary>
    bool canMove = true;

    /// <summary>
    /// When false prevents the rotate logic from triggering
    /// </summary>
    bool canRotate = true;

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
    /// Initialize
    /// </summary>
    void Start()
    {
        this.animator = this.transform.FindChild("Model").GetComponent<Animator>();
        this.levelController = FindObjectOfType<LevelController>();
        this.companion = FindObjectOfType<Companion>();
        this.rigidbody = GetComponent<Rigidbody>();
        this.targetPosition = this.rigidbody.position;
        this.targetRotation = this.rigidbody.rotation;
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
        this.targetRotation = Quaternion.LookRotation(this.inputVector, Vector3.up);
        StartCoroutine("SmoothRotate", this.targetRotation);
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
    /// Calculates the target destination based on player input
    /// Triggers the SmoothMove corouting
    /// </summary>
    void Move()
    {
        // No movement required
        if(this.inputVector == Vector3.zero) {
            return;
        }

        // Ensures the Y axis remains are 0
        Vector3 curPosition = new Vector3(
            this.rigidbody.position.x,
            0f,
            this.rigidbody.position.z
        );

        // Store the current input vector as the direction
        // since the player can change it to zero while moving
        this.moveDirection = this.inputVector;
        
        Vector3 desiredPosition = curPosition + this.moveDirection * this.tileScale;

        // We want to 0 out the y and remove the floating numbers since we work with whole numbers
        this.targetPosition = new Vector3(
            (int)desiredPosition.x,
            0f,
            (int)desiredPosition.z
        );

        // As long as the path is cleared then the player can move
        bool isAvailable = this.levelController.IsDestinationAvailable(curPosition,
                                                                       this.targetPosition,
                                                                       this.moveDirection,
                                                                       this.tileScale,
                                                                       this.rayHeight,
                                                                       this.obstacleMask);
        // Move
        if(isAvailable) {
            StartCoroutine("SmoothMove", this.targetPosition);
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

        // Y position is changed during rotation so let us reset it
        this.rigidbody.position = new Vector3(
            this.rigidbody.position.x,
            0f,
            this.rigidbody.position.z
        );

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

        // Always ignore the "y" position since the player could be moving up or down
        Vector3 currentPosition = new Vector3(
            this.rigidbody.position.x,
            0f,
            this.rigidbody.position.z
        );
        
        while(Vector3.Distance(targetPosition, currentPosition) > this.distancePad) {
            Vector3 newPosition = this.rigidbody.position + this.moveDirection * this.moveSpeed * Time.fixedDeltaTime;
            this.rigidbody.MovePosition(newPosition);

            // Get new current position
            currentPosition = new Vector3(
                this.rigidbody.position.x,
                0f,
                this.rigidbody.position.z
            );
            yield return new WaitForFixedUpdate();
        }

        // Snap into position
        this.rigidbody.position = targetPosition;
        this.rigidbody.velocity = Vector3.zero;

        // Check if the new position is a save place to stand 
        // otherwise trigger a fall
        GameObject GOUnderneath = this.levelController.GetObjectUnderPosition(this.rigidbody.position, this.feetHeight, this.distanceToFloor);

        if( GOUnderneath != null ) {
            this.canMove = true;
            this.canRotate = true;

            // Only save it if it is a floor
            if(GOUnderneath.layer == LayerMask.NameToLayer("Floor")) {
                this.lastSafePosition = this.rigidbody.position;
            }
        } else {
            this.TriggerFall();
        }
    }

    /// <summary>
    /// Causes the player to fall down
    /// </summary>
    void TriggerFall()
    {
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
    /// Player enter a condition that calls for a resapwn
    /// Place the player on the last known "safe" position
    /// </summary>
    public void Respawn()
    {
        // Reset position
        this.rigidbody.useGravity = false;
        this.rigidbody.velocity = Vector3.zero;
        this.rigidbody.position = this.targetPosition = this.lastSafePosition;

        // Restore control
        this.canMove = true;
        this.canRotate = true;
    }
}
