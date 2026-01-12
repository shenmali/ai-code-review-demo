# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repository Overview

This repository contains two main projects:

1. **AI Code Review Bot** - GitHub Action that automatically reviews Unity 2D code
2. **Unity 2D Sliding Tile Puzzle Game** - Complete puzzle game implementation

---

## Project 1: AI Code Review Bot

### Overview
An AI-powered code review bot specialized for Unity 2D casual game development. Runs as a GitHub Action using OpenRouter API (Claude 3.5 Sonnet) and posts reviews in Turkish.

### Architecture
1. **GitHub Action Trigger** (`.github/workflows/ai-code-review.yml`): Runs on PR open/update
2. **Commit-by-Commit Review**: Reviews each commit separately for detailed feedback
3. **AI Review** (`ai_review.js`): Sends diff to OpenRouter API, receives specialized review
4. **Comment Posting**: Posts AI-generated reviews as PR comments

### Key Features
- `.aiignore` file support (like `.gitignore` for excluding files from review)
- Per-commit analysis with line-by-line feedback
- Unity 2D specific review criteria
- Turkish language output
- Performance optimization checks (GC allocation, Update loops, object pooling)
- Unity best practices validation

### Commands

```bash
# Install dependencies
npm install

# Run review script manually (requires diff.txt and env vars)
npm run review
# or:
node ai_review.js
```

### Environment Variables
- `OPENROUTER_API_KEY`: OpenRouter API key (GitHub secret)
- `GITHUB_TOKEN`: Auto-provided by GitHub Actions
- `GITHUB_EVENT_PATH`: Auto-provided (contains PR metadata)
- `GITHUB_REPOSITORY`: Auto-provided
- `PR_BASE_REF`: Base branch name
- `PR_HEAD_SHA`: PR head commit SHA

### Review Focus Areas
The bot checks for:
- **Gameplay & Performance**: MonoBehaviour lifecycle, Update/FixedUpdate usage, object pooling
- **Unity Best Practices**: Component pattern, GetComponent caching, ScriptableObject usage
- **Mobile Optimization**: GC allocations, LINQ performance, draw calls, memory leaks
- **Bugs & Edge Cases**: Null references, race conditions, platform-specific issues
- **Code Quality**: Readability, naming conventions, magic numbers
- **Common Pitfalls**: FindObjectOfType abuse, Animator usage, prefab instantiation

---

## Project 2: Unity 2D Sliding Tile Puzzle Game

### Overview
A classic sliding tile puzzle game (like the 15-puzzle) built with Unity 2D. Features configurable grid sizes, move counting, optional timer, and clean MVC-ish architecture.

### Project Structure

```
Assets/
└── Scripts/
    ├── Core/              # Core game logic
    │   ├── GameManager.cs       # Game state, win/lose conditions, singleton
    │   ├── GridManager.cs       # Grid setup, tile positioning, shuffle
    │   ├── TileController.cs    # Individual tile behavior
    │   └── InputHandler.cs      # Mouse/touch input handling
    ├── UI/                # User interface
    │   ├── UIManager.cs         # UI updates and panel management
    │   └── MenuController.cs    # Main menu and settings
    └── Utils/             # Utilities and data
        ├── AudioManager.cs      # Sound and music management
        └── GameData.cs          # ScriptableObject for configuration
```

### Architecture Principles

#### Separation of Concerns
- **Core**: Game logic and mechanics
- **UI**: Visual feedback and user interaction
- **Utils**: Shared utilities and configuration

#### Design Patterns
- **Singleton**: GameManager and AudioManager for global access
- **Component Pattern**: MonoBehaviour components with clear responsibilities
- **ScriptableObject**: Data-driven configuration via GameData
- **MVC-ish**: Separation between data (Grid), logic (GameManager), and view (UI)

#### Unity Best Practices Applied
- Component references cached in Awake()
- SerializeField for Inspector configuration
- Proper lifecycle method usage (Awake, Start, Update)
- No FindObjectOfType in Update loops
- Object pooling concepts (grid reuse)
- Minimal GC allocations (pre-allocated lists)
- Clean namespacing (PuzzleGame.Core, PuzzleGame.UI, PuzzleGame.Utils)

### Key Scripts Explained

#### GameManager.cs
- Singleton pattern for game state management
- Handles game flow: Menu → Playing → Won/Lost
- Tracks moves and timer
- Coordinates between GridManager and UIManager

#### GridManager.cs
- Creates and manages NxN tile grid
- Handles tile movement logic
- Shuffle algorithm (random valid moves for solvability)
- Win condition checking

#### TileController.cs
- Individual tile behavior
- Smooth movement with Lerp
- Click/touch detection via OnMouseDown
- Adjacent-to-empty validation

#### InputHandler.cs
- Alternative to OnMouseDown (more control)
- Supports both mouse and touch input
- Raycast-based tile selection
- Can be enabled/disabled

#### UIManager.cs
- Manages all UI panels (menu, game, pause, win, lose)
- Updates move counter and timer displays
- Button event handling

#### AudioManager.cs
- Singleton for global audio control
- Separate music and SFX channels
- Volume persistence via PlayerPrefs
- PlayOneShot for SFX (no GC allocation)

#### GameData.cs (ScriptableObject)
- Configurable game settings
- Difficulty presets (Easy/Medium/Hard)
- Visual configuration (colors, spacing)
- Validation in OnValidate()

### Setup Instructions for Unity

1. **Create Unity Project**
   - Unity 2D template
   - Unity 2021.3 LTS or newer recommended

2. **Import Scripts**
   - Copy entire `Assets/Scripts/` folder to your Unity project

3. **Create Basic Assets**
   - **Tile Sprite**: Simple square sprite (e.g., 100x100px white square)
   - **Tile Prefab**:
     - Create empty GameObject named "Tile"
     - Add SpriteRenderer component
     - Add BoxCollider2D (for OnMouseDown)
     - Add TileController script
     - Create TextMesh child for tile number
     - Save as prefab

4. **Scene Setup**
   - Create empty GameObject "GameManager", add GameManager script
   - Create empty GameObject "GridManager", add GridManager script
   - Create empty GameObject "AudioManager", add AudioManager script
   - Create Canvas for UI, add UIManager script
   - Create UI panels (Menu, Game, Pause, Win, Lose)
   - Add buttons and text elements

5. **Assign References in Inspector**
   - GameManager: Link GridManager, UIManager
   - GridManager: Link Tile Prefab, set grid size/spacing
   - UIManager: Link all panels and UI elements
   - TileController: Link SpriteRenderer and TextMesh

6. **Create GameData ScriptableObject** (optional)
   - Right-click in Project → Create → Puzzle Game → Game Data
   - Configure settings
   - Reference from GameManager if using

### Development Guidelines

#### Performance Considerations
- Tiles use Lerp for smooth movement (runs in Update only when moving)
- Grid array is pre-allocated in Awake
- Lists pre-sized where possible (e.g., `new List<Vector2Int>(4)`)
- No LINQ in hot paths
- GetComponent calls cached in Awake
- AudioManager uses PlayOneShot to avoid extra AudioSource allocation

#### Common Modifications

**Change Grid Size:**
- Modify `gridSize` in GridManager Inspector
- Adjust `tileSpacing` for visual layout

**Add New Tile Effects:**
- Modify `TileController.MoveTile()` for custom animations
- Add particle effects on tile movement

**Custom Shuffle:**
- Modify `GridManager.ShufflePuzzle()` for different shuffle logic

**Scoring System:**
- Track moves in GameManager
- Add timer penalties
- Store high scores via PlayerPrefs

### Testing
- Test with different grid sizes (3x3, 4x4, 5x5)
- Verify win detection works correctly
- Test on mobile (touch input)
- Check performance with larger grids

### Known Limitations
- No undo functionality (can be added)
- No hint system (can be added via A* solver)
- Shuffle uses random moves (not optimal, but ensures solvability)
- No animation between states (can add DOTween)

---

## Development Environment

- **Node.js**: >= 18.0.0 (for review bot)
- **Unity**: 2021.3 LTS or newer (for game)
- **Git**: For version control

## Common Workflows

### Working on Review Bot
```bash
# Make changes to ai_review.js
# Test locally with sample diff.txt
npm run review

# Commit and push
git add ai_review.js
git commit -m "feat: improve review logic"
git push
```

### Working on Unity Game
1. Open Unity project
2. Make script changes in IDE
3. Test in Unity Editor (Play mode)
4. Commit C# scripts via git

### Testing Review Bot on PRs
1. Create feature branch
2. Make changes to Unity scripts
3. Create PR
4. Bot will automatically review each commit
5. Check PR comments for feedback
