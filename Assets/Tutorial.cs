using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
    public PlayerController playerController;
    public GameObject countdown;

    public void CloseTutorialButton()
    {
        Invoke("CloseTutorial", 0.5f);
    }

    public void CloseTutorial()
    {
        playerController.gameActive = true;
        countdown.SetActive(true);
        Destroy(gameObject);
    }
}
