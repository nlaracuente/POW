using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinTile : MonoBehaviour
{
    /// <summary>
    /// Holds a reference to the menu
    /// </summary>
    MenuCanvas menu;

    /// <summary>
    /// Init
    /// </summary>
    void Start()
    {
        this.menu = FindObjectOfType<MenuCanvas>();
    }

    /// <summary>
    /// Game won!
    /// </summary>
    void OnTriggerEnter()
    {
        this.menu.OpenCreditsMenu();
    }
}
