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

    public int[] highScores;

    void Start()
    {
        finalScoreText.text = "SCORE: " + scoreCounter.playerScore.ToString();

        for (int i = 1; i <= 10; i++)
        {
            if (PlayerPrefs.HasKey("Highscore" + i))
            {
                if (scoreCounter.playerScore > PlayerPrefs.GetInt("Highscore" + i))
                {
                    // Store the current high score and replace it
                    int temp = PlayerPrefs.GetInt("Highscore" + i);
                    string date = System.DateTime.Now.ToString("MM/dd/yy");
                    
                    PlayerPrefs.SetInt("Highscore" + i, scoreCounter.playerScore);
                    PlayerPrefs.SetString("HighscoreDate" + i, date);

                    // Move lower scores down
                    for (int j = i + 1; j <= 10; j++)
                    {
                        if (PlayerPrefs.HasKey("Highscore" + j))
                        {
                            int nextTemp = PlayerPrefs.GetInt("Highscore" + j);
                            PlayerPrefs.SetInt("Highscore" + j, temp);
                            temp = nextTemp;
                        }
                    }

                    break; // Break the loop after updating the score
                }
            }
            else
            {
                // If a slot is empty, just add the new score
                PlayerPrefs.SetInt("Highscore" + i, scoreCounter.playerScore);
                break; // Break the loop after adding the score
            }
        }
    }
}
