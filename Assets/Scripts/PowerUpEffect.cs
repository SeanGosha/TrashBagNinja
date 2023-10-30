using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpEffect : MonoBehaviour
{
    private void OnTriggerEnter(Collider other) 
    {
        if(this.gameObject.name == "HeartPowerUp(Clone)")
        {
            if(other.gameObject.CompareTag("Player"))
            {
                // Add health to the player by calling the GainHealth() method from the PlayerHealth script
                other.gameObject.GetComponent<PlayerHealth>().GainHealth();
            }
        }
    }
}
