using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    //player health
    private int health = 3;

    //heart gameobjects
    public GameObject heart1;
    public GameObject heart2;
    public GameObject heart3;

    //game over screen
    public GameObject gameOverScreen;

    public void LoseHealth()
    {
        health--;
        if (health == 2)
        {
            heart3.SetActive(false);
        }
        else if (health == 1)
        {
            heart2.SetActive(false);
        }
        else if (health == 0 && heart1.activeSelf)
        {
            heart1.SetActive(false);
            Invoke("GameOver", .5f);
        }
    }

    public void GainHealth()
    {
        if (health < 3)
        {
            health++;
            if (health == 2)
            {
                heart2.SetActive(true);
            }
            else if (health == 3)
            {
                heart3.SetActive(true);
            }
        }
    }

    private void GameOver()
    {
        Debug.Log("Game Over");
        Time.timeScale = 0;
        gameOverScreen.SetActive(true);
    }

    public void Restart()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Exit()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(0);
    }
}
