using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyTrashbag : MonoBehaviour
{
    //player controller script
    public PlayerController playerController;
    //player health script
    public PlayerHealth playerHealth;
    //explosion prefab
    public GameObject trashBagExplosionPrefab;
    //explosion gameobject
    private GameObject trashBagExplosion;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Trashbag") && playerController.isAttacking)
        {
            Destroy(other.gameObject);
            trashBagExplosion = Instantiate(trashBagExplosionPrefab, other.transform.position, Quaternion.identity);
            Destroy(trashBagExplosion, 1f);
            playerController.score++;
        }
    }

    private void OnCollisionEnter(Collision other) 
    {
        if(other.gameObject.CompareTag("Trashbag"))
        {
            playerHealth.LoseHealth();
            Destroy(other.gameObject);
            trashBagExplosion = Instantiate(trashBagExplosionPrefab, other.transform.position, Quaternion.identity);
            Destroy(trashBagExplosion, 1f);
        }
    }
}
