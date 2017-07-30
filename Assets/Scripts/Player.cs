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
    [SerializeField]
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
    /// A reference to the companion power supply object the player can carry around
    /// </summary>
    Companion companion;

    /// <summary>
    /// Initialize
    /// </summary>
    void Start()
    {
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
                this.companion.PickedUp(this.transform);
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
        if(this.IsDestinationAvailable(curPosition, this.targetPosition, this.moveDirection, this.tileScale)) {
            StartCoroutine("SmoothMove", this.targetPosition);
        }
    }
    
    /// <summary>
    /// Returns true if the given position does not contain an obstacle
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    bool IsDestinationAvailable(Vector3 currentPosition, Vector3 targetDestination, Vector3 direction, float rayDistance)
    {
        bool isAvailable = true;

        // The origin starts at the bottom of the feet
        // We want to raise it up to waist level
        Vector3 origin = new Vector3(
            currentPosition.x,
            this.rayHeight,
            currentPosition.z
        );

        // The same thing happens with the destination
        Vector3 destination = new Vector3(
            targetDestination.x,
            this.rayHeight,
            targetDestination.z
        );

        // Draw the line to see where the raycast will go
        Debug.DrawLine(origin, destination, Color.red);
        
        Ray ray = new Ray(origin, direction);
        RaycastHit hitInfo;
        if(Physics.Raycast(ray, out hitInfo, rayDistance, this.obstacleMask)) {
            isAvailable = false;
        }

        return isAvailable;
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
        if( this.IsGrounded() ) {
            this.canMove = true;
            this.canRotate = true;
            this.lastSafePosition = this.rigidbody.position;
        } else {
            this.TriggerFall();
        }
    }

    /// <summary>
    /// Returns true if the player is standing on a surface they can stand on
    /// </summary>
    /// <returns></returns>
    bool IsGrounded()
    {
        bool isGrounded = false;
        Vector3 origin = new Vector3(this.rigidbody.position.x, this.feetHeight, this.rigidbody.position.z);

        // Draw the line to see where the raycast will go
        Debug.DrawLine(origin, origin + Vector3.down * this.distanceToFloor, Color.red);

        Ray ray = new Ray(origin, Vector3.down);
        RaycastHit hitInfo;

        // Is standing on something
        if(Physics.Raycast(ray, out hitInfo, this.distanceToFloor)) {
            isGrounded = true;
        }

        return isGrounded;
    }

    /// <summary>
    /// Causes the player to fall down
    /// </summary>
    void TriggerFall()
    {
        this.canMove = false;
        this.canRotate = false;
        this.rigidbody.useGravity = true;
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
