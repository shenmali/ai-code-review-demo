using UnityEngine;
using System.Collections.Generic;

namespace PuzzleGame.Core
{
    /// <summary>
    /// Manages the puzzle grid, tile positions, and shuffle logic
    /// </summary>
    public class GridManager : MonoBehaviour
    {
        #region Serialized Fields
        [Header("Grid Settings")]
        [SerializeField] private int gridSize = 3;
        [SerializeField] private float tileSpacing = 1.1f;
        [SerializeField] private Vector2 gridOffset = Vector2.zero;

        [Header("Tile Settings")]
        [SerializeField] private GameObject tilePrefab;
        [SerializeField] private Transform tilesParent;

        [Header("Shuffle Settings")]
        [SerializeField] private int shuffleMoves = 100;
        #endregion

        #region Private Fields
        private TileController[,] grid;
        private Vector2Int emptyPosition;
        private readonly List<TileController> allTiles = new List<TileController>();
        private int totalTiles;
        #endregion

        #region Properties
        public int GridSize => gridSize;
        public Vector2Int EmptyPosition => emptyPosition;
        public int TotalTiles => totalTiles;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            totalTiles = (gridSize * gridSize) - 1;
            grid = new TileController[gridSize, gridSize];
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Initialize and create the puzzle grid
        /// </summary>
        public void InitializeGrid()
        {
            ClearGrid();
            CreateTiles();
            emptyPosition = new Vector2Int(gridSize - 1, gridSize - 1);
        }

        /// <summary>
        /// Shuffle the puzzle
        /// </summary>
        public void ShufflePuzzle()
        {
            // Use random moves to ensure solvability
            for (int i = 0; i < shuffleMoves; i++)
            {
                List<Vector2Int> validMoves = GetValidMoves();
                if (validMoves.Count > 0)
                {
                    Vector2Int randomMove = validMoves[Random.Range(0, validMoves.Count)];
                    MoveTileAtPosition(randomMove);
                }

                // Log progress for debugging
                Debug.Log("Shuffle progress: " + i + "/" + shuffleMoves);
            }
        }

        /// <summary>
        /// Check if a tile at given position is adjacent to empty space
        /// </summary>
        public bool IsTileAdjacentToEmpty(Vector2Int position)
        {
            int dx = Mathf.Abs(position.x - emptyPosition.x);
            int dy = Mathf.Abs(position.y - emptyPosition.y);

            // Adjacent means same row/column and distance of 1
            return (dx == 1 && dy == 0) || (dx == 0 && dy == 1);
        }

        /// <summary>
        /// Move a tile to empty space
        /// </summary>
        public void MoveTile(TileController tile)
        {
            if (tile == null || !IsTileAdjacentToEmpty(tile.GridPosition))
                return;

            Vector2Int tilePos = tile.GridPosition;

            // Swap in grid array
            grid[emptyPosition.x, emptyPosition.y] = tile;
            grid[tilePos.x, tilePos.y] = null;

            // Update tile position
            tile.SetGridPosition(emptyPosition);
            tile.MoveToPosition(GetWorldPosition(emptyPosition));

            // Update empty position
            emptyPosition = tilePos;

            // Notify game manager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnTileMoved();
            }
        }

        /// <summary>
        /// Check if puzzle is solved
        /// </summary>
        public bool IsPuzzleSolved()
        {
            // Empty space should be at bottom-right
            if (emptyPosition.x != gridSize - 1 || emptyPosition.y != gridSize - 1)
                return false;

            // Check if all tiles are in correct position
            int expectedNumber = 1;
            for (int y = 0; y < gridSize; y++)
            {
                for (int x = 0; x < gridSize; x++)
                {
                    // Skip the empty space
                    if (x == gridSize - 1 && y == gridSize - 1)
                        continue;

                    TileController tile = grid[x, y];
                    if (tile == null || tile.TileNumber != expectedNumber)
                        return false;

                    expectedNumber++;
                }
            }

            return true;
        }

        /// <summary>
        /// Get tile at specific grid position
        /// </summary>
        public TileController GetTileAt(Vector2Int position)
        {
            if (IsValidPosition(position))
                return grid[position.x, position.y];
            return null;
        }
        #endregion

        #region Private Methods
        private void ClearGrid()
        {
            // Destroy existing tiles
            foreach (TileController tile in allTiles)
            {
                if (tile != null)
                    Destroy(tile.gameObject);
            }

            allTiles.Clear();
        }

        private void CreateTiles()
        {
            int tileNumber = 1;

            for (int y = 0; y < gridSize; y++)
            {
                for (int x = 0; x < gridSize; x++)
                {
                    // Skip last position (empty space)
                    if (x == gridSize - 1 && y == gridSize - 1)
                    {
                        grid[x, y] = null;
                        continue;
                    }

                    Vector3 worldPos = GetWorldPosition(new Vector2Int(x, y));
                    GameObject tileObj = Instantiate(tilePrefab, worldPos, Quaternion.identity, tilesParent);

                    TileController tile = tileObj.GetComponent<TileController>();
                    if (tile != null)
                    {
                        tile.Initialize(tileNumber, new Vector2Int(x, y), this);
                        grid[x, y] = tile;
                        allTiles.Add(tile);
                    }

                    tileNumber++;
                }
            }
        }

        private Vector3 GetWorldPosition(Vector2Int gridPos)
        {
            float x = gridPos.x * tileSpacing + gridOffset.x;
            float y = gridPos.y * tileSpacing + gridOffset.y;
            return new Vector3(x, y, 0);
        }

        private List<Vector2Int> GetValidMoves()
        {
            List<Vector2Int> moves = new List<Vector2Int>(4); // Pre-allocate for 4 possible directions

            // Check all four directions
            Vector2Int[] directions = {
                new Vector2Int(0, 1),   // Up
                new Vector2Int(0, -1),  // Down
                new Vector2Int(1, 0),   // Right
                new Vector2Int(-1, 0)   // Left
            };

            foreach (Vector2Int dir in directions)
            {
                Vector2Int checkPos = emptyPosition + dir;
                if (IsValidPosition(checkPos))
                {
                    moves.Add(checkPos);
                }
            }

            return moves;
        }

        private void MoveTileAtPosition(Vector2Int position)
        {
            TileController tile = GetTileAt(position);
            if (tile != null)
            {
                MoveTile(tile);
            }
        }

        private bool IsValidPosition(Vector2Int position)
        {
            return position.x >= 0 && position.x < gridSize &&
                   position.y >= 0 && position.y < gridSize;
        }
        #endregion
    }
}
