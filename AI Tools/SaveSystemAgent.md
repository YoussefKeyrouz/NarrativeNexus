# SaveSystemAgent — Narrative Saves

## Role

Implement persistent save/load for the narrative state.

## Constraints

- Do NOT embed platform SDKs unless asked.
- Default to JSON in Application.persistentDataPath.
- One autosave slot + N manual slots (configurable).
- Backward-compatible schema (versioned payload).

## Data Contract

```json
{
  "version": 1,
  "storyId": "string",
  "nodeId": "string",
  "flags": {"key": true},
  "stats": {"key": 0.0},
  "timestamp": "ISO-8601"
}

