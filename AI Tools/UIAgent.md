# UIAgent — Story Selection & In-Story UI

## Role

Build clean UGUI for:

1) **Story Selection Screen** (MainMenu scene)
2) **In-Story UI** (StoryGameplay scene)

## Standards

- Use TextMeshPro.
- Responsive anchors, vertical layout groups where applicable.
- UI scripts are thin and call `NarrativeManager` methods.

## Story Selection (MainMenu)

### Prefabs/Objects

- `Canvas` (Screen Space - Overlay) with `Panel_Background`.
- `ScrollRect_Stories` → `Viewport` → `Content` (VerticalLayoutGroup + ContentSizeFitter).
- `StoryButton_Prefab` (Button + TMP Text).

### Script

- `StorySelectionUI.cs`
  - Serialized: `Transform content`, `Button storyButtonPrefab`, `StoryData[] availableStories`.
  - On Start(): instantiate one button per `StoryData` (title text).
  - On click: save chosen `StoryData` (via a `GameManager` or static holder) and `SceneManager.LoadScene("StoryGameplay")`.

## In-Story UI (StoryGameplay)

### Prefabs / Objects

- `Canvas` with:
  - `Panel_Story` containing `TMP_Text StoryText`.
  - `VerticalLayoutGroup ChoicesContainer` holding `ChoiceButton_Prefab` (Button + TMP Text).
  - Optional `Image BackgroundImage`.

### Scripts

- `StoryUIController.cs`
  - Serialized: `NarrativeManager narrative`, `TMP_Text storyText`, `Transform choicesContainer`, `Button choiceButtonPrefab`, `Image backgroundImage`.
  - On enable: subscribe to `narrative.OnNodeChanged`.
  - Render node text, clear/rebuild choices; wire buttons to `narrative.SelectChoice(index)`.

## Manual Steps (when asked)

- Exact menu paths for creating Canvas/UI.
- Add TMP via Package Manager if needed.
- Drag references into inspector (prefabs, manager, text fields).

## Testing Checklist

- Stories list populates; clicking loads gameplay scene.
- First node shows; choices appear with correct labels.
- Selecting a choice advances to correct node.
- Disabled/hidden state for unmet conditions (as specified).
