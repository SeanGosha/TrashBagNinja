using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class Countdown : MonoBehaviour
{
    public TextMeshProUGUI countdownText;
    private int countdownValue = 5;
    public bool countDownDone = false;

    public AudioSource countdownAudio;
    public AudioClip shortBeep;
    public AudioClip goSound;


    void Start()
    {
        StartCoroutine(StartCountdown());
    }

    IEnumerator StartCountdown()
    {
        countdownAudio.volume = 0.5f;
        while (countdownValue > 0)
        {
            countdownText.text = countdownValue.ToString();
            countdownAudio.PlayOneShot(shortBeep);
            yield return new WaitForSeconds(1f);
            countdownValue--;
        }

        countdownAudio.volume = 1f;
        countdownAudio.PlayOneShot(goSound);
        countdownText.text = "Go!";
        countDownDone = true;
        yield return new WaitForSeconds(1f);
        Destroy(countdownText.gameObject);
    }
}