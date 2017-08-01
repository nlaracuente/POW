using UnityEngine;

/// <summary>
/// Bombs are unique in that their effect is instance and can happened even when not active
/// Players can walk into the bomb not carrying the companion and trigger it if walking into
/// the bomb or active it when one tile away if they have the companion and it has power
/// </summary>
public class BombTile : Consumable
{
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
    /// True to avoid triggering the bomb more than once
    /// </summary>
    internal bool isTriggered = false;

    /// <summary>
    /// A reference to the player script
    /// </summary>
    Player player;

    /// <summary>
    /// Initialize
    /// </summary>
    void Start()
    {
        this.player = FindObjectOfType<Player>();
    }

    /// <summary>
    /// Rotate the bomb
    /// Disables the bomb triggered flag if the bomb is not active
    /// </summary>
    void LateUpdate()
    {
        if(this.isDeactivated) {
            this.isTriggered = false;
        }

        this.bombTransform.Rotate(new Vector3(0f, this.rotationSpeed * Time.deltaTime, 0f));
    }

    /// <summary>
    /// Notify the player to take damage
    /// </summary>
    public override void Activate()
    {
        base.Activate();
        this.player.TakeDamage();
    }
}
