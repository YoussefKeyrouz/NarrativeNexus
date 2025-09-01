using UnityEngine;
using UnityEditor;
using System.Linq;

namespace NarrativeNexus.Narrative.Editor
{
    /// <summary>
    /// Custom editor for StoryData ScriptableObjects
    /// </summary>
    [CustomEditor(typeof(StoryData))]
    public class StoryDataEditor : UnityEditor.Editor
    {
        private StoryData storyData;
        
        private void OnEnable()
        {
            storyData = (StoryData)target;
        }

        public override void OnInspectorGUI()
        {
            // Draw default inspector
            DrawDefaultInspector();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Story Validation", EditorStyles.boldLabel);

            // Validation buttons
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Validate Story"))
            {
                ValidateStory();
            }
            if (GUILayout.Button("Sort Nodes by ID"))
            {
                SortNodesByIds();
            }
            EditorGUILayout.EndHorizontal();

            // Quick stats
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Quick Stats", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Total Nodes: {storyData.Nodes.Count}");
            
            var totalChoices = storyData.Nodes.Sum(n => n.Choices.Count);
            EditorGUILayout.LabelField($"Total Choices: {totalChoices}");
            
            var endNodes = storyData.Nodes.Where(n => n.IsEndNode).Count();
            EditorGUILayout.LabelField($"End Nodes: {endNodes}");
        }

        private void ValidateStory()
        {
            var issues = storyData.ValidateStory();
            
            if (issues.Count == 0)
            {
                EditorUtility.DisplayDialog("Validation Result", 
                    $"Story '{storyData.Title}' passed validation!", "OK");
            }
            else
            {
                var message = $"Story '{storyData.Title}' has {issues.Count} issue(s):\n\n" +
                             string.Join("\n", issues.Select(issue => $"• {issue}"));
                
                EditorUtility.DisplayDialog("Validation Issues", message, "OK");
                
                // Also log to console for easy copying
                Debug.LogWarning($"Story validation issues for '{storyData.Title}':\n{string.Join("\n", issues)}");
            }
        }

        private void SortNodesByIds()
        {
            if (EditorUtility.DisplayDialog("Sort Nodes", 
                "This will reorder the nodes list by ID. Continue?", "Yes", "Cancel"))
            {
                Undo.RecordObject(storyData, "Sort Nodes by ID");
                storyData.SortNodesByIds();
                EditorUtility.SetDirty(storyData);
            }
        }
    }
}
