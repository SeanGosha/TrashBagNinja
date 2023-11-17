using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpThrower : MonoBehaviour
{
    //references
    public TrashBagThrower trashBagThrower;
    public GameObject[] powerUpPrefabs;
    public Countdown countdown;

    // int to hold random powerup index
    private int randomPowerUp;
    // int to hold random spawn point index
    private int powerUpSpawnPoint;
    // float to hold the interval between powerup throws
    public float throwInterval = 15f;
    // bool to check if the countdown is done
    private bool startThrowing = false;


    void Update()
    {
        if (countdown.countDownDone && !startThrowing)
        {
            startThrowing = true;
            StartCoroutine(ThrowPowerUpsCoroutine());
        }
    }

    private IEnumerator ThrowPowerUpsCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(throwInterval);

            ThrowPowerUp();
        }
    }

    public void ThrowPowerUp()
    {
        randomPowerUp = Random.Range(0, powerUpPrefabs.Length);

        powerUpSpawnPoint = Random.Range(0, trashBagThrower.trashbagSpawnPoints.Length);

        do
        {
            powerUpSpawnPoint = Random.Range(0, trashBagThrower.trashbagSpawnPoints.Length);
        }
        while(powerUpSpawnPoint == trashBagThrower.randomSpawnPoint);

        GameObject powerUp = Instantiate(powerUpPrefabs[randomPowerUp], trashBagThrower.trashbagSpawnPoints[powerUpSpawnPoint].transform.position, Quaternion.identity);
        
        Rigidbody powerUpRigidbody = powerUp.GetComponent<Rigidbody>();
        powerUpRigidbody.AddForce(new Vector3(0, 12, -3.6f), ForceMode.Impulse);
    }


}
