using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyPowerUp : MonoBehaviour
{
    //rigidbody of the power up
    private Rigidbody powerUpRigidbody;
    //explosion prefab
    public GameObject powerUpExplosionPrefab;

    private void Start() 
    {
        powerUpRigidbody = GetComponent<Rigidbody>();
    }

    private void OnTriggerEnter(Collider other) 
    {
        if(other.gameObject.CompareTag("Player"))
        {
            DestroyPowerUpObject();
        } 
        else if(other.gameObject.CompareTag("Environment"))
        {
            transform.position = new Vector3(transform.position.x, -.3f, transform.position.z);
            powerUpRigidbody.isKinematic = true;
            powerUpRigidbody.constraints = RigidbodyConstraints.FreezePositionZ;
            Invoke("DestroyPowerUpObject", .7f);
        }   
    }

    private void DestroyPowerUpObject()
    {
            Destroy(this.gameObject);
            Instantiate(powerUpExplosionPrefab, transform.position, Quaternion.identity);
    }
}
