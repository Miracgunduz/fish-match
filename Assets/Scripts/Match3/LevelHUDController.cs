using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FishMatch.Match3
{
    public class LevelHUDController : MonoBehaviour
    {
        [SerializeField] private LevelController levelController;
        [SerializeField] private Text levelText;
        [SerializeField] private Text movesText;
        [SerializeField] private Text scoreText;
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private Text resultText;
        [SerializeField] private Button restartButton;

        private void Awake()
        {
            resultPanel.SetActive(false);
            restartButton.onClick.AddListener(RestartLevel);
        }

        private void OnEnable()
        {
            levelText.text = $"Seviye {levelController.CurrentLevelNumber}";

            levelController.OnMovesChanged += HandleMovesChanged;
            levelController.OnScoreChanged += HandleScoreChanged;
            levelController.OnLevelWon += HandleLevelWon;
            levelController.OnLevelLost += HandleLevelLost;
        }

        private void OnDisable()
        {
            levelController.OnMovesChanged -= HandleMovesChanged;
            levelController.OnScoreChanged -= HandleScoreChanged;
            levelController.OnLevelWon -= HandleLevelWon;
            levelController.OnLevelLost -= HandleLevelLost;
        }

        private void HandleMovesChanged(int remaining, int limit)
        {
            movesText.text = $"Hamle: {remaining}/{limit}";
        }

        private void HandleScoreChanged(int current, int target)
        {
            scoreText.text = $"Puan: {current}/{target}";
        }

        private void HandleLevelWon()
        {
            resultText.text = "Seviye Tamamlandı!";
            SetRestartButtonLabel("Sonraki Seviye");
            resultPanel.SetActive(true);
        }

        private void HandleLevelLost()
        {
            resultText.text = "Hamle Bitti";
            SetRestartButtonLabel("Tekrar Dene");
            resultPanel.SetActive(true);
        }

        private void SetRestartButtonLabel(string label)
        {
            var buttonText = restartButton.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.text = label;
            }
        }

        private void RestartLevel()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
