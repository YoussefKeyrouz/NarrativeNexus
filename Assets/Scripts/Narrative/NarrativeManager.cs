using System;
using System.Collections.Generic;
using UnityEngine;

namespace NarrativeNexus.Narrative
{
    /// <summary>
    /// Main controller for the narrative system runtime
    /// </summary>
    public class NarrativeManager : MonoBehaviour
    {
        [SerializeField] private GameState gameState = new GameState();

        private StoryData currentStory;
        private StoryNode currentNode;

        // Properties
        public StoryData CurrentStory => currentStory;
        public StoryNode CurrentNode => currentNode;
        public IGameState GameState => gameState;

        // Events
        public event Action<StoryNode, List<Choice>> OnNodeChanged;

        #region Public API
        /// <summary>
        /// Load a story data asset
        /// </summary>
        public void LoadStory(StoryData storyData)
        {
            if (storyData == null)
            {
                Debug.LogError("Cannot load null story data");
                return;
            }

            currentStory = storyData;
            currentNode = null;

            Debug.Log($"Loaded story: {currentStory.Title}");
        }

        /// <summary>
        /// Start the loaded story from the beginning
        /// </summary>
        public void StartStory()
        {
            if (currentStory == null)
            {
                Debug.LogError("No story loaded. Call LoadStory() first.");
                return;
            }

            var startNode = currentStory.GetStartNode();
            if (startNode == null)
            {
                Debug.LogError($"Start node '{currentStory.StartNodeId}' not found in story '{currentStory.Title}'");
                return;
            }

            GoToNode(currentStory.StartNodeId);
        }

        /// <summary>
        /// Navigate to a specific node by ID
        /// </summary>
        public void GoToNode(string nodeId)
        {
            if (currentStory == null)
            {
                Debug.LogError("No story loaded");
                return;
            }

            var targetNode = currentStory.GetNode(nodeId);
            if (targetNode == null)
            {
                Debug.LogError($"Node '{nodeId}' not found in story '{currentStory.Title}'");
                return;
            }

            currentNode = targetNode;
            var availableChoices = currentNode.GetAvailableChoices(gameState);

            Debug.Log($"Moved to node: {nodeId}");
            OnNodeChanged?.Invoke(currentNode, availableChoices);
        }

        /// <summary>
        /// Select a choice by index from the available choices
        /// </summary>
        public void SelectChoice(int choiceIndex)
        {
            if (currentNode == null)
            {
                Debug.LogError("No current node");
                return;
            }

            var availableChoices = currentNode.GetAvailableChoices(gameState);
            
            if (choiceIndex < 0 || choiceIndex >= availableChoices.Count)
            {
                Debug.LogError($"Choice index {choiceIndex} out of range. Available choices: {availableChoices.Count}");
                return;
            }

            var selectedChoice = availableChoices[choiceIndex];
            Debug.Log($"Selected choice: {selectedChoice.Text}");

            // Navigate to target node
            GoToNode(selectedChoice.TargetNodeId);
        }

        /// <summary>
        /// Get the current node (null if no story loaded or started)
        /// </summary>
        public StoryNode GetCurrentNode()
        {
            return currentNode;
        }
        #endregion

        #region Save/Load Support
        /// <summary>
        /// Create a snapshot of the current narrative state
        /// </summary>
        public NarrativeSnapshot CreateSnapshot()
        {
            if (currentStory == null || currentNode == null)
            {
                return null;
            }

            return new NarrativeSnapshot
            {
                storyId = currentStory.StoryId,
                nodeId = currentNode.Id,
                flags = gameState.GetAllFlags(),
                stats = gameState.GetAllStats()
            };
        }

        /// <summary>
        /// Restore narrative state from a snapshot
        /// </summary>
        public bool RestoreSnapshot(NarrativeSnapshot snapshot)
        {
            if (snapshot == null)
            {
                Debug.LogError("Cannot restore null snapshot");
                return false;
            }

            if (currentStory == null || currentStory.StoryId != snapshot.storyId)
            {
                Debug.LogError($"Cannot restore snapshot: story mismatch. Current: {currentStory?.StoryId}, Snapshot: {snapshot.storyId}");
                return false;
            }

            // Restore game state
            gameState.ClearAll();
            foreach (var flag in snapshot.flags)
            {
                gameState.SetFlag(flag.Key, flag.Value);
            }
            foreach (var stat in snapshot.stats)
            {
                gameState.SetStat(stat.Key, stat.Value);
            }

            // Navigate to saved node
            GoToNode(snapshot.nodeId);

            Debug.Log($"Restored snapshot: {snapshot.storyId} at node {snapshot.nodeId}");
            return true;
        }
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (gameState == null)
            {
                gameState = new GameState();
            }
        }
        #endregion
    }

    /// <summary>
    /// Serializable snapshot of narrative state for save/load
    /// </summary>
    [System.Serializable]
    public class NarrativeSnapshot
    {
        public string storyId;
        public string nodeId;
        public Dictionary<string, bool> flags;
        public Dictionary<string, float> stats;
    }
}
