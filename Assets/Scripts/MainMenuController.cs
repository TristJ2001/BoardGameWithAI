using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public static MainMenuController Instance { get; private set; }
    
    [SerializeField] private TMP_Text currentDifficultyText;

    public int hardDifficulty = 0;

    public bool singleplayer = true;

    public bool qLearning = false;
    
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Update()
    {
        if (hardDifficulty == 2)
        {
            currentDifficultyText.SetText($"Current Difficulty: Hard");
        }
        else if(hardDifficulty == 1)
        {
            currentDifficultyText.SetText($"Current Difficulty: Medium");
        }
        else
        {
            currentDifficultyText.SetText($"Current Difficulty: Easy");
        }
    }

    public void OnQLearningStartButtonCLicked()
    {
        singleplayer = false;
        qLearning = true;
        SceneManager.LoadScene("Game");
    }
    
    public void OnSinglePlayerStartButtonClicked()
    {
        singleplayer = true;
        qLearning = false;
        SceneManager.LoadScene("Game");
    }
    
    public void OnMultiplayerStartButtonClicked()
    {
        singleplayer = false;
        qLearning = false;
        SceneManager.LoadScene("Game");
    }

    public void OnHardDifficultyButtonClicked()
    {
        hardDifficulty = 2;
    }

    public void OnMediumDifficultyButtonClicked()
    {
        hardDifficulty = 1;
    }

    public void OnEasyDifficultyButtonClicked()
    {
        hardDifficulty = 0;
    }
}
