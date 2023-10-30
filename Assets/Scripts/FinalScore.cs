using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FinalScore : MonoBehaviour
{
    //final score text object
    public TMP_Text finalScoreText;
    //score counter script
    public ScoreCounter scoreCounter;

    // Update is called once per frame
    void Start()
    {
        finalScoreText.text = ("SCORE: " + scoreCounter.playerScore.ToString());
    }
}
