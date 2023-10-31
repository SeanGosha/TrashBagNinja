using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    //player health
    private int health = 3;

    //shield
    public bool shield = false;
    private bool shieldActive = false;
    public Shader outlineShader;
    public SkinnedMeshRenderer playerMesh;

    //toxic
    public bool toxic = false;
    private bool toxicActive = false;

    //heart gameobjects
    public GameObject heart1;
    public GameObject heart2;
    public GameObject heart3;

    //game over screen
    public GameObject gameOverScreen;

    private void Update()
    {
        if (shield && !shieldActive)
        {
            shield = false;
            StartCoroutine(ShieldEffect());
        }

        if (toxic && !toxicActive)
        {
            toxic = false;
            StartCoroutine(ToxicEffect());
        }
    }

    public void LoseHealth()
    {
        if (!shieldActive) // Check if the shield is not active
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
                Invoke("GameOver", 0.5f);
            }
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

    private IEnumerator ShieldEffect()
    {
        shieldActive = true;
        playerMesh.material.shader = outlineShader;
        playerMesh.material.SetColor("_OutlineColor", new Color(195, 0, 143));
        yield return new WaitForSeconds(5.2f);
        int i = 0;
        while (i < 4)
        {
            playerMesh.material.shader = Shader.Find("Mobile/Diffuse");
            yield return new WaitForSeconds(.1f);
            playerMesh.material.shader = outlineShader;
            yield return new WaitForSeconds(.1f);
            i++;
        }
        playerMesh.material.shader = Shader.Find("Mobile/Diffuse");
        shieldActive = false;
    }

        private IEnumerator ToxicEffect()
    {
        toxicActive = true;
        playerMesh.material.shader = outlineShader;
        playerMesh.material.SetColor("_OutlineColor", new Color(0, 255, 0));
        int i = 0;
        while (i < 4)
        {
            playerMesh.material.shader = Shader.Find("Mobile/Diffuse");
            yield return new WaitForSeconds(.1f);
            playerMesh.material.shader = outlineShader;
            yield return new WaitForSeconds(.1f);
            i++;
        }
        LoseHealth();
        playerMesh.material.shader = Shader.Find("Mobile/Diffuse");
        toxicActive = false;
    }

    private void GameOver()
    {
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
