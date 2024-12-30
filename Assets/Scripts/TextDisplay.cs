
using UnityEngine;
using TMPro;


public class TextDisplay : MonoBehaviour
{
    public TextMeshProUGUI frogStateText; // TextMeshProUGUI component
    public Manager manager;               //  Manager
    public RectTransform textRectTransform; // RectTransform of the Text UI element

    private void Start()
    {
        if (frogStateText != null)
        {
            textRectTransform = frogStateText.GetComponent<RectTransform>();
        }
    }

    private void Update()
    {
        //int wins = PlayerPrefs.GetInt("Wins", 0);
        //int losses = PlayerPrefs.GetInt("Losses", 0);
        //int totalGames = wins + losses;
        //float winRate = totalGames > 0 ? (float)wins / totalGames * 100 : 0;

        if (manager.frog != null && manager.frog.state != null)
        {

            if (manager.frog.state.Row <= 9)
            {
                frogStateText.text = $"Flies in Belly: {manager.frog.state.FliesInBelly}     " +
                                     $"                                                              " +
                                     $"Row: {manager.frog.state.Row}";
            }
        }
        else
        {
            frogStateText.text = "Frog state unavailable.";

        }
    }
}
