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
    /// Initialize
    /// </summary>
    void Start()
    {
        this.rigidbody = GetComponent<Rigidbody>();
        this.targetPosition = this.transform.position;
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

        if(this.canMove) {
            this.Move();
        }

        // this.UpdateLastSafePosition();
    }

    /// <summary>
    /// Checks if the player has reached their destination
    /// If there's a floor tile underneath then this is a safe position
    /// Otherwise, trigger a fall and prevent movement and rotation
    /// </summary>
    void UpdateLastSafePosition()
    {
        
        Vector3 origin = new Vector3(this.transform.position.x, this.feetHeight, this.transform.position.z);

        // Draw the line to see where the raycast will go
        Debug.DrawLine(origin, origin + Vector3.down * this.distanceToFloor, Color.red);

        Ray ray = new Ray(origin, Vector3.down);
        RaycastHit hitInfo;

        // Floor...safe
        if( Physics.Raycast(ray, out hitInfo, this.distanceToFloor, this.floorMask) ) {
            this.lastSafePosition = new Vector3(this.transform.position.x, 0f, this.transform.position.z);

        // No floor...fall
        } else {
            this.canMove = false;
            this.canRotate = false;
            this.rigidbody.useGravity = true;
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

        this.inputVector = new Vector3(h, 0f, v);
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
            this.transform.position.x,
            0f,
            this.transform.position.z
        );

        // Store the current input vector as the direction
        // since the player can change it to zero while moving
        this.moveDirection = this.inputVector;
        this.targetPosition = curPosition + this.moveDirection * this.tileScale;

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

        while(Quaternion.Angle(this.transform.rotation, targetRotation) > this.anglePad) {

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
        this.transform.position = new Vector3(
            this.transform.position.x,
            0f,
            this.transform.position.z
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
        
        while(Vector3.Distance(targetPosition, this.transform.position) > this.distancePad) {
            Vector3 newPosition = this.rigidbody.position + this.moveDirection * this.moveSpeed * Time.fixedDeltaTime;
            this.rigidbody.MovePosition(newPosition);
            yield return new WaitForFixedUpdate();
        }

        this.rigidbody.position = targetPosition;
        this.canMove = true;
        this.canRotate = true;
    }

    /// <summary>
    /// Player enter a condition that calls for a resapwn
    /// Place the player on the last known "safe" position
    /// </summary>
    public void Respawn()
    {
        this.rigidbody.position = this.lastSafePosition;
        this.canMove = true;
        this.canRotate = true;
        this.rigidbody.useGravity = false;
        this.rigidbody.velocity = Vector3.zero;
    }
}
