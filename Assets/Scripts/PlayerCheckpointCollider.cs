using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCheckpointCollider : MonoBehaviour
{
	/// <summary>
    /// A reference to this parent's teleporter
    /// </summary>
    [SerializeField]
    ChargeStation station;

    /// <summary>
    /// Stores a reference to the parent portal
    /// </summary>
    void Start()
    {
        this.station = GetComponentInParent<ChargeStation>();
    }

    /// <summary>
    /// Notifies the connecting teleported we are sending a target over there
    /// Sets the "teleporting" flag ON to avoid double teleporting since we use a coroutine
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerStay(Collider other)
    {        
        if(other.tag == "Player") {
            other.GetComponent<Player>().UpdateCheckpoint(this.station.transform.position);
        }
    }
}
