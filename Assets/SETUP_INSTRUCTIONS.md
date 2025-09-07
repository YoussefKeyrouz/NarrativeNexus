# Narrative Nexus - Setup Instructions

## Prerequisites

- Unity 2022.3 LTS or newer
- URP 2D project template
- TextMeshPro package (will be prompted to install)

## Manual Unity Setup Steps

### 1. Install TextMeshPro

1. Open **Window > Package Manager**
2. Search for "TextMeshPro" 
3. Click **Install**
4. When prompted, click **Import TMP Essentials**

### 2. Create Scenes

1. In **Assets/Scenes/**, create two new scenes:
   - Right-click > **Create > Scene** → Name: `MainMenu`
   - Right-click > **Create > Scene** → Name: `StoryGameplay`

### 3. Setup MainMenu Scene

1. Open `MainMenu.unity`
2. **Create Canvas:**
   - Hierarchy → **UI > Canvas**
   - Set Canvas **Render Mode** to "Screen Space - Overlay"
3. **Create Background Panel:**
   - Right-click Canvas → **UI > Panel**
   - Rename to "Panel_Background"
   - Set **Anchors** to stretch full screen
4. **Create Scroll View:**
   - Right-click Panel_Background → **UI > Scroll View**
   - Rename to "ScrollRect_Stories"
   - Delete the default **Scrollbar Horizontal**
   - In **Content** child object:
     - Add **Vertical Layout Group** component
     - Add **Content Size Fitter** component
     - Set Content Size Fitter: **Vertical Fit = Preferred Size**
     - Set Vertical Layout Group: **Spacing = 10**, **Child Force Expand Width = true**
5. **Create Story Button Prefab:**
   - Right-click Hierarchy → **UI > Button - TextMeshPro**
   - Rename to "StoryButton_Prefab"
   - Configure button appearance (colors, size)
   - Drag from Hierarchy to **Assets/Prefabs/UI/** to create prefab
   - Delete the instance from Hierarchy
6. **Add StorySelectionUI Script:**
   - Create empty GameObject, rename to "StorySelection"
   - Add **StorySelectionUI** component
   - Assign **Content** = ScrollRect_Stories/Viewport/Content
   - Assign **Story Button Prefab** = your StoryButton_Prefab
   - Leave **Available Stories** empty for now (will add after creating sample story)

### 4. Setup StoryGameplay Scene

1. Open `StoryGameplay.unity`
2. **Create Canvas:**
   - Hierarchy → **UI > Canvas** (Screen Space - Overlay)
3. **Create Story UI:**
   - Right-click Canvas → **UI > Panel**
   - Rename to "Panel_Story"
   - Right-click Panel_Story → **UI > Text - TextMeshPro**
   - Rename to "StoryText"
   - Configure text: larger font size, word wrapping, appropriate margins
4. **Create Choices Container:**
   - Right-click Canvas → **Create Empty**
   - Rename to "ChoicesContainer"
   - Add **Vertical Layout Group** component
   - Set **Spacing = 10**, **Child Force Expand Width = true**
5. **Create Choice Button Prefab:**
   - Right-click Hierarchy → **UI > Button - TextMeshPro**
   - Rename to "ChoiceButton_Prefab"
   - Configure appearance
   - Drag to **Assets/Prefabs/UI/** to create prefab
   - Delete instance from Hierarchy
6. **Create Background Image (Optional):**
   - Right-click Canvas → **UI > Image**
   - Rename to "BackgroundImage"
   - Set **Source Image** to None initially
   - Move to top of Canvas children list (render behind other UI)
7. **Add Narrative System:**
   - Create empty GameObject, rename to "NarrativeSystem"
   - Add **NarrativeManager** component
   - Add **StoryUIController** component
   - In StoryUIController:
     - Assign **Narrative** = NarrativeManager on same GameObject
     - Assign **Story Text** = StoryText
     - Assign **Choices Container** = ChoicesContainer
     - Assign **Choice Button Prefab** = ChoiceButton_Prefab
     - Assign **Background Image** = BackgroundImage (if created)

### 5. Build Settings

1. **File > Build Settings**
2. Click **Add Open Scenes** while MainMenu is open
3. Open StoryGameplay scene, click **Add Open Scenes** again
4. Ensure **MainMenu** is index 0, **StoryGameplay** is index 1

### 6. Create Sample Story

1. In Unity menu: **Narrative Nexus > Create Sample Story**
2. This creates `Assets/StoryData/Sample_ForestAdventure.asset`
3. Return to MainMenu scene
4. Select "StorySelection" GameObject
5. In **StorySelectionUI** component:
   - Set **Available Stories** size to 1
   - Assign **Element 0** = Sample_ForestAdventure asset

## Testing the Setup

### Basic Flow Test:

1. **Play MainMenu scene**
2. **Verify:** Story button appears with "Sample: Forest Adventure"
3. **Click the story button**
4. **Verify:** Scene loads to StoryGameplay
5. **Verify:** Story text appears: "You find yourself at the edge of a mysterious forest..."
6. **Verify:** Two choice buttons appear
7. **Click a choice**
8. **Verify:** Story advances to next node
9. **Continue until reaching an end node**
10. **Verify:** "Return to Main Menu" button appears
11. **Click return button**
12. **Verify:** Returns to MainMenu scene

### Validation Test:

1. **Narrative Nexus > Validate All Stories**
2. **Check Console:** Should show "✓ Sample: Forest Adventure - No issues found"

## Troubleshooting

### Common Issues:

- **"No TextMeshPro found"**: Install TMP package and import essentials
- **"Story button has no text"**: Ensure button prefab has TextMeshProUGUI child
- **"No pending story found"**: Make sure GameManager.PendingStory is set in StorySelectionUI
- **"Missing references"**: Double-check all component assignments in inspectors

### Console Warnings to Ignore:

- Shader compilation warnings (normal for URP)
- "No story loaded" when first opening StoryGameplay scene directly

## Next Steps

### Adding More Stories:

1. **Create > Narrative Nexus > Story Data**
2. Fill in Story ID, Title, Start Node ID
3. Add nodes with unique IDs
4. Add choices with target node IDs
5. Use **Validate Story** button in inspector
6. Add to StorySelectionUI's Available Stories array

### Adding Conditions:

1. In Choice component, expand **Condition**
2. Set **Condition Type** to "Flag"
3. Set **Flag Key** and **Flag Value**
4. Use GameState.SetFlag() in code to control availability

### Custom Editor Tools:

- **Narrative Nexus > Create Sample Story**: Creates demo content
- **Narrative Nexus > Validate All Stories**: Checks all story assets
- **Narrative Nexus > Create Prefab Setup README**: Detailed prefab instructions
