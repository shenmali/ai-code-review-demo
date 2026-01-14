using UnityEngine;

namespace PuzzleGame.Core
{
    /// <summary>
    /// Main game manager - handles game state, win/lose conditions, and game flow
    /// Singleton pattern for easy access
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        #region Singleton
        private static GameManager instance;
        public static GameManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<GameManager>();
                }
                return instance;
            }
        }
        #endregion

        #region Serialized Fields
        [Header("References")]
        [SerializeField] private GridManager gridManager;
        [SerializeField] private UI.UIManager uiManager;

        [Header("Game Settings")]
        [SerializeField] private bool useTimer = false;
        [SerializeField] private float maxTime = 300f; // 5 minutes
        #endregion

        #region Private Fields
        private GameState currentState = GameState.Menu;
        private int moveCount;
        private float elapsedTime;
        private bool isPuzzleSolved;
        #endregion

        #region Properties
        public GameState CurrentState => currentState;
        public bool IsPlaying => currentState == GameState.Playing;
        public int MoveCount => moveCount;
        public float ElapsedTime => elapsedTime;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            // Singleton setup
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;

            // Cache references if not set in Inspector
            if (gridManager == null)
                gridManager = FindObjectOfType<GridManager>();

            if (uiManager == null)
                uiManager = FindObjectOfType<UI.UIManager>();
        }

        private void Start()
        {
            ChangeState(GameState.Menu);
        }

        private void Update()
        {
            if (currentState == GameState.Playing)
            {
                if (useTimer)
                {
                    UpdateTimer();
                }
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Start a new game
        /// </summary>
        public void StartNewGame()
        {
            ResetGameStats();

            if (gridManager != null)
            {
                gridManager.InitializeGrid();
                gridManager.ShufflePuzzle();
            }

            ChangeState(GameState.Playing);
        }

        /// <summary>
        /// Restart current game
        /// </summary>
        public void RestartGame()
        {
            StartNewGame();
        }

        /// <summary>
        /// Pause the game
        /// </summary>
        public void PauseGame()
        {
            if (currentState == GameState.Playing)
            {
                ChangeState(GameState.Paused);
            }
        }

        /// <summary>
        /// Resume the game
        /// </summary>
        public void ResumeGame()
        {
            if (currentState == GameState.Paused)
            {
                ChangeState(GameState.Playing);
            }
        }

        /// <summary>
        /// Return to main menu
        /// </summary>
        public void ReturnToMenu()
        {
            ChangeState(GameState.Menu);
        }

        /// <summary>
        /// Called when a tile is moved
        /// </summary>
        public void OnTileMoved()
        {
            if (currentState != GameState.Playing)
                return;

            moveCount++;

            if (uiManager != null)
            {
                uiManager.UpdateMoveCount(moveCount);
            }

            // Check win condition
            CheckWinCondition();
        }
        #endregion

        #region Private Methods
        private void ChangeState(GameState newState)
        {
            currentState = newState;

            if (uiManager != null)
            {
                uiManager.OnGameStateChanged(newState);
            }

            switch (newState)
            {
                case GameState.Menu:
                    Time.timeScale = 1f;
                    break;

                case GameState.Playing:
                    Time.timeScale = 1f;
                    break;

                case GameState.Paused:
                    Time.timeScale = 0f;
                    break;

                case GameState.Won:
                    Time.timeScale = 1f;
                    OnGameWon();
                    break;

                case GameState.Lost:
                    Time.timeScale = 1f;
                    OnGameLost();
                    break;
            }
        }

        private void ResetGameStats()
        {
            moveCount = 0;
            elapsedTime = 0f;
            isPuzzleSolved = false;

            if (uiManager != null)
            {
                uiManager.UpdateMoveCount(moveCount);
                uiManager.UpdateTimer(elapsedTime);
            }
        }

        private void UpdateTimer()
        {
            elapsedTime += Time.deltaTime;

            if (uiManager != null)
            {
                uiManager.UpdateTimer(elapsedTime);
            }

            // Check time limit
            if (useTimer && elapsedTime >= maxTime)
            {
                ChangeState(GameState.Lost);
            }
        }

        private void CheckWinCondition()
        {
            if (isPuzzleSolved)
                return;

            if (gridManager != null && gridManager.IsPuzzleSolved())
            {
                isPuzzleSolved = true;
                ChangeState(GameState.Won);
            }
        }

        private void OnGameWon()
        {
            Debug.Log($"Puzzle solved! Moves: {moveCount}, Time: {elapsedTime:F2}s");

            if (uiManager != null)
            {
                uiManager.ShowWinScreen(moveCount, elapsedTime);
            }

            // Play win sound
            if (Utils.AudioManager.Instance != null)
            {
                Utils.AudioManager.Instance.PlayWinSound();
            }
        }

        private void OnGameLost()
        {
            Debug.Log("Game over - Time's up!");

            if (uiManager != null)
            {
                uiManager.ShowLoseScreen();
            }

            // Play lose sound
            if (Utils.AudioManager.Instance != null)
            {
                Utils.AudioManager.Instance.PlayLoseSound();
            }
        }
        #endregion
    }

    /// <summary>
    /// Game state enumeration
    /// </summary>
    public enum GameState
    {
        Menu,
        Playing,
        Paused,
        Won,
        Lost
    }
}
