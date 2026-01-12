using UnityEngine;

namespace PuzzleGame.Utils
{
    /// <summary>
    /// ScriptableObject for game configuration data
    /// Create via: Assets > Create > Puzzle Game > Game Data
    /// </summary>
    [CreateAssetMenu(fileName = "GameData", menuName = "Puzzle Game/Game Data", order = 1)]
    public class GameData : ScriptableObject
    {
        #region Grid Settings
        [Header("Grid Settings")]
        [Tooltip("Grid size (NxN). Recommended: 3-5")]
        [Range(3, 6)]
        public int gridSize = 3;

        [Tooltip("Space between tiles")]
        public float tileSpacing = 1.1f;

        [Tooltip("Offset from world origin")]
        public Vector2 gridOffset = Vector2.zero;
        #endregion

        #region Gameplay Settings
        [Header("Gameplay Settings")]
        [Tooltip("Number of random moves to shuffle puzzle")]
        [Range(10, 500)]
        public int shuffleMoves = 100;

        [Tooltip("Tile movement speed")]
        [Range(1f, 20f)]
        public float tileSpeed = 10f;

        [Tooltip("Enable timer mode")]
        public bool useTimer = false;

        [Tooltip("Time limit in seconds (if timer enabled)")]
        public float timeLimit = 300f; // 5 minutes
        #endregion

        #region Visual Settings
        [Header("Visual Settings")]
        [Tooltip("Tile background color")]
        public Color tileColor = Color.white;

        [Tooltip("Empty space color")]
        public Color emptySpaceColor = new Color(0.8f, 0.8f, 0.8f, 0.5f);

        [Tooltip("Text color for tile numbers")]
        public Color textColor = Color.black;
        #endregion

        #region Difficulty Presets
        [Header("Difficulty Presets")]
        [Tooltip("Easy: 3x3, no timer")]
        public DifficultyPreset easy = new DifficultyPreset
        {
            gridSize = 3,
            shuffleMoves = 50,
            useTimer = false
        };

        [Tooltip("Medium: 4x4, no timer")]
        public DifficultyPreset medium = new DifficultyPreset
        {
            gridSize = 4,
            shuffleMoves = 150,
            useTimer = false
        };

        [Tooltip("Hard: 5x5, with timer")]
        public DifficultyPreset hard = new DifficultyPreset
        {
            gridSize = 5,
            shuffleMoves = 300,
            useTimer = true,
            timeLimit = 600f // 10 minutes
        };
        #endregion

        #region Public Methods
        /// <summary>
        /// Apply difficulty preset
        /// </summary>
        public void ApplyPreset(Difficulty difficulty)
        {
            DifficultyPreset preset = difficulty switch
            {
                Difficulty.Easy => easy,
                Difficulty.Medium => medium,
                Difficulty.Hard => hard,
                _ => easy
            };

            gridSize = preset.gridSize;
            shuffleMoves = preset.shuffleMoves;
            useTimer = preset.useTimer;
            if (preset.useTimer)
            {
                timeLimit = preset.timeLimit;
            }
        }

        /// <summary>
        /// Validate settings (called in editor)
        /// </summary>
        private void OnValidate()
        {
            // Ensure sensible values
            gridSize = Mathf.Clamp(gridSize, 3, 6);
            shuffleMoves = Mathf.Max(10, shuffleMoves);
            tileSpeed = Mathf.Max(1f, tileSpeed);
            timeLimit = Mathf.Max(30f, timeLimit);
        }
        #endregion
    }

    /// <summary>
    /// Difficulty preset data structure
    /// </summary>
    [System.Serializable]
    public class DifficultyPreset
    {
        public int gridSize;
        public int shuffleMoves;
        public bool useTimer;
        public float timeLimit;
    }

    /// <summary>
    /// Difficulty levels enum
    /// </summary>
    public enum Difficulty
    {
        Easy,
        Medium,
        Hard
    }
}
