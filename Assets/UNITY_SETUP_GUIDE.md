# Unity Setup Guide - Sliding Tile Puzzle

Step-by-step instructions for setting up the puzzle game in Unity.

## Prerequisites
- Unity 2021.3 LTS or newer
- Unity 2D template

## Step 1: Import Scripts ✅

The scripts are already in `Assets/Scripts/`. Unity will compile them automatically.

## Step 2: Create Tile Sprite

1. Create a simple square sprite:
   - In your image editor: Create 100x100px white square with black border
   - Or use Unity: GameObject → 2D Object → Sprite → Square

2. Save sprite in `Assets/Sprites/` folder

## Step 3: Create Tile Prefab

1. **Create Tile GameObject**
   - Right-click in Hierarchy → Create Empty
   - Name it "Tile"

2. **Add Components**
   - Add Component → Rendering → Sprite Renderer
     - Assign your tile sprite
     - Order in Layer: 0

   - Add Component → Physics 2D → Box Collider 2D
     - Size: 1, 1 (matches sprite)

   - Add Component → Scripts → Tile Controller

3. **Create Number Text**
   - Right-click Tile → 3D Object → 3D Text
   - Name it "NumberText"
   - Position: (0, 0, -0.1)
   - Font Size: 50
   - Alignment: Center, Middle
   - Color: Black

4. **Configure TileController**
   - Drag SpriteRenderer to "Sprite Renderer" field
   - Drag NumberText to "Number Text" field
   - Move Speed: 10

5. **Save as Prefab**
   - Drag Tile from Hierarchy to `Assets/Prefabs/` folder
   - Delete Tile from Hierarchy

## Step 4: Setup Main Scene

### Create Core GameObjects

1. **GameManager**
   - Create Empty GameObject
   - Name: "GameManager"
   - Add Component → Game Manager
   - Position: (0, 0, 0)

2. **GridManager**
   - Create Empty GameObject
   - Name: "GridManager"
   - Add Component → Grid Manager
   - Position: (0, 0, 0)
   - **Configure**:
     - Grid Size: 3
     - Tile Spacing: 1.1
     - Grid Offset: (-1, -1) to center
     - Tile Prefab: Drag your Tile prefab here

3. **AudioManager**
   - Create Empty GameObject
   - Name: "AudioManager"
   - Add Component → Audio Manager
   - Position: (0, 0, 0)

4. **InputHandler** (optional, TileController has OnMouseDown)
   - Create Empty GameObject
   - Name: "InputHandler"
   - Add Component → Input Handler

### Connect References

In GameManager Inspector:
- Grid Manager: Drag GridManager GameObject
- UI Manager: (will set after UI is created)

## Step 5: Setup UI

### Create Canvas

1. **Create Canvas**
   - Right-click Hierarchy → UI → Canvas
   - Canvas Scaler → Scale With Screen Size
   - Reference Resolution: 1920x1080

2. **Add UI Manager**
   - Select Canvas
   - Add Component → UI Manager

### Create UI Panels

Create these panels as children of Canvas:

#### 1. Menu Panel
- Right-click Canvas → UI → Panel
- Name: "MenuPanel"
- **Add children**:
  - Text: "Sliding Tile Puzzle" (title)
  - Button: "Play" → Add to MenuController playButton
  - Button: "Settings" → Add to MenuController settingsButton
  - Button: "Quit" → Add to MenuController quitButton

#### 2. Game Panel
- Right-click Canvas → UI → Panel
- Name: "GamePanel"
- Disable by default (uncheck)
- **Add children**:
  - Text: "Moves: 0" → Name "MoveCountText"
  - Text: "Time: 00:00" → Name "TimerText"
  - Button: "Pause" → pauseButton

#### 3. Pause Panel
- Right-click Canvas → UI → Panel
- Name: "PausePanel"
- Disable by default
- **Add children**:
  - Text: "Paused"
  - Button: "Resume" → resumeButton
  - Button: "Restart" → restartButton
  - Button: "Menu"

#### 4. Win Panel
- Right-click Canvas → UI → Panel
- Name: "WinPanel"
- Disable by default
- **Add children**:
  - Text: "You Win!"
  - Text: "Moves: 0" → Name "WinMoveCountText"
  - Text: "Time: 00:00" → Name "WinTimeText"
  - Button: "Play Again"
  - Button: "Menu"

#### 5. Lose Panel
- Right-click Canvas → UI → Panel
- Name: "LosePanel"
- Disable by default
- **Add children**:
  - Text: "Time's Up!"
  - Button: "Try Again"
  - Button: "Menu"

### Add Menu Controller

1. Create Empty GameObject under Canvas
2. Name: "MenuController"
3. Add Component → Menu Controller
4. Connect all button references

### Configure UIManager

Select Canvas, in UIManager:
- Menu Panel: Drag MenuPanel
- Game Panel: Drag GamePanel
- Pause Panel: Drag PausePanel
- Win Panel: Drag WinPanel
- Lose Panel: Drag LosePanel
- Move Count Text: Drag MoveCountText
- Timer Text: Drag TimerText
- Win Move Count Text: Drag WinMoveCountText
- Win Time Text: Drag WinTimeText
- Pause Button: Drag pause button
- Resume Button: Drag resume button
- Restart Button: Drag restart button

## Step 6: Configure Camera

1. Select Main Camera
2. **Orthographic** view
3. Size: 3 (for 3x3 grid)
4. Position: (0, 0, -10)
5. Background: Color of your choice

## Step 7: Final Connections

1. **GameManager**
   - Grid Manager: GridManager GameObject
   - UI Manager: Canvas (with UIManager component)
   - Use Timer: False (or True for timer mode)
   - Max Time: 300

2. **Save Scene**
   - File → Save Scene As → "MainScene"

## Step 8: Test!

1. Press Play
2. Click "Play" button
3. Click tiles adjacent to empty space
4. Solve the puzzle!

## Optional Enhancements

### Create GameData ScriptableObject

1. Right-click in Project → Create → Puzzle Game → Game Data
2. Configure settings
3. Use in GameManager for data-driven design

### Add Audio

1. Import audio clips to `Assets/Audio/`
2. Select AudioManager
3. Assign clips:
   - Music clips (menu/game)
   - SFX clips (tile move, button, win, lose)

### Customize Visuals

1. **Tile Colors**
   - Edit Tile prefab sprite
   - Or change SpriteRenderer color

2. **UI Theme**
   - Customize panel colors
   - Change button styles
   - Add images/icons

3. **Animations**
   - Add Animator to buttons
   - Create panel transitions
   - Add particle effects on win

## Troubleshooting

### Tiles don't move when clicked
- Check BoxCollider2D is on tile
- Verify TileController script attached
- Ensure camera has Physics2D Raycaster (for UI)

### UI doesn't show
- Check Canvas enabled
- Verify correct panel is active
- Check UIManager references

### Compile errors
- Check all scripts in correct folders
- Verify namespace usage
- Check Unity version compatibility

### Grid not centered
- Adjust GridManager → Grid Offset
- Change camera size/position

## Performance Tips

- Use sprite atlases for multiple sprites
- Keep grid size ≤ 5x5 for mobile
- Disable unused UI panels
- Use object pooling for particles

## Next Steps

- Add difficulty selection
- Implement hint system
- Add animations with DOTween
- Create level progression
- Add leaderboard
- Implement undo feature

Happy puzzle building! 🎮
