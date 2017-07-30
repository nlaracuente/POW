using UnityEngine;

public class SimpleLookAt : MonoBehaviour
{
    /// <summary>
    /// Where to look at
    /// </summary>
    [SerializeField]
	Transform target;

    /// <summary>
    /// How fast to track the target
    /// </summary>
    [SerializeField]
    float trackSpeed = 0.3f;

    /// <summary>
    /// The current velocity at which this is moving to track the target
    /// </summary>
    Vector3 velocity = Vector3.zero;

    /// <summary>
    /// Rotate to look at the target
    /// </summary>
    void Update()
    {
        Vector3 targetPosition = this.target.position;
        targetPosition.y = this.transform.position.y;

        this.transform.position = Vector3.SmoothDamp(
            this.transform.position, 
            targetPosition, 
            ref this.velocity,
            this.trackSpeed
        );
    }
}
