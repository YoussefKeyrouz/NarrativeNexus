#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NarrativeNexus.Narrative;

namespace NarrativeNexus.Editor.StoryBuilder
{
    /// <summary>
    /// Internal editing model for a story, decoupled from ScriptableObject assets
    /// </summary>
    [System.Serializable]
    public class EditableStory
    {
        public string storyId = "";
        public string title = "";
        public string startNodeId = "";
        public List<EditableNode> nodes = new List<EditableNode>();

        public EditableStory()
        {
        }

        public EditableStory(string id, string storyTitle)
        {
            storyId = id;
            title = storyTitle;
        }

        /// <summary>
        /// Get node by ID
        /// </summary>
        public EditableNode GetNode(string nodeId)
        {
            return nodes.FirstOrDefault(n => n.id == nodeId);
        }

        /// <summary>
        /// Get all node IDs
        /// </summary>
        public List<string> GetAllNodeIds()
        {
            return nodes.Select(n => n.id).ToList();
        }

        /// <summary>
        /// Check if a node ID already exists
        /// </summary>
        public bool HasNodeId(string nodeId)
        {
            return nodes.Any(n => n.id == nodeId);
        }

        /// <summary>
        /// Remove a node by ID and clean up any choices pointing to it
        /// </summary>
        public void RemoveNode(string nodeId)
        {
            // Remove the node
            nodes.RemoveAll(n => n.id == nodeId);

            // Clean up choices pointing to this node
            foreach (var node in nodes)
            {
                node.choices.RemoveAll(c => c.targetNodeId == nodeId);
            }

            // Clear start node if it was pointing to the deleted node
            if (startNodeId == nodeId)
            {
                startNodeId = "";
            }
        }

        /// <summary>
        /// Remap all references from oldId to newId
        /// </summary>
        public void RemapNodeId(string oldId, string newId)
        {
            // Update start node if it matches
            if (startNodeId == oldId)
            {
                startNodeId = newId;
            }

            // Update all choice targets
            foreach (var node in nodes)
            {
                foreach (var choice in node.choices)
                {
                    if (choice.targetNodeId == oldId)
                    {
                        choice.targetNodeId = newId;
                    }
                }
            }
        }

        /// <summary>
        /// Validate the story and return a list of issues
        /// </summary>
        public List<string> ValidateStory()
        {
            var issues = new List<string>();

            // Check for empty story ID
            if (string.IsNullOrEmpty(storyId))
            {
                issues.Add("Story ID is required");
            }

            // Check for duplicate node IDs
            var nodeIds = nodes.Select(n => n.id).ToList();
            var duplicateIds = nodeIds.GroupBy(id => id)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);

            foreach (var duplicateId in duplicateIds)
            {
                issues.Add($"Duplicate node ID: {duplicateId}");
            }

            // Check for missing or invalid start node
            if (string.IsNullOrEmpty(startNodeId))
            {
                issues.Add("Start node is not set");
            }
            else if (!HasNodeId(startNodeId))
            {
                issues.Add($"Start node '{startNodeId}' does not exist");
            }

            // Check for invalid choice targets
            foreach (var node in nodes)
            {
                foreach (var choice in node.choices)
                {
                    if (!string.IsNullOrEmpty(choice.targetNodeId) && !HasNodeId(choice.targetNodeId))
                    {
                        issues.Add($"Node '{node.id}' has choice targeting non-existent node '{choice.targetNodeId}'");
                    }
                }
            }

            // Check for unreachable nodes (nodes with no incoming edges)
            var reachableNodes = new HashSet<string>();
            if (!string.IsNullOrEmpty(startNodeId))
            {
                reachableNodes.Add(startNodeId);
            }

            foreach (var node in nodes)
            {
                foreach (var choice in node.choices)
                {
                    if (!string.IsNullOrEmpty(choice.targetNodeId))
                    {
                        reachableNodes.Add(choice.targetNodeId);
                    }
                }
            }

            var unreachableNodes = nodeIds.Where(id => !reachableNodes.Contains(id) && id != startNodeId);
            foreach (var unreachableId in unreachableNodes)
            {
                issues.Add($"Node '{unreachableId}' is unreachable (no incoming connections)");
            }

            return issues;
        }
    }

    /// <summary>
    /// Internal editing model for a story node
    /// </summary>
    [System.Serializable]
    public class EditableNode
    {
        public string id = "";
        public string text = "";
        public Sprite background;
        public List<EditableChoice> choices = new List<EditableChoice>();

        public EditableNode()
        {
        }

        public EditableNode(string nodeId, string nodeText = "")
        {
            id = nodeId;
            text = nodeText;
        }

        /// <summary>
        /// Add a new choice to this node
        /// </summary>
        public void AddChoice(EditableChoice choice)
        {
            choices.Add(choice);
        }

        /// <summary>
        /// Remove a choice from this node
        /// </summary>
        public void RemoveChoice(EditableChoice choice)
        {
            choices.Remove(choice);
        }

        /// <summary>
        /// Get a short preview of the node text (for display in lists)
        /// </summary>
        public string GetTextPreview(int maxLength = 24)
        {
            if (string.IsNullOrEmpty(text))
                return "<empty>";

            var preview = text.Replace('\n', ' ').Replace('\r', ' ');
            if (preview.Length > maxLength)
            {
                preview = preview.Substring(0, maxLength - 3) + "...";
            }
            return preview;
        }
    }

    /// <summary>
    /// Internal editing model for a choice
    /// </summary>
    [System.Serializable]
    public class EditableChoice
    {
        public string text = "";
        public string targetNodeId = "";

        public EditableChoice()
        {
        }

        public EditableChoice(string choiceText, string target = "")
        {
            text = choiceText;
            targetNodeId = target;
        }
    }

    /// <summary>
    /// Utility class for converting between editing models and ScriptableObject assets
    /// </summary>
    public static class StoryBuilderConverter
    {
        /// <summary>
        /// Convert a StoryData ScriptableObject to an EditableStory
        /// </summary>
        public static EditableStory FromStoryData(StoryData storyData)
        {
            if (storyData == null)
                return new EditableStory();

            var editableStory = new EditableStory
            {
                storyId = storyData.StoryId ?? "",
                title = storyData.Title ?? "",
                startNodeId = storyData.StartNodeId ?? ""
            };

            foreach (var node in storyData.Nodes)
            {
                var editableNode = new EditableNode
                {
                    id = node.Id ?? "",
                    text = node.Text ?? "",
                    background = node.Background
                };

                foreach (var choice in node.Choices)
                {
                    editableNode.choices.Add(new EditableChoice
                    {
                        text = choice.Text ?? "",
                        targetNodeId = choice.TargetNodeId ?? ""
                    });
                }

                editableStory.nodes.Add(editableNode);
            }

            return editableStory;
        }

        /// <summary>
        /// Convert an EditableStory to a StoryData ScriptableObject
        /// </summary>
        public static StoryData ToStoryData(EditableStory editableStory, string assetPath)
        {
            if (editableStory == null)
                return null;

            var storyData = ScriptableObject.CreateInstance<StoryData>();

            // Use reflection to set private fields
            var storyDataType = typeof(StoryData);
            var storyIdField = storyDataType.GetField("storyId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var titleField = storyDataType.GetField("title", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var startNodeIdField = storyDataType.GetField("startNodeId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var nodesField = storyDataType.GetField("nodes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            storyIdField?.SetValue(storyData, editableStory.storyId);
            titleField?.SetValue(storyData, editableStory.title);
            startNodeIdField?.SetValue(storyData, editableStory.startNodeId);

            var storyNodes = new List<StoryNode>();
            foreach (var editableNode in editableStory.nodes)
            {
                var storyNode = new StoryNode(editableNode.id, editableNode.text);
                
                // Set background using reflection
                var nodeType = typeof(StoryNode);
                var backgroundField = nodeType.GetField("background", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                backgroundField?.SetValue(storyNode, editableNode.background);

                foreach (var editableChoice in editableNode.choices)
                {
                    var choice = new Choice(editableChoice.text, editableChoice.targetNodeId);
                    storyNode.AddChoice(choice);
                }

                storyNodes.Add(storyNode);
            }

            nodesField?.SetValue(storyData, storyNodes);
            return storyData;
        }

        /// <summary>
        /// Update an existing StoryData with data from an EditableStory
        /// </summary>
        public static void UpdateStoryData(StoryData existingStoryData, EditableStory editableStory)
        {
            if (existingStoryData == null || editableStory == null)
                return;

            // Clear existing data and update with new data
            var storyDataType = typeof(StoryData);
            var storyIdField = storyDataType.GetField("storyId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var titleField = storyDataType.GetField("title", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var startNodeIdField = storyDataType.GetField("startNodeId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var nodesField = storyDataType.GetField("nodes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            storyIdField?.SetValue(existingStoryData, editableStory.storyId);
            titleField?.SetValue(existingStoryData, editableStory.title);
            startNodeIdField?.SetValue(existingStoryData, editableStory.startNodeId);

            var storyNodes = new List<StoryNode>();
            foreach (var editableNode in editableStory.nodes)
            {
                var storyNode = new StoryNode(editableNode.id, editableNode.text);
                
                // Set background using reflection
                var nodeType = typeof(StoryNode);
                var backgroundField = nodeType.GetField("background", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                backgroundField?.SetValue(storyNode, editableNode.background);

                foreach (var editableChoice in editableNode.choices)
                {
                    var choice = new Choice(editableChoice.text, editableChoice.targetNodeId);
                    storyNode.AddChoice(choice);
                }

                storyNodes.Add(storyNode);
            }

            nodesField?.SetValue(existingStoryData, storyNodes);
        }
    }

    /// <summary>
    /// Utility class for generating unique node IDs and other builder operations
    /// </summary>
    public static class StoryBuilderUtility
    {
        /// <summary>
        /// Generate a unique node ID within the given story
        /// </summary>
        public static string GenerateUniqueNodeId(EditableStory story)
        {
            string newId;
            int attempts = 0;
            const int maxAttempts = 100;

            do
            {
                var guid = System.Guid.NewGuid().ToString("N");
                newId = "N-" + guid.Substring(0, 6).ToUpper();
                attempts++;
                
                if (attempts > maxAttempts)
                {
                    // Fallback to timestamp-based ID
                    newId = "N-" + DateTime.Now.Ticks.ToString("X").Substring(0, 6);
                    break;
                }
            }
            while (story.HasNodeId(newId));

            return newId;
        }

        /// <summary>
        /// Create a new EditableNode with auto-generated ID
        /// </summary>
        public static EditableNode CreateNewNode(EditableStory story, string text = "")
        {
            var nodeId = GenerateUniqueNodeId(story);
            return new EditableNode(nodeId, text);
        }

        /// <summary>
        /// Regenerate a node's ID and update all references
        /// </summary>
        public static string RegenerateNodeId(EditableStory story, EditableNode node)
        {
            if (node == null)
                return "";

            var oldId = node.id;
            var newId = GenerateUniqueNodeId(story);
            
            // Update the node's ID
            node.id = newId;
            
            // Update all references in the story
            story.RemapNodeId(oldId, newId);
            
            return newId;
        }

        /// <summary>
        /// Get a list of potential target node IDs (includes <None> option)
        /// </summary>
        public static List<string> GetTargetOptions(EditableStory story)
        {
            var options = new List<string> { "<None>" };
            options.AddRange(story.GetAllNodeIds());
            return options;
        }

        /// <summary>
        /// Convert choice target for display (empty string becomes <None>)
        /// </summary>
        public static string GetDisplayTarget(string targetNodeId)
        {
            return string.IsNullOrEmpty(targetNodeId) ? "<None>" : targetNodeId;
        }

        /// <summary>
        /// Convert display target back to actual target (handles <None>)
        /// </summary>
        public static string GetActualTarget(string displayTarget)
        {
            return displayTarget == "<None>" ? "" : displayTarget;
        }
    }
}
#endif
