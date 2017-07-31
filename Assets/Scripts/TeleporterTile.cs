using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Triggers a teleport to the associated teleporter
/// </summary>
public class TeleporterTile : Consumable
{
    /// <summary>
    /// Material that represents the portal is active
    /// </summary>
    [SerializeField]
    Material activeMaterial;

    /// <summary>
    /// Material that represents the portal is not active
    /// </summary>
    [SerializeField]
    Material deactiveMaterial;

    /// <summary>
    /// A reference to the mesh renderer where we can change the material
    /// </summary>
    [SerializeField]
    MeshRenderer meshRenderer;

    /// <summary>
    /// Updates the texture to match its current state
    /// </summary>
    void LateUpdate()
    {
        if(this.isActivated) {
            this.meshRenderer.material = this.activeMaterial;
        } else {
            this.meshRenderer.material = this.deactiveMaterial;
        }
    }
}
