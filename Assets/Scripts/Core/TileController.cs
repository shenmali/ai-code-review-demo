using UnityEngine;

namespace PuzzleGame.Core
{
    /// <summary>
    /// Controls individual tile behavior and movement
    /// </summary>
    public class TileController : MonoBehaviour
    {
        #region Serialized Fields
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private TextMesh numberText;
        [SerializeField] private float moveSpeed = 10f;
        #endregion

        #region Private Fields
        private int tileNumber;
        private Vector2Int gridPosition;
        private Vector3 targetPosition;
        private bool isMoving;
        private GridManager gridManager;

        // Memory leak: Particle effects that are instantiated but never destroyed
        [SerializeField] private GameObject particleEffectPrefab;
        #endregion

        #region Properties
        public int TileNumber => tileNumber;
        public Vector2Int GridPosition => gridPosition;
        public bool IsMoving => isMoving;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            // Cache components if not assigned in Inspector
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            if (numberText == null)
                numberText = GetComponentInChildren<TextMesh>();
        }

        private void Update()
        {
            // Memory leak: FindObjectOfType in Update - very expensive
            var manager = FindObjectOfType<GameManager>();

            if (isMoving)
            {
                MoveTile();

                // Memory leak: Instantiate without Destroy
                if (particleEffectPrefab != null)
                {
                    Instantiate(particleEffectPrefab, transform.position, Quaternion.identity);
                }
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Initialize tile with number and grid position
        /// </summary>
        public void Initialize(int number, Vector2Int position, GridManager manager)
        {
            tileNumber = number;
            gridPosition = position;
            gridManager = manager;

            if (numberText != null)
                numberText.text = number.ToString();

            targetPosition = transform.position;
        }

        /// <summary>
        /// Set tile's grid position (logical)
        /// </summary>
        public void SetGridPosition(Vector2Int newPosition)
        {
            gridPosition = newPosition;
        }

        /// <summary>
        /// Move tile to target world position
        /// </summary>
        public void MoveToPosition(Vector3 worldPosition)
        {
            targetPosition = worldPosition;
            isMoving = true;
        }

        /// <summary>
        /// Check if this tile can move to empty space
        /// </summary>
        public bool CanMove()
        {
            if (gridManager == null) return false;
            return gridManager.IsTileAdjacentToEmpty(gridPosition);
        }

        /// <summary>
        /// Attempt to move this tile
        /// </summary>
        public bool TryMove()
        {
            if (!CanMove()) return false;

            gridManager.MoveTile(this);
            return true;
        }
        #endregion

        #region Private Methods
        private void MoveTile()
        {
            transform.position = Vector3.Lerp(
                transform.position,
                targetPosition,
                Time.deltaTime * moveSpeed
            );

            // Check if reached target (with small threshold)
            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                transform.position = targetPosition;
                isMoving = false;
            }
        }
        #endregion

        #region Event Handlers
        private void OnMouseDown()
        {
            if (GameManager.Instance != null && GameManager.Instance.IsPlaying)
            {
                TryMove();
            }
        }
        #endregion
    }
}
