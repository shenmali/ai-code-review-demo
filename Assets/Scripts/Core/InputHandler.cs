using UnityEngine;

namespace PuzzleGame.Core
{
    /// <summary>
    /// Handles input for both mouse and touch
    /// Alternative to OnMouseDown for more control
    /// </summary>
    public class InputHandler : MonoBehaviour
    {
        #region Serialized Fields
        [SerializeField] private Camera mainCamera;
        [SerializeField] private LayerMask tileLayerMask = -1;
        #endregion

        #region Private Fields
        private bool isInputEnabled = true;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            // Cache camera reference
            if (mainCamera == null)
                mainCamera = Camera.main;
        }

        private void Update()
        {
            if (!isInputEnabled || GameManager.Instance == null)
                return;

            if (!GameManager.Instance.IsPlaying)
                return;

            HandleInput();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Enable or disable input
        /// </summary>
        public void SetInputEnabled(bool enabled)
        {
            isInputEnabled = enabled;
        }
        #endregion

        #region Private Methods
        private void HandleInput()
        {
            // Handle mouse input
            if (Input.GetMouseButtonDown(0))
            {
                ProcessInput(Input.mousePosition);
            }

            // Handle touch input for mobile
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    ProcessInput(touch.position);
                }
            }
        }

        private void ProcessInput(Vector2 screenPosition)
        {
            if (mainCamera == null)
                return;

            Ray ray = mainCamera.ScreenPointToRay(screenPosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity, tileLayerMask);

            if (hit.collider != null)
            {
                TileController tile = hit.collider.GetComponent<TileController>();
                if (tile != null)
                {
                    tile.TryMove();
                }
            }
        }
        #endregion

        #region Debug
        private void OnDrawGizmos()
        {
            // Visualize raycast in editor (optional)
            if (mainCamera != null && Input.GetMouseButton(0))
            {
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(ray.origin, ray.origin + ray.direction * 100f);
            }
        }
        #endregion
    }
}
