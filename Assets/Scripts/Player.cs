using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls the player movement, rotation, and actions
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour
{
    /// <summary>
    /// How fast the player moves
    /// </summary>
    [SerializeField]
    float moveSpeed = 15f;

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
    /// The direction the player wants to move
    /// </summary>
    Vector3 inputVector = Vector3.zero;

    /// <summary>
    /// Where the player wants to move
    /// </summary>
    Vector3 targetPosition = Vector3.zero;

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
        this.Rotate();
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
    /// Uses the input vector to determine if the player is facing in the desired move direction
    /// Rotates the rigidbody until it is close enough to snap into rotation
    /// </summary>
    void Rotate()
    {
        // Wait until rotation is done
        if(this.rigidbody.rotation != this.targetRotation) {
            return;
        }

        // Cannot rotate when the input vector is Zero
        if(this.inputVector == Vector3.zero) {
            return;
        }

        // Calculate and trigger smooth rotate to target
        this.targetRotation = Quaternion.LookRotation(this.inputVector, Vector3.up);
        StartCoroutine("SmoothRotate", this.targetRotation);
    }

    /// <summary>
    /// Continues to moves towards the given target rotation until close enough
    /// Snaps into the desire rotation once reached
    /// </summary>
    /// <param name="targetRotation"></param>
    /// <returns></returns>
    IEnumerator SmoothRotate(Quaternion targetRotation)
    {
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
    }
}
