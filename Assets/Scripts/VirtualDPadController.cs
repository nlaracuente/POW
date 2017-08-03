using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles receiving and process player inputs
/// </summary>
public class VirtualDPadController : MonoBehaviour
{
    /// <summary>
    /// Holds -1, 0, 1 to represent which button is being pressed
    /// Where "Horizontal" is left[-1] / right[1]
    /// And "Vertical" is down[-1] / up[1]
    /// A value of 0 could mean both are pressed or neither is
    /// </summary>
    Dictionary<string, float> input = new Dictionary<string, float>{
        { "Horizontal", 0f },
        { "Vertical", 0f },
        { "Respawn", 0f },
        { "Recall", 0f },
        { "Action", 0f },
        { "Menu", 0f },
    };

    /// <summary>
    /// Returns the current player directional input 
    /// </summary>
    public Dictionary<string, float> Input
    {
        get
        {
            return this.input;
        }
    }

    /// <summary>
    /// Translates the Clicable.Value button into an input value
    /// </summary>
    Dictionary<Clickable.Value, int> inputValue = new Dictionary<Clickable.Value, int> {
        { Clickable.Value.DPad_Up,    1},
        { Clickable.Value.DPad_Left, -1},
        { Clickable.Value.DPad_Down, -1},
        { Clickable.Value.DPad_Right, 1},
        { Clickable.Value.Respawn,    1},
        { Clickable.Value.Recall,     1},
        { Clickable.Value.Action,     1},
        { Clickable.Value.Menu,       1},
    };

    /// <summary>
    /// When not mobile it disables the buttons
    /// </summary>
    [SerializeField]
    bool isMobile = true;
    public bool IsMobile { get { return this.isMobile; } }

    /// <summary>
    /// Register the controller to the child buttons to listed for the on pressed/released
    /// </summary>
    void Start()
    {
        foreach(Clickable clickable in GetComponentsInChildren<Clickable>()) {
            clickable.PressedEvent += this.OnButtonPressed;
            clickable.ReleasedEvent += this.OnButtonReleased;

            clickable.gameObject.SetActive(this.isMobile);
        }
    }
    
    /// <summary>
    /// Player pressed a virtual d-pad button
    /// </summary>
    /// <param name="value"></param>
    void OnButtonPressed(Clickable button)
    {
        Clickable.Value value = button.GetValue();

        // Default to Action
        string inputType = "Action";

        // Recall
        if(value == Clickable.Value.Recall) {
            inputType = "Recall";
        }

        // Respawn
        if(value == Clickable.Value.Respawn) {
            inputType = "Respawn";
        }

        // Menu
        if(value == Clickable.Value.Menu) {
            inputType = "Menu";
        }

        // Horizontal input
        if(value == Clickable.Value.DPad_Left || value == Clickable.Value.DPad_Right) {
            inputType = "Horizontal";
        } 

        // Vertical input
        if(value == Clickable.Value.DPad_Down || value == Clickable.Value.DPad_Up) {
            inputType = "Vertical";
        }

        this.input[inputType] += this.inputValue[value];
    }

    /// <summary>
    /// Player released a virtual d-pad button
    /// </summary>
    /// <param name="value"></param>
    void OnButtonReleased(Clickable button)
    {
        Clickable.Value value = button.GetValue();

        // Default to Action
        string inputType = "Action";

        // Recall
        if(value == Clickable.Value.Recall) {
            inputType = "Recall";
        }

        // Respawn
        if(value == Clickable.Value.Respawn) {
            inputType = "Respawn";
        }

        // Menu
        if(value == Clickable.Value.Menu) {
            inputType = "Menu";
        }

        // Horizontal input
        if(value == Clickable.Value.DPad_Left || value == Clickable.Value.DPad_Right) {
            inputType = "Horizontal";
        } 

        // Vertical input
        if(value == Clickable.Value.DPad_Down || value == Clickable.Value.DPad_Up) {
            inputType = "Vertical";
        }

        // Only a single button was pressed before, it is safe to zero out
        if(this.input[inputType] != 0) {
            this.input[inputType] = 0;

        // Both buttons were being pressed and now one has been released
        // Therefore we use the inverse value of the button release to get the value of the one still pressed
        } else {
             this.input[inputType] = -this.inputValue[value];
        }
    }
}
