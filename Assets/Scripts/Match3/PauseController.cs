using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FishMatch.Match3
{
    public class PauseController : MonoBehaviour
    {
        [SerializeField] private Match3GridController gridController;
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;

        private void Awake()
        {
            pausePanel.SetActive(false);
            pauseButton.onClick.AddListener(OpenPause);
            resumeButton.onClick.AddListener(ClosePause);
            restartButton.onClick.AddListener(RestartLevel);
        }

        private void OpenPause()
        {
            pausePanel.SetActive(true);
            gridController.InputEnabled = false;
            Time.timeScale = 0f;
        }

        private void ClosePause()
        {
            pausePanel.SetActive(false);
            gridController.InputEnabled = true;
            Time.timeScale = 1f;
        }

        private void RestartLevel()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
