using UnityEngine;

/// <summary>
/// Detects when objects have fallen from the map
/// </summary>
public class DeathPlane : MonoBehaviour
{
    /// <summary>
    /// Detects an object that enter it and respawns it
    /// </summary>
    /// <param name="other"></param>
	void OnTriggerStay(Collider other)
    {
        IRespawnable respawnable = other.gameObject.GetComponent<IRespawnable>();
        if(respawnable != null) {
            respawnable.Respawn();
        }
    }
}