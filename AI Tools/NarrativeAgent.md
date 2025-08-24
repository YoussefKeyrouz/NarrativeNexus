# NarrativeAgent — Branching Story & Flow

## Role

Implement and evolve the **branching narrative engine**. You own data structures and runtime flow: `StoryData`, `StoryNode`, `Choice`, `NarrativeManager`, `IChoiceCondition`, and `GameState`.

## Mandatory Architecture

- **StoryData (ScriptableObject)**: holds metadata (id, title) and an array/list of `StoryNode`.
- **StoryNode**: `id`, `text`, `Choice[] choices`, optional visuals (bg sprite, audio cue).
- **Choice**: `text`, `targetNodeId`, optional `IChoiceCondition` reference, optional `Effect` list.
- **NarrativeManager (MonoBehaviour)**:
  - APIs: `LoadStory(StoryData)`, `StartStory()`, `GoToNode(nodeId)`, `SelectChoice(index)`, `GetCurrentNode()`.
  - Events/Callbacks for UI: `OnNodeChanged(StoryNode node, IReadOnlyList<Choice> availableChoices)`.
  - Enforces: no business logic in UI.
- **GameState** (`IGameState` + `GameState` impl):
  - Flags (string→bool), Stats (string→float/int).
  - Safe getters/setters and change events.

## Conditions & Effects

- **IChoiceCondition**: `bool IsMet(IGameState state)`.
  - Provide base types: `FlagCondition(name, expected)`, `StatThresholdCondition(name, op, value)`.
- **IEffect** (future-proof): `void Apply(IGameState state)`.
  - Examples: `SetFlagEffect`, `ModifyStatEffect`.

## Data Rules

- Node IDs are unique within a story.
- Choices reference `targetNodeId` only; never direct object links in code.
- No text in code; always read from data.

## Save/Load (baseline hooks)

- `NarrativeSnapshot` { storyId, nodeId, flags, stats }.
- `CreateSnapshot()` and `RestoreSnapshot(snapshot)` on `NarrativeManager`.
- Actual I/O lives in SaveSystemAgent.

## Deliverables (when asked)

- Full C# for the above classes.
- ScriptableObject inspectors (optional) that validate unique node IDs.
- Minimal sample `StoryData` and two nodes wired for testing.

## Testing Checklist

- Start from first node, make a choice, land on correct target node.
- Condition-locked choices are hidden/disabled according to spec.
- Effects mutate `GameState` and unlock a gated choice later.
- Snapshot taken/restored yields identical state.
