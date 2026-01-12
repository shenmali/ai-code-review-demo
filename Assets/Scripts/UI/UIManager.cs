using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGame.UI
{
    /// <summary>
    /// Manages all UI elements and updates
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        #region Serialized Fields
        [Header("Panels")]
        [SerializeField] private GameObject menuPanel;
        [SerializeField] private GameObject gamePanel;
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject winPanel;
        [SerializeField] private GameObject losePanel;

        [Header("Game UI Elements")]
        [SerializeField] private Text moveCountText;
        [SerializeField] private Text timerText;

        [Header("Win Screen")]
        [SerializeField] private Text winMoveCountText;
        [SerializeField] private Text winTimeText;

        [Header("Buttons")]
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            // Setup button listeners
            SetupButtons();

            // Show menu by default
            ShowPanel(menuPanel);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Called when game state changes
        /// </summary>
        public void OnGameStateChanged(Core.GameState newState)
        {
            HideAllPanels();

            switch (newState)
            {
                case Core.GameState.Menu:
                    ShowPanel(menuPanel);
                    break;

                case Core.GameState.Playing:
                    ShowPanel(gamePanel);
                    break;

                case Core.GameState.Paused:
                    ShowPanel(pausePanel);
                    break;

                case Core.GameState.Won:
                    ShowPanel(winPanel);
                    break;

                case Core.GameState.Lost:
                    ShowPanel(losePanel);
                    break;
            }
        }

        /// <summary>
        /// Update move counter display
        /// </summary>
        public void UpdateMoveCount(int moves)
        {
            if (moveCountText != null)
            {
                moveCountText.text = $"Moves: {moves}";
            }
        }

        /// <summary>
        /// Update timer display
        /// </summary>
        public void UpdateTimer(float time)
        {
            if (timerText != null)
            {
                int minutes = Mathf.FloorToInt(time / 60f);
                int seconds = Mathf.FloorToInt(time % 60f);
                timerText.text = $"Time: {minutes:00}:{seconds:00}";
            }
        }

        /// <summary>
        /// Show win screen with stats
        /// </summary>
        public void ShowWinScreen(int moves, float time)
        {
            if (winMoveCountText != null)
            {
                winMoveCountText.text = $"Moves: {moves}";
            }

            if (winTimeText != null)
            {
                int minutes = Mathf.FloorToInt(time / 60f);
                int seconds = Mathf.FloorToInt(time % 60f);
                winTimeText.text = $"Time: {minutes:00}:{seconds:00}";
            }

            ShowPanel(winPanel);
        }

        /// <summary>
        /// Show lose screen
        /// </summary>
        public void ShowLoseScreen()
        {
            ShowPanel(losePanel);
        }
        #endregion

        #region Private Methods
        private void SetupButtons()
        {
            if (pauseButton != null)
            {
                pauseButton.onClick.RemoveAllListeners();
                pauseButton.onClick.AddListener(OnPauseClicked);
            }

            if (resumeButton != null)
            {
                resumeButton.onClick.RemoveAllListeners();
                resumeButton.onClick.AddListener(OnResumeClicked);
            }

            if (restartButton != null)
            {
                restartButton.onClick.RemoveAllListeners();
                restartButton.onClick.AddListener(OnRestartClicked);
            }
        }

        private void ShowPanel(GameObject panel)
        {
            if (panel != null)
            {
                panel.SetActive(true);
            }
        }

        private void HidePanel(GameObject panel)
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }

        private void HideAllPanels()
        {
            HidePanel(menuPanel);
            HidePanel(gamePanel);
            HidePanel(pausePanel);
            HidePanel(winPanel);
            HidePanel(losePanel);
        }
        #endregion

        #region Button Handlers
        private void OnPauseClicked()
        {
            if (Core.GameManager.Instance != null)
            {
                Core.GameManager.Instance.PauseGame();
            }

            if (Utils.AudioManager.Instance != null)
            {
                Utils.AudioManager.Instance.PlayButtonSound();
            }
        }

        private void OnResumeClicked()
        {
            if (Core.GameManager.Instance != null)
            {
                Core.GameManager.Instance.ResumeGame();
            }

            if (Utils.AudioManager.Instance != null)
            {
                Utils.AudioManager.Instance.PlayButtonSound();
            }
        }

        private void OnRestartClicked()
        {
            if (Core.GameManager.Instance != null)
            {
                Core.GameManager.Instance.RestartGame();
            }

            if (Utils.AudioManager.Instance != null)
            {
                Utils.AudioManager.Instance.PlayButtonSound();
            }
        }
        #endregion
    }
}
