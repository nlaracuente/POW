using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the actions that take place within the level
/// </summary>
public class LevelController : MonoBehaviour
{
    /// <summary>
    /// Returns true if the given position does not contain an obstacle
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public bool IsDestinationAvailable(Vector3 currentPosition, Vector3 targetDestination, Vector3 direction, float rayDistance, float rayHeight, LayerMask mask)
    {
        bool isAvailable = true;

        // The origin starts at the bottom of the feet
        // We want to raise it up to waist level
        Vector3 origin = new Vector3(
            currentPosition.x,
            currentPosition.y + rayHeight,
            currentPosition.z
        );

        // The same thing happens with the destination
        Vector3 destination = new Vector3(
            targetDestination.x,
            targetDestination.y + rayHeight,
            targetDestination.z
        );

        // Draw the line to see where the raycast will go
        Debug.DrawLine(origin, destination, Color.red);
        
        Ray ray = new Ray(origin, direction);
        RaycastHit hitInfo;
        if(Physics.Raycast(ray, out hitInfo, rayDistance, mask)) {
            isAvailable = false;
        }

        return isAvailable;
    }

    /// <summary>
    /// Returns true if the position given is over a tile that can be walked on
    /// </summary>
    /// <param name="position"></param>
    /// <param name="rayStart"></param>
    /// <param name="rayEnd"></param>
    /// <returns></returns>
    public GameObject GetObjectUnderPosition(Vector3 position, float rayStart, float rayEnd)
    {
        GameObject GO = null;
        Vector3 origin = new Vector3(position.x, position.y + rayStart, position.z);

        // Draw the line to see where the raycast will go
        Debug.DrawLine(origin, origin + Vector3.down * rayEnd, Color.red);

        Ray ray = new Ray(origin, Vector3.down);
        RaycastHit hitInfo;

        // Is standing on something
        if(Physics.Raycast(ray, out hitInfo, rayEnd)) {
            GO = hitInfo.collider.gameObject;
        }

        return GO;
    }
}
