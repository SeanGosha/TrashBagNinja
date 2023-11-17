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
        else if(this.gameObject.name == "SpeedPowerUp(Clone)")
        {
            if(other.gameObject.CompareTag("Player"))
            {
                // set the speed boost flag to true
                other.gameObject.GetComponent<PlayerController>().speedBoost = true;
            }
        }
        else if(this.gameObject.name == "ShieldPowerUp(Clone)")
        {
            if (other.gameObject.CompareTag("Player"))
            {
                // set the shield flag to true
                other.gameObject.GetComponent<PlayerHealth>().shield = true;
            }
        }
        else if(this.gameObject.name == "ToxicPowerUp(Clone)")
        {
            if (other.gameObject.CompareTag("Player"))
            {
                // set the toxic flag to true
                other.gameObject.GetComponent<PlayerHealth>().toxic = true;
            }
        }
        else if(this.gameObject.name == "PlusOnePowerUp(Clone)")
        {
            if (other.gameObject.CompareTag("Player"))
            {
                // Add another player to the scene
                other.gameObject.GetComponent<PlayerController>().plusOne = true;
            }
        }
    }
}
