using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashBagThrower : MonoBehaviour
{
    // references
    public GameObject[] trashbagSpawnPoints;
    public GameObject trashBagPrefab;
    public Countdown countdown;
    public PlayerController playerController;

    //rate of trashbag spawn
    public float throwInterval = 3f; // Interval between trash bag throws
    //int to hold random spawn point index
    public int randomSpawnPoint;
    //bool to check if the countdown is done
    private bool startThrowing = false;
 
    void Update()
    {
        if (countdown.countDownDone && !startThrowing)
        {
            startThrowing = true;
            StartCoroutine(ThrowTrashBagsCoroutine());
        }
    }

    private IEnumerator ThrowTrashBagsCoroutine()
    {
        while (true)
        {
            ThrowTrashBag();

            yield return new WaitForSeconds(throwInterval);
        }
    }

    public void ThrowTrashBag()
    {
        randomSpawnPoint = Random.Range(0, trashbagSpawnPoints.Length);
        // Instantiate the trash bag prefab
        GameObject trashBag = Instantiate(trashBagPrefab, trashbagSpawnPoints[randomSpawnPoint].transform.position, Quaternion.identity);
        playerController.totalTrashbags++;
        ChangeThrowInterval();

        // Apply a random force to simulate throwing
        Rigidbody trashBagRigidbody = trashBag.GetComponent<Rigidbody>();
        trashBagRigidbody.AddForce(new Vector3(0, 10, -4.3f), ForceMode.Impulse);
    }

    public void ChangeThrowInterval()
    {
        throwInterval = Mathf.Clamp(2f * Mathf.Exp(-playerController.totalTrashbags / 100f), .6f, 2f);
    }
}
