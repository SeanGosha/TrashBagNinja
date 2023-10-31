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
    public GameObject[] hearts = new GameObject[3];
    public GameObject heartContainer;

    //game over screen
    public GameObject gameOverScreen;

    private void Start()
    {
        heartContainer = GameObject.FindWithTag("Health");

        if (heartContainer != null)
        {
            foreach (Transform child in heartContainer.transform)
            {
                if (child.name == "Heart1")
                {
                    hearts[0] = child.gameObject;
                }
                else if (child.name == "Heart2")
                {
                    hearts[1] = child.gameObject;
                }
                else if (child.name == "Heart3")
                {
                    hearts[2] = child.gameObject;
                }
            }
        }
        else
        {
            Debug.LogError("Could not find an object with the 'Health' tag.");
        }
    }

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
                hearts[2].SetActive(false);
            }
            else if (health == 1)
            {
                hearts[1].SetActive(false);
            }
            else if (health == 0 && hearts[0].activeSelf)
            {
                hearts[0].SetActive(false);
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
                hearts[1].SetActive(true);
            }
            else if (health == 3)
            {
                hearts[2].SetActive(true);
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
