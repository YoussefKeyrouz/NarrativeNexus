# UI Prefab Setup Instructions

## ChoiceButton_Prefab Setup

1. Create a Button in the Hierarchy (UI > Button - TextMeshPro)
2. Rename it to 'ChoiceButton_Prefab'
3. Ensure the Button has the following structure:
   - ChoiceButton_Prefab (Button component)
     - Text (TMP) (TextMeshProUGUI component)

4. Configure the Button:
   - Set appropriate colors for Normal, Highlighted, Pressed states
   - Adjust the Image component's Source Image if desired

5. Configure the TextMeshProUGUI:
   - Set appropriate font size (recommended: 18-24)
   - Set text alignment to Center/Middle
   - Set text color to a readable color (e.g., black or white)
   - Enable 'Auto Size' if you want text to scale

6. Drag the configured Button from Hierarchy to Assets/Prefabs/UI/ to create the prefab

## StoryButton_Prefab Setup (for MainMenu)

1. Create a Button in the Hierarchy (UI > Button - TextMeshPro)
2. Rename it to 'StoryButton_Prefab'
3. Follow similar structure as ChoiceButton_Prefab
4. Consider making this button larger/more prominent for story selection
5. Save as prefab in Assets/Prefabs/UI/

## Canvas Setup Tips

### MainMenu Scene:
- Canvas (Screen Space - Overlay)
  - Panel_Background (Image - full screen)
    - ScrollRect_Stories
      - Viewport
        - Content (Vertical Layout Group + Content Size Fitter)

### StoryGameplay Scene:
- Canvas (Screen Space - Overlay)
  - Panel_Story (Image - background panel)
    - StoryText (TextMeshProUGUI - story content)
    - ChoicesContainer (Vertical Layout Group - for choice buttons)
  - BackgroundImage (Image - optional, for story backgrounds)

## Component Settings:

### Vertical Layout Group (for choice containers):
- Spacing: 10-15
- Child Alignment: Upper Center
- Control Child Size: Width = true, Height = false
- Use Child Scale: false
- Child Force Expand: Width = true, Height = false

### Content Size Fitter (for scroll content):
- Horizontal Fit: Unconstrained
- Vertical Fit: Preferred Size

### ScrollRect:
- Content: Assign the Content GameObject
- Viewport: Assign the Viewport GameObject
- Horizontal: false
- Vertical: true
