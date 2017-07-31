using UnityEngine;

public class BombTile : Consumable
{
    /// <summary>
    /// When true it will check if the player is within range and inflict damage
    /// </summary>
    bool inflictDamage = false;

    /// <summary>
    /// Prevents damage from being inflicted more than once
    /// </summary>
    bool damageInflicted = false;

    /// <summary>
    /// How fast to rate 
    /// </summary>
    [SerializeField]
    float rotationSpeed = 8f;

    /// <summary>
    /// References the bomb transform to rotate only it and not the entire object
    /// </summary>
    [SerializeField]
    Transform bombTransform;

    /// <summary>
    /// The script containing the trigger collider that allows us to inflict damage
    /// </summary>
    [SerializeField]
    BombDamageCollider damageCollider;

    /// <summary>
    /// Rotate while damage has not activated 
    /// </summary>
    void LateUpdate()
    {
        if(!this.isActivated) {
            this.bombTransform.Rotate(new Vector3(0f, this.rotationSpeed * Time.deltaTime, 0f));
        }
    }

    /// <summary>
    /// We are good to inflict damage
    /// </summary>
    public void InflictDamage()
    {
        if(this.damageCollider != null) { 
            this.damageCollider.canInflictDamage = true;
        }
    }
}
