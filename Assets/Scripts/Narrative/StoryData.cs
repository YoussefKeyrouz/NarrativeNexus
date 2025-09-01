using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NarrativeNexus.Narrative
{
    /// <summary>
    /// ScriptableObject that holds all data for a branching narrative story
    /// </summary>
    [CreateAssetMenu(fileName = "NewStory", menuName = "Narrative Nexus/Story Data")]
    public class StoryData : ScriptableObject
    {
        [SerializeField] private string storyId;
        [SerializeField] private string title;
        [SerializeField] private string startNodeId;
        [SerializeField] private List<StoryNode> nodes = new List<StoryNode>();

        public string StoryId => storyId;
        public string Title => title;
        public string StartNodeId => startNodeId;
        public IReadOnlyList<StoryNode> Nodes => nodes;

        /// <summary>
        /// Find a node by its ID
        /// </summary>
        public StoryNode GetNode(string nodeId)
        {
            return nodes.FirstOrDefault(node => node.Id == nodeId);
        }

        /// <summary>
        /// Get the starting node of the story
        /// </summary>
        public StoryNode GetStartNode()
        {
            return GetNode(startNodeId);
        }

        /// <summary>
        /// Validate the story data for common issues
        /// </summary>
        public List<string> ValidateStory()
        {
            var issues = new List<string>();

            // Check for empty story ID
            if (string.IsNullOrEmpty(storyId))
            {
                issues.Add("Story ID is empty");
            }

            // Check for empty title
            if (string.IsNullOrEmpty(title))
            {
                issues.Add("Story title is empty");
            }

            // Check for empty start node ID
            if (string.IsNullOrEmpty(startNodeId))
            {
                issues.Add("Start node ID is empty");
            }

            // Check if start node exists
            if (!string.IsNullOrEmpty(startNodeId) && GetStartNode() == null)
            {
                issues.Add($"Start node '{startNodeId}' not found in nodes list");
            }

            // Check for duplicate node IDs
            var nodeIds = nodes.Where(n => !string.IsNullOrEmpty(n.Id)).Select(n => n.Id).ToList();
            var duplicateIds = nodeIds.GroupBy(id => id).Where(g => g.Count() > 1).Select(g => g.Key);
            foreach (var duplicateId in duplicateIds)
            {
                issues.Add($"Duplicate node ID found: '{duplicateId}'");
            }

            // Check for nodes with empty IDs
            var emptyIdNodes = nodes.Where(n => string.IsNullOrEmpty(n.Id)).ToList();
            if (emptyIdNodes.Any())
            {
                issues.Add($"Found {emptyIdNodes.Count} nodes with empty IDs");
            }

            // Check for orphaned target references
            var allTargetIds = new HashSet<string>();
            foreach (var node in nodes)
            {
                foreach (var choice in node.Choices)
                {
                    if (!string.IsNullOrEmpty(choice.TargetNodeId))
                    {
                        allTargetIds.Add(choice.TargetNodeId);
                    }
                }
            }

            foreach (var targetId in allTargetIds)
            {
                if (GetNode(targetId) == null)
                {
                    issues.Add($"Choice references missing node: '{targetId}'");
                }
            }

            return issues;
        }

        #region Editor Helper Methods
#if UNITY_EDITOR
        /// <summary>
        /// Add a new node to the story (Editor only)
        /// </summary>
        public void AddNode(StoryNode node)
        {
            if (node != null && !nodes.Contains(node))
            {
                nodes.Add(node);
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }

        /// <summary>
        /// Remove a node from the story (Editor only)
        /// </summary>
        public void RemoveNode(StoryNode node)
        {
            if (nodes.Remove(node))
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }

        /// <summary>
        /// Sort nodes by ID (Editor only)
        /// </summary>
        public void SortNodesByIds()
        {
            nodes = nodes.OrderBy(n => n.Id).ToList();
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
        #endregion
    }
}
