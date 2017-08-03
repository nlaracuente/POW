using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Triggers the victory screen
/// </summary>
public class WinTileTrigger : MonoBehaviour
{
    /// <summary>
    /// Holds a reference to the menu
    /// </summary>
    MenuCanvas menu;

    /// <summary>
    /// True when the menu is opened
    /// </summary>
    bool isMenuOpened = false;

    /// <summary>
    /// Init
    /// </summary>
    void Start()
    {
        this.menu = FindObjectOfType<MenuCanvas>();
    }

    /// <summary>
    /// Opens the credits menu 
    /// Prevents the onTriggerStay from being called again by disabling the collider
    /// Turns off the isMenuOpened flag
    /// </summary>
    void OpenCreditsMenu()
    {
        AudioManager.instance.PlaySound(AudioManager.SoundName.TeleportalUsed);
        this.menu.OpenCreditsMenu();
        GetComponent<BoxCollider>().enabled = false;
        this.isMenuOpened = false;
    }

    /// <summary>
    /// Triggers the credits page
    /// </summary>
    /// <param name="other"></param>
	void OnTriggerStay(Collider other)
    {        
        if(other.tag == "Player" && !this.isMenuOpened) {
            this.isMenuOpened = true;
            this.OpenCreditsMenu();
        }
    }
}
