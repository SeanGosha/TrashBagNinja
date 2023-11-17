using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class ScoreCounter : MonoBehaviour
{
    // references
    public TMP_Text scoreText;
    public PlayerController playerController;

    // previous score and color
    private int previousScore;
    private Color previousColor = new Color(0.58f, 0.18f, 1f);

    // current player score
    public int playerScore;

    private void Start()
    {
        // Initialize the previous score and color
        previousScore = playerController.score;
        previousColor = Color.white; // Initial color doesn't matter, so we set it to white
    }

    private void Update()
    {
        // Check if the score has changed
        if (playerController.score != previousScore)
        {
            // Update the player score
            playerScore = playerController.score / 2;
            // Update the score text
            scoreText.text = playerScore.ToString();

            // Change the color of the text to a random bright color
            Color randomColor = GetRandomBrightColor();
            scoreText.color = randomColor;

            // Update the previous score and color
            previousScore = playerController.score;
            previousColor = randomColor;
        }
    }

    private Color GetRandomBrightColor()
    {
        Color[] brightColors =
        {
            new Color(1f, 0.92f, 0.016f),  // Bright yellow
            new Color(1f, 0.15f, 0f),      // Bright orange
            new Color(1f, 0.04f, 0.04f),   // Bright red
            new Color(1f, 0.44f, 0.016f),  // Bright gold
            new Color(0f, 0.82f, 0.08f),   // Bright green
            new Color(0f, 0.65f, 1f),      // Bright blue
            new Color(0.58f, 0.18f, 1f),   // Bright purple
            new Color(1f, 1f, 1f)          // Bright white
        };

        // Generate a random index different from the previous color
        int randomIndex;
        do
        {
            randomIndex = Random.Range(0, brightColors.Length);
        } while (brightColors[randomIndex] == previousColor);

        // Return the randomly selected bright color
        return brightColors[randomIndex];
    }
}