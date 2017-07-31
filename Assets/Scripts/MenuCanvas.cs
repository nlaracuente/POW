using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// The Menu Canvas holds the menu options for the player
/// It also contains the music for the game
/// Handles "pausing/resuming" the game when the menu is enabled/disabled
/// Quits the application when requested
/// </summary>
public class MenuCanvas : MonoBehaviour
{

    /// <summary>
    /// Closes the menu
    /// Resumes the game
    /// </summary>
    public void Play()
    {

    }

    /// <summary>
    /// Menu is triggered opened
    /// </summary>
    public void OpenMenu()
    {

    }

    /// <summary>
    /// When is closed
    /// </summary>
    public void CloseMenu()
    {

    }

	// <summary>
    /// Closes the app
    /// </summary>
    public void Exit()
    {
        Debug.Log("Quit");
        Application.Quit();
    }
}
