using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace NarrativeNexus.Narrative.Serialization
{
    /// <summary>
    /// JSON-serializable representation of a complete story
    /// </summary>
    [System.Serializable]
    public class StoryJson
    {
        public string storyId;
        public string title;
        public string startNodeId;
        public List<NodeJson> nodes = new List<NodeJson>();
    }

    /// <summary>
    /// JSON-serializable representation of a story node
    /// </summary>
    [System.Serializable]
    public class NodeJson
    {
        public string id;
        public string text;
        public string backgroundPath;
        public List<ChoiceJson> choices = new List<ChoiceJson>();
    }

    /// <summary>
    /// JSON-serializable representation of a choice
    /// </summary>
    [System.Serializable]
    public class ChoiceJson
    {
        public string text;
        public string targetNodeId;
    }

    /// <summary>
    /// Converts between StoryData ScriptableObjects and JSON representations
    /// </summary>
    public static class StoryJsonMapper
    {
        /// <summary>
        /// Convert StoryData to JSON representation
        /// </summary>
        public static StoryJson ToJson(StoryData storyData)
        {
            if (storyData == null)
                return null;

            var storyJson = new StoryJson
            {
                storyId = storyData.StoryId,
                title = storyData.Title,
                startNodeId = storyData.StartNodeId
            };

            foreach (var node in storyData.Nodes)
            {
                var nodeJson = new NodeJson
                {
                    id = node.Id,
                    text = node.Text,
                    backgroundPath = GetSpritePath(node.Background)
                };

                foreach (var choice in node.Choices)
                {
                    nodeJson.choices.Add(new ChoiceJson
                    {
                        text = choice.Text,
                        targetNodeId = choice.TargetNodeId
                    });
                }

                storyJson.nodes.Add(nodeJson);
            }

            return storyJson;
        }

        /// <summary>
        /// Convert JSON representation to StoryData ScriptableObject
        /// </summary>
        public static StoryData FromJson(StoryJson json, StoryData targetSo = null)
        {
            if (json == null)
                return null;

            StoryData storyData = targetSo;
            if (storyData == null)
            {
                storyData = ScriptableObject.CreateInstance<StoryData>();
            }

            // Set story metadata using reflection since fields are private
            var storyDataType = typeof(StoryData);
            var storyIdField = storyDataType.GetField("storyId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var titleField = storyDataType.GetField("title", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var startNodeIdField = storyDataType.GetField("startNodeId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var nodesField = storyDataType.GetField("nodes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            storyIdField?.SetValue(storyData, json.storyId);
            titleField?.SetValue(storyData, json.title);
            startNodeIdField?.SetValue(storyData, json.startNodeId);

            // Clear existing nodes
            var nodesList = (List<StoryNode>)nodesField?.GetValue(storyData) ?? new List<StoryNode>();
            nodesList.Clear();

            // Convert JSON nodes to StoryNodes
            foreach (var nodeJson in json.nodes)
            {
                var storyNode = new StoryNode(nodeJson.id, nodeJson.text);
                
                // Set background sprite if path is provided
                if (!string.IsNullOrEmpty(nodeJson.backgroundPath))
                {
                    var sprite = LoadSpriteFromPath(nodeJson.backgroundPath);
                    if (sprite != null)
                    {
                        SetNodeBackground(storyNode, sprite);
                    }
                }

                // Add choices
                foreach (var choiceJson in nodeJson.choices)
                {
                    var choice = new Choice(choiceJson.text, choiceJson.targetNodeId);
                    storyNode.AddChoice(choice);
                }

                nodesList.Add(storyNode);
            }

            nodesField?.SetValue(storyData, nodesList);
            return storyData;
        }

        /// <summary>
        /// Get the asset path for a sprite (returns empty if null)
        /// </summary>
        private static string GetSpritePath(Sprite sprite)
        {
            if (sprite == null)
                return string.Empty;

#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.GetAssetPath(sprite);
#else
            return string.Empty;
#endif
        }

        /// <summary>
        /// Load a sprite from an asset path
        /// </summary>
        private static Sprite LoadSpriteFromPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
#else
            return Resources.Load<Sprite>(path);
#endif
        }

        /// <summary>
        /// Set the background sprite on a story node using reflection
        /// </summary>
        private static void SetNodeBackground(StoryNode node, Sprite sprite)
        {
            var nodeType = typeof(StoryNode);
            var backgroundField = nodeType.GetField("background", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            backgroundField?.SetValue(node, sprite);
        }
    }

    /// <summary>
    /// Handles file I/O operations for story JSON data
    /// </summary>
    public static class StoryJsonIO
    {
        /// <summary>
        /// Export a StoryData to a JSON file
        /// </summary>
        public static void ExportToFile(StoryData storyData, string filePath)
        {
            if (storyData == null)
            {
                Debug.LogError("Cannot export null StoryData");
                return;
            }

            try
            {
                var storyJson = StoryJsonMapper.ToJson(storyData);
                var jsonString = JsonUtility.ToJson(storyJson, true);

                // Ensure directory exists
                var directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(filePath, jsonString, System.Text.Encoding.UTF8);
                Debug.Log($"Story exported to: {filePath}");

#if UNITY_EDITOR
                UnityEditor.AssetDatabase.Refresh();
#endif
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to export story to {filePath}: {e.Message}");
            }
        }

        /// <summary>
        /// Import a StoryData from a JSON file
        /// </summary>
        public static StoryData ImportFromFile(string filePath, string targetAssetPath = null)
        {
            if (!File.Exists(filePath))
            {
                Debug.LogError($"File not found: {filePath}");
                return null;
            }

            try
            {
                var jsonString = File.ReadAllText(filePath, System.Text.Encoding.UTF8);
                var storyJson = JsonUtility.FromJson<StoryJson>(jsonString);

                if (storyJson == null)
                {
                    Debug.LogError($"Failed to parse JSON from {filePath}");
                    return null;
                }

                StoryData targetSo = null;

#if UNITY_EDITOR
                // Try to find existing asset with same storyId
                if (string.IsNullOrEmpty(targetAssetPath))
                {
                    targetAssetPath = FindExistingStoryAsset(storyJson.storyId);
                }

                if (!string.IsNullOrEmpty(targetAssetPath))
                {
                    targetSo = UnityEditor.AssetDatabase.LoadAssetAtPath<StoryData>(targetAssetPath);
                }

                var storyData = StoryJsonMapper.FromJson(storyJson, targetSo);

                if (targetSo == null)
                {
                    // Create new asset
                    if (string.IsNullOrEmpty(targetAssetPath))
                    {
                        targetAssetPath = $"Assets/StoryData/{storyJson.storyId}.asset";
                    }

                    // Ensure directory exists
                    var directory = Path.GetDirectoryName(targetAssetPath);
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    UnityEditor.AssetDatabase.CreateAsset(storyData, targetAssetPath);
                    Debug.Log($"Created new StoryData asset: {targetAssetPath}");
                }
                else
                {
                    UnityEditor.EditorUtility.SetDirty(storyData);
                    Debug.Log($"Updated existing StoryData asset: {targetAssetPath}");
                }

                UnityEditor.AssetDatabase.SaveAssets();
                UnityEditor.AssetDatabase.Refresh();
#else
                var storyData = StoryJsonMapper.FromJson(storyJson);
#endif

                return storyData;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to import story from {filePath}: {e.Message}");
                return null;
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Find existing StoryData asset with matching storyId
        /// </summary>
        private static string FindExistingStoryAsset(string storyId)
        {
            if (string.IsNullOrEmpty(storyId))
                return null;

            var guids = UnityEditor.AssetDatabase.FindAssets("t:StoryData");
            foreach (var guid in guids)
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var storyData = UnityEditor.AssetDatabase.LoadAssetAtPath<StoryData>(path);
                if (storyData != null && storyData.StoryId == storyId)
                {
                    return path;
                }
            }

            return null;
        }
#endif
    }
}
