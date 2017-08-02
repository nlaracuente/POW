using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// UIButtons notify the associated controller when a button
/// has been pressed or released. Controllers are associated 
/// based on the type of the button. The button itself is 
/// passed to the controller to interprit the action
/// </summary>
public class Clickable : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    /// <summary>
    /// Delgates that this clickable object will dispatch
    /// </summary>
    /// <param name="value">Value of the object clicked</param>
    public delegate void PressedDelegate(Clickable button);
    public delegate void ReleasedDelegate(Clickable button);

    /// <summary>
    /// Registered listeners
    /// </summary>
    public event PressedDelegate PressedEvent;
    public event ReleasedDelegate ReleasedEvent;

    /// <summary>
    /// The value of this clickable object
    /// </summary>
    public enum Value
    {
        // Directional Pad Inputs
        DPad_Up,
        DPad_Left,
        DPad_Down,
        DPad_Right,

        // Warp to the last check point
        Respawn,
        Recall,
        Action,
        Menu,
    };

    /// <summary>
    /// Which clickable item this is
    /// </summary>
    [SerializeField]
    Value value;
    
    /// <summary>
    /// A reference to the image that represents this button
    /// </summary>
    Image buttonImage;

    /// <summary>
    /// Stores all references
    /// </summary>
    void Start()
    {
        this.buttonImage = GetComponent<Image>();
    } // Start

    /// <summary>
    /// Returns True when the point of contact was made within the bounds
    /// of this button
    /// </summary>
    /// <param name="ped">Information about the point of contact</param>
    /// <returns>True: interaction occured on this button</returns>
    bool InBounds(PointerEventData ped)
    {
        // Holds the position of the touch
        Vector2 pos;

        // Check if the touch is within the bounds on the background image
        bool inBound = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            this.buttonImage.rectTransform,
            ped.position,
            ped.pressEventCamera,
            out pos
        );

        return inBound;
    } // InBounds

	/// <summary>
    /// Called once when the touch input is detected
    /// </summary>
    /// <param name="ped">The input information</param>
	public virtual void OnPointerDown(PointerEventData ped)
    {
        // Ignores interactions outside of this button
        if( ! this.InBounds(ped) ) {
            return;
        }
        
        if(this.PressedEvent != null) {
            this.PressedEvent(this);
        }
    } // OnPointerDown

    /// <summary>
    /// Called once when the touch input is no longer touching
    /// </summary>
    /// <param name="ped"></param>
    public virtual void OnPointerUp(PointerEventData ped)
    {
        // Ignores interactions outside of this button
        if( ! this.InBounds(ped) ) {
            return;
        }

        if(this.ReleasedEvent != null) {
            this.ReleasedEvent(this);
        }
    } // OnPointerUp

    /// <summary>
    /// Returns the value of this clickable item
    /// </summary>
    public virtual Value GetValue()
    {
        return this.value;
    }
} // class