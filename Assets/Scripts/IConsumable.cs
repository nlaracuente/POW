using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Objects that consume power in order to operate
/// </summary>
public interface IConsumable
{
    /// <summary>
    /// Triggers the object to attempt to consume power from the source
    /// in order to activate itself
    /// </summary>
    void Activate();

    /// <summary>
    /// Power source is no longer within range thus deactivating this object 
    /// </summary>
    void Deactivate();
}
