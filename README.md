# 2D Unity Puzzle Game + AI Code Review Bot

This repository contains two projects:

## 🎮 Unity 2D Sliding Tile Puzzle Game

A classic sliding tile puzzle game (like the 15-puzzle) with configurable difficulty levels.

### Features
- ✅ Configurable grid sizes (3x3, 4x4, 5x5)
- ✅ Move counter
- ✅ Optional timer mode
- ✅ Clean MVC-ish architecture
- ✅ Mobile-friendly (touch + mouse input)
- ✅ Audio system (music + SFX)
- ✅ Settings menu with persistence
- ✅ Win/lose detection

### Quick Start

1. **Create Unity 2D Project** (Unity 2021.3+ recommended)

2. **Copy Scripts**
   ```bash
   # Copy the Assets folder to your Unity project
   ```

3. **Create Tile Prefab**
   - Create square sprite (100x100px)
   - Create GameObject with:
     - SpriteRenderer
     - BoxCollider2D
     - TileController script
     - TextMesh child for number
   - Save as prefab

4. **Setup Scene**
   - Add GameManager GameObject with GameManager script
   - Add GridManager GameObject with GridManager script
   - Add AudioManager GameObject with AudioManager script
   - Create Canvas with UIManager
   - Create UI panels (Menu, Game, Pause, Win, Lose)

5. **Configure in Inspector**
   - Assign all references
   - Set grid size and spacing
   - Link UI elements

6. **Play!**

### Script Organization

```
Assets/Scripts/
├── Core/               # Game logic
│   ├── GameManager.cs        # Game state management (Singleton)
│   ├── GridManager.cs        # Grid and tile management
│   ├── TileController.cs     # Individual tile behavior
│   └── InputHandler.cs       # Input handling
├── UI/                 # User interface
│   ├── UIManager.cs          # UI panel management
│   └── MenuController.cs     # Menu and settings
└── Utils/              # Utilities
    ├── AudioManager.cs       # Audio management (Singleton)
    └── GameData.cs           # Configuration ScriptableObject
```

---

## 🤖 AI Code Review Bot

Automated code reviewer for Unity 2D projects using Claude 3.5 Sonnet.

### Features
- ✅ Per-commit reviews
- ✅ Unity 2D specialized feedback
- ✅ `.aiignore` support
- ✅ Performance optimization checks
- ✅ Turkish language output
- ✅ GitHub Actions integration

### Setup

1. **Add Repository Secrets**
   - `OPENROUTER_API_KEY`: Your OpenRouter API key

2. **Create `.aiignore`** (optional)
   ```
   # Ignore generated files
   *.meta
   Library/
   Temp/

   # Ignore specific paths
   Assets/ThirdParty/
   ```

3. **Open a Pull Request**
   - Bot automatically reviews each commit
   - Comments appear on the PR

### Review Criteria

The bot checks for:
- **Performance**: GC allocations, Update loops, object pooling
- **Unity Best Practices**: Component caching, lifecycle methods
- **Mobile Optimization**: LINQ usage, draw calls, memory
- **Code Quality**: Naming, organization, magic numbers
- **Bugs**: Null references, race conditions, edge cases

### Commands

```bash
# Install dependencies
npm install

# Run review locally
node ai_review.js
```

---

## 📋 Requirements

- **Unity**: 2021.3 LTS or newer (for game)
- **Node.js**: >= 18.0.0 (for review bot)
- **Git**: For version control

## 📖 Documentation

See [CLAUDE.md](CLAUDE.md) for detailed documentation and architecture information.

## 🛠️ Development

### Working on the Puzzle Game
1. Open Unity project
2. Edit scripts in your IDE
3. Test in Play mode
4. Commit changes

### Working on the Review Bot
1. Edit `ai_review.js`
2. Test with sample diffs
3. Create PR to test on actual code

## 📝 License

This is a demo project. Feel free to use and modify as needed.
