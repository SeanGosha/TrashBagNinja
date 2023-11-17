using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HighscoreList : MonoBehaviour
{
    public TMP_Text[] highscoreTexts;

    public TMP_Text[] highscoreDateTexts;

    

    public void Start()
    {
        SetHighScores();
        Debug.Log(PlayerPrefs.GetInt("Highscore1"));
        SetHighScoreTexts();
    }


    private void SetHighScores()
    {
        for(int i = 1; i <= 10; i++)
        {
            if (PlayerPrefs.HasKey("Highscore" + i) == false)
            {
                PlayerPrefs.SetInt("Highscore" + i, 0);
            }

            if (PlayerPrefs.HasKey("HighscoreDate" + i) == false)
            {
                PlayerPrefs.SetString("HighscoreDate" + i, "00/00/00");
            }
        }
    }

    private void SetHighScoreTexts()
    {
        for (int i = 1; i <= 10; i++)
        {
            highscoreTexts[i - 1].text = PlayerPrefs.GetInt("Highscore" + i).ToString();
        }

        for (int i = 1; i <= 10; i++)
        {
            highscoreDateTexts[i - 1].text = PlayerPrefs.GetString("HighscoreDate" + i);
        }
    }
}
