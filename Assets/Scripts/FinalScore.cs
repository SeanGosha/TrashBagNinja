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
                    // Store the current high score and date
                    int tempScore = PlayerPrefs.GetInt("Highscore" + i);
                    string tempDate = PlayerPrefs.GetString("HighscoreDate" + i);

                    // Replace the high score and date
                    PlayerPrefs.SetInt("Highscore" + i, scoreCounter.playerScore);
                    PlayerPrefs.SetString("HighscoreDate" + i, System.DateTime.Now.ToString("dd/MM/yy"));

                    // Move lower scores and dates down
                    for (int j = i + 1; j <= 10; j++)
                    {
                        if (PlayerPrefs.HasKey("Highscore" + j))
                        {
                            int nextTempScore = PlayerPrefs.GetInt("Highscore" + j);
                            string nextTempDate = PlayerPrefs.GetString("HighscoreDate" + j);

                            PlayerPrefs.SetInt("Highscore" + j, tempScore);
                            PlayerPrefs.SetString("HighscoreDate" + j, tempDate);

                            tempScore = nextTempScore;
                            tempDate = nextTempDate;
                        }
                    }

                    break; // Break the loop after updating the score and date
                }
            }
            else
            {
                // If a slot is empty, just add the new score and date
                PlayerPrefs.SetInt("Highscore" + i, scoreCounter.playerScore);
                PlayerPrefs.SetString("HighscoreDate" + i, System.DateTime.Now.ToString("dd/MM/yy"));
                break; // Break the loop after adding the score and date
            }
        }
    }
}
