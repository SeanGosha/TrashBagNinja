using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        // Load the first level to start the game
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }
}
