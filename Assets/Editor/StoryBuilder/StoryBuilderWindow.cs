#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using NarrativeNexus.Narrative;

namespace NarrativeNexus.Editor.StoryBuilder
{
    /// <summary>
    /// Main editor window for the Story Builder tool
    /// </summary>
    public class StoryBuilderWindow : EditorWindow
    {
        #region Private Fields
        
        // Story state
        private EditableStory currentStory;
        private EditableNode selectedNode;
        private int selectedNodeIndex = -1;
        private new bool hasUnsavedChanges = false;
        private string lastSavedAssetPath = "";
        
        // UI state
        private Vector2 leftPaneScrollPos;
        private Vector2 rightPaneScrollPos;
        private string searchFilter = "";
        private List<string> validationIssues = new List<string>();
        
        // Layout
        private float leftPaneWidth = 350f;
        
        // GUI styles
        private GUIStyle headerStyle;
        private GUIStyle boxStyle;
        private GUIStyle errorStyle;
        private GUIStyle selectedNodeStyle;
        
        #endregion

        #region Unity Lifecycle
        
        [MenuItem("Window/Narrative Nexus/Story Builder")]
        public static void ShowWindow()
        {
            var window = GetWindow<StoryBuilderWindow>("Story Builder");
            window.minSize = new Vector2(800, 600);
            window.Show();
        }

        private void OnEnable()
        {
            if (currentStory == null)
            {
                currentStory = new EditableStory();
            }
            
            UpdateValidation();
        }

        private void OnDisable()
        {
            if (hasUnsavedChanges)
            {
                if (EditorUtility.DisplayDialog(
                    "Unsaved Changes", 
                    "You have unsaved changes. Do you want to save before closing?", 
                    "Save", 
                    "Don't Save"))
                {
                    SaveCurrentStory();
                }
            }
        }

        private void OnGUI()
        {
            InitializeStyles();
            DrawToolbar();
            DrawMainContent();
            HandleKeyboardShortcuts();
        }

        #endregion

        #region GUI Drawing

        private void InitializeStyles()
        {
            if (headerStyle == null)
            {
                headerStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 14
                };
            }

            if (boxStyle == null)
            {
                boxStyle = new GUIStyle(GUI.skin.box)
                {
                    padding = new RectOffset(10, 10, 10, 10)
                };
            }

            if (errorStyle == null)
            {
                errorStyle = new GUIStyle(EditorStyles.helpBox);
            }

            if (selectedNodeStyle == null)
            {
                selectedNodeStyle = new GUIStyle(GUI.skin.button)
                {
                    normal = { background = EditorGUIUtility.whiteTexture, textColor = Color.white }
                };
            }
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                if (GUILayout.Button("New Story", EditorStyles.toolbarButton))
                {
                    CreateNewStory();
                }

                if (GUILayout.Button("Load StoryData", EditorStyles.toolbarButton))
                {
                    LoadStoryData();
                }

                GUI.enabled = currentStory != null && !string.IsNullOrEmpty(currentStory.storyId);
                if (GUILayout.Button("Save as StoryData", EditorStyles.toolbarButton))
                {
                    SaveCurrentStory();
                }
                GUI.enabled = true;

                GUILayout.Space(10);

                if (GUILayout.Button("Export JSON", EditorStyles.toolbarButton))
                {
                    ExportToJson();
                }

                if (GUILayout.Button("Import JSON", EditorStyles.toolbarButton))
                {
                    ImportFromJson();
                }

                GUILayout.FlexibleSpace();

                // Unsaved changes indicator
                if (hasUnsavedChanges)
                {
                    GUILayout.Label("*", EditorStyles.toolbarButton);
                }
            }
        }

        private void DrawMainContent()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                // Left Pane - Node List (fixed width)
                using (new EditorGUILayout.VerticalScope(GUILayout.Width(leftPaneWidth)))
                {
                    DrawLeftPaneContent();
                }
                
                // Right Pane - Node Editor & Settings (flexible width)  
                using (new EditorGUILayout.VerticalScope())
                {
                    DrawRightPaneContent();
                }
            }
        }

        private void DrawLeftPaneContent()
        {
            // Header
            EditorGUILayout.LabelField("Node List", headerStyle);
            
            // Search bar
            using (new EditorGUILayout.HorizontalScope())
            {
                var newSearchFilter = EditorGUILayout.TextField("Search", searchFilter);
                if (newSearchFilter != searchFilter)
                {
                    searchFilter = newSearchFilter;
                }
                
                if (GUILayout.Button("Clear", GUILayout.Width(50)))
                {
                    searchFilter = "";
                    GUI.FocusControl(null);
                }
            }

            EditorGUILayout.Space();
            
            // Node management buttons
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Add Node"))
                {
                    AddNewNode();
                }
                
                GUI.enabled = selectedNode != null;
                if (GUILayout.Button("Delete Node"))
                {
                    DeleteSelectedNode();
                }
                GUI.enabled = true;
            }

            EditorGUILayout.Space();
            
            // Node list
            DrawNodeList();
        }

        private void DrawRightPaneContent()
        {
            EditorGUILayout.LabelField("Node Editor", headerStyle);
            
            using (var scrollView = new EditorGUILayout.ScrollViewScope(rightPaneScrollPos))
            {
                rightPaneScrollPos = scrollView.scrollPosition;
                
                if (selectedNode == null)
                {
                    DrawNoSelectionMessage();
                }
                else
                {
                    DrawNodeEditor();
                }
                
                EditorGUILayout.Space();
                DrawStorySettings();
                DrawValidationPanel();
            }
        }

        private void DrawNodeList()
        {
            if (currentStory == null || currentStory.nodes.Count == 0)
            {
                EditorGUILayout.HelpBox("No nodes yet. Click 'Add Node' to create your first node.", MessageType.Info);
                return;
            }

            using (var scrollView = new EditorGUILayout.ScrollViewScope(leftPaneScrollPos))
            {
                leftPaneScrollPos = scrollView.scrollPosition;
                
                var filteredNodes = GetFilteredNodes();
                
                for (int i = 0; i < filteredNodes.Count; i++)
                {
                    var node = filteredNodes[i];
                    var isSelected = selectedNode == node;
                    var originalIndex = currentStory.nodes.IndexOf(node);
                    
                    var style = isSelected ? selectedNodeStyle : GUI.skin.button;
                    var backgroundColor = isSelected ? Color.blue : Color.white;
                    
                    GUI.backgroundColor = backgroundColor;
                    
                    var buttonContent = new GUIContent($"{node.id}\n{node.GetTextPreview()}");
                    
                    if (GUILayout.Button(buttonContent, style, GUILayout.Height(50)))
                    {
                        SelectNode(originalIndex);
                    }
                    
                    GUI.backgroundColor = Color.white;
                }
            }
        }

        private void DrawNoSelectionMessage()
        {
            EditorGUILayout.Space(50);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                using (new EditorGUILayout.VerticalScope())
                {
                    EditorGUILayout.LabelField("Select a node to edit", EditorStyles.centeredGreyMiniLabel);
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Or create a new node from the left panel", EditorStyles.centeredGreyMiniLabel);
                }
                GUILayout.FlexibleSpace();
            }
        }

        private void DrawNodeEditor()
        {
            using (new EditorGUILayout.VerticalScope(boxStyle))
            {
                // Node ID section
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("ID", GUILayout.Width(30));
                    EditorGUILayout.SelectableLabel(selectedNode.id, EditorStyles.textField);
                    
                    if (GUILayout.Button("Regenerate", GUILayout.Width(80)))
                    {
                        RegenerateNodeId();
                    }
                }
                
                EditorGUILayout.Space();
                
                // Node text
                EditorGUILayout.LabelField("Text");
                GUI.SetNextControlName($"NodeText_{selectedNode.id}");
                var newNodeText = EditorGUILayout.TextArea(selectedNode.text, GUILayout.Height(100));
                if (newNodeText != selectedNode.text)
                {
                    selectedNode.text = newNodeText;
                    MarkDirty();
                }
                
                EditorGUILayout.Space();
                
                // Background sprite
                EditorGUILayout.LabelField("Background Sprite");
                var newBackground = (Sprite)EditorGUILayout.ObjectField(
                    selectedNode.background, typeof(Sprite), false);
                if (newBackground != selectedNode.background)
                {
                    selectedNode.background = newBackground;
                    MarkDirty();
                }
                
                EditorGUILayout.Space();
                
                // Choices section
                DrawChoicesSection();
            }
        }

        private void DrawChoicesSection()
        {
            EditorGUILayout.LabelField("Choices", EditorStyles.boldLabel);
            
            // Add choice button
            if (GUILayout.Button("Add Choice"))
            {
                selectedNode.choices.Add(new EditableChoice("New Choice", ""));
                MarkDirty();
            }
            
            EditorGUILayout.Space();
            
            // Choice list
            for (int i = selectedNode.choices.Count - 1; i >= 0; i--)
            {
                DrawChoice(i);
            }
        }

        private void DrawChoice(int index)
        {
            var choice = selectedNode.choices[index];
            
            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField($"Choice {index + 1}", EditorStyles.boldLabel);
                    
                    if (GUILayout.Button("X", GUILayout.Width(25)))
                    {
                        selectedNode.choices.RemoveAt(index);
                        MarkDirty();
                        return;
                    }
                }
                
                // Choice text
                EditorGUILayout.LabelField("Text");
                GUI.SetNextControlName($"Choice_{selectedNode.id}_{index}");
                var newChoiceText = EditorGUILayout.TextField(choice.text);
                if (newChoiceText != choice.text)
                {
                    choice.text = newChoiceText;
                    MarkDirty();
                }
                
                EditorGUILayout.Space();
                
                // Target selection
                EditorGUILayout.LabelField("Target");
                if (currentStory != null && currentStory.nodes.Count > 0)
                {
                    var targetOptions = StoryBuilderUtility.GetTargetOptions(currentStory);
                    var currentTarget = StoryBuilderUtility.GetDisplayTarget(choice.targetNodeId);
                    var selectedIndex = targetOptions.IndexOf(currentTarget);
                    
                    if (selectedIndex < 0) selectedIndex = 0;
                    
                    var newSelectedIndex = EditorGUILayout.Popup(selectedIndex, targetOptions.ToArray());
                    
                    if (newSelectedIndex != selectedIndex && newSelectedIndex >= 0 && newSelectedIndex < targetOptions.Count)
                    {
                        var newTarget = StoryBuilderUtility.GetActualTarget(targetOptions[newSelectedIndex]);
                        choice.targetNodeId = newTarget;
                        MarkDirty();
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("No target nodes available", EditorStyles.centeredGreyMiniLabel);
                }
            }
            
            EditorGUILayout.Space();
        }

        private void DrawStorySettings()
        {
            using (new EditorGUILayout.VerticalScope(boxStyle))
            {
                EditorGUILayout.LabelField("Story Settings", EditorStyles.boldLabel);
                
                // Story ID
                EditorGUILayout.LabelField("Story ID");
                GUI.SetNextControlName("StoryId");
                var newStoryId = EditorGUILayout.TextField(currentStory.storyId);
                if (newStoryId != currentStory.storyId)
                {
                    currentStory.storyId = newStoryId;
                    MarkDirty();
                }
                
                // Story Title
                EditorGUILayout.LabelField("Story Title");
                GUI.SetNextControlName("StoryTitle");
                var newTitle = EditorGUILayout.TextField(currentStory.title);
                if (newTitle != currentStory.title)
                {
                    currentStory.title = newTitle;
                    MarkDirty();
                }
                
                // Start Node
                EditorGUILayout.LabelField("Start Node");
                if (currentStory.nodes.Count > 0)
                {
                    var startNodeOptions = StoryBuilderUtility.GetTargetOptions(currentStory);
                    var currentStartNode = StoryBuilderUtility.GetDisplayTarget(currentStory.startNodeId);
                    var selectedStartIndex = startNodeOptions.IndexOf(currentStartNode);
                    
                    if (selectedStartIndex < 0) selectedStartIndex = 0;
                    
                    var newStartIndex = EditorGUILayout.Popup(selectedStartIndex, startNodeOptions.ToArray());
                    
                    if (newStartIndex != selectedStartIndex && newStartIndex >= 0 && newStartIndex < startNodeOptions.Count)
                    {
                        var newStartNode = StoryBuilderUtility.GetActualTarget(startNodeOptions[newStartIndex]);
                        currentStory.startNodeId = newStartNode;
                        MarkDirty();
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("No nodes available", EditorStyles.centeredGreyMiniLabel);
                }
            }
        }

        private void DrawValidationPanel()
        {
            if (validationIssues.Count > 0)
            {
                using (new EditorGUILayout.VerticalScope(errorStyle))
                {
                    EditorGUILayout.LabelField("Validation Issues", EditorStyles.boldLabel);
                    
                    foreach (var issue in validationIssues)
                    {
                        EditorGUILayout.LabelField($"• {issue}", EditorStyles.wordWrappedMiniLabel);
                    }
                }
            }
        }

        #endregion

        #region Story Operations

        private void CreateNewStory()
        {
            if (hasUnsavedChanges)
            {
                if (!EditorUtility.DisplayDialog(
                    "Unsaved Changes", 
                    "You have unsaved changes. Create new story anyway?", 
                    "Yes", 
                    "Cancel"))
                {
                    return;
                }
            }

            // Simple dialog to get story info
            var storyId = "MyNewStory";
            currentStory = new EditableStory(storyId, storyId);
            selectedNode = null;
            selectedNodeIndex = -1;
            hasUnsavedChanges = true;
            lastSavedAssetPath = "";
            UpdateValidation();
        }

        private void LoadStoryData()
        {
            if (hasUnsavedChanges)
            {
                if (!EditorUtility.DisplayDialog(
                    "Unsaved Changes", 
                    "You have unsaved changes. Load story anyway?", 
                    "Yes", 
                    "Cancel"))
                {
                    return;
                }
            }

            var path = EditorUtility.OpenFilePanel("Load StoryData", "Assets/StoryData", "asset");
            if (!string.IsNullOrEmpty(path))
            {
                // Convert absolute path to relative asset path
                if (path.StartsWith(Application.dataPath))
                {
                    path = "Assets" + path.Substring(Application.dataPath.Length);
                }
                
                var storyData = AssetDatabase.LoadAssetAtPath<StoryData>(path);
                if (storyData != null)
                {
                    currentStory = StoryBuilderConverter.FromStoryData(storyData);
                    selectedNode = null;
                    selectedNodeIndex = -1;
                    hasUnsavedChanges = false;
                    lastSavedAssetPath = path;
                    UpdateValidation();
                    Debug.Log($"Loaded story: {storyData.Title} ({storyData.StoryId})");
                }
                else
                {
                    EditorUtility.DisplayDialog("Load Error", "Failed to load StoryData from selected file", "OK");
                }
            }
        }

        private void SaveCurrentStory()
        {
            if (currentStory == null)
                return;

            // Validate before saving
            var issues = currentStory.ValidateStory();
            var criticalIssues = issues.Where(i => 
                i.Contains("Duplicate node ID") || 
                i.Contains("Story ID is required") ||
                i.Contains("Start node") && !i.Contains("unreachable")).ToList();

            if (criticalIssues.Count > 0)
            {
                var message = "Cannot save due to critical issues:\n\n" + string.Join("\n", criticalIssues);
                EditorUtility.DisplayDialog("Save Error", message, "OK");
                return;
            }

            StoryData storyData = null;
            string assetPath = lastSavedAssetPath;

            if (string.IsNullOrEmpty(assetPath))
            {
                assetPath = EditorUtility.SaveFilePanelInProject(
                    "Save StoryData", 
                    currentStory.storyId + ".asset", 
                    "asset", 
                    "Save StoryData asset");

                if (string.IsNullOrEmpty(assetPath))
                    return;
            }

            // Check if asset already exists
            storyData = AssetDatabase.LoadAssetAtPath<StoryData>(assetPath);

            if (storyData == null)
            {
                // Create new asset
                storyData = StoryBuilderConverter.ToStoryData(currentStory, assetPath);
                AssetDatabase.CreateAsset(storyData, assetPath);
            }
            else
            {
                // Update existing asset
                Undo.RecordObject(storyData, "Update Story");
                StoryBuilderConverter.UpdateStoryData(storyData, currentStory);
                EditorUtility.SetDirty(storyData);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            hasUnsavedChanges = false;
            lastSavedAssetPath = assetPath;

            Debug.Log($"Story saved to: {assetPath}");
        }

        private void ExportToJson()
        {
            if (currentStory == null)
                return;

            StoryJsonEditorBridge.TryExportEditable(currentStory);
        }

        private void ImportFromJson()
        {
            if (hasUnsavedChanges)
            {
                if (!EditorUtility.DisplayDialog(
                    "Unsaved Changes", 
                    "You have unsaved changes. Import story anyway?", 
                    "Yes", 
                    "Cancel"))
                {
                    return;
                }
            }

            EditableStory importedStory;
            if (StoryJsonEditorBridge.TryImportEditable(out importedStory))
            {
                currentStory = importedStory;
                selectedNode = null;
                selectedNodeIndex = -1;
                hasUnsavedChanges = true;
                lastSavedAssetPath = "";
                UpdateValidation();
            }
        }

        #endregion

        #region Node Operations

        private void AddNewNode()
        {
            if (currentStory == null)
                return;

            var newNode = StoryBuilderUtility.CreateNewNode(currentStory, "Enter your story text here...");
            currentStory.nodes.Add(newNode);
            
            SelectNode(currentStory.nodes.Count - 1);
            MarkDirty();
        }

        private void DeleteSelectedNode()
        {
            if (selectedNode == null || currentStory == null)
                return;

            if (EditorUtility.DisplayDialog(
                "Delete Node", 
                $"Are you sure you want to delete node '{selectedNode.id}'?\n\nThis will also remove any choices targeting this node.", 
                "Delete", 
                "Cancel"))
            {
                currentStory.RemoveNode(selectedNode.id);
                selectedNode = null;
                selectedNodeIndex = -1;
                MarkDirty();
            }
        }

        private void SelectNode(int index)
        {
            if (currentStory == null || index < 0 || index >= currentStory.nodes.Count)
                return;

            selectedNode = currentStory.nodes[index];
            selectedNodeIndex = index;
            
            // Clear GUI focus to prevent text field bleed when switching nodes
            GUI.FocusControl(null);
        }

        private void RegenerateNodeId()
        {
            if (selectedNode == null || currentStory == null)
                return;

            if (EditorUtility.DisplayDialog(
                "Regenerate Node ID", 
                $"This will change the node ID from '{selectedNode.id}' to a new unique ID and update all references.\n\nContinue?", 
                "Yes", 
                "Cancel"))
            {
                var newId = StoryBuilderUtility.RegenerateNodeId(currentStory, selectedNode);
                Debug.Log($"Node ID regenerated: {newId}");
                MarkDirty();
            }
        }

        #endregion

        #region Utility Methods

        private List<EditableNode> GetFilteredNodes()
        {
            if (currentStory == null)
                return new List<EditableNode>();

            if (string.IsNullOrEmpty(searchFilter))
                return currentStory.nodes;

            var filter = searchFilter.ToLower();
            return currentStory.nodes.Where(n => 
                n.id.ToLower().Contains(filter) || 
                n.text.ToLower().Contains(filter)).ToList();
        }

        private void UpdateValidation()
        {
            if (currentStory != null)
            {
                validationIssues = currentStory.ValidateStory();
            }
            else
            {
                validationIssues.Clear();
            }
        }

        private void MarkDirty()
        {
            hasUnsavedChanges = true;
            UpdateValidation();
        }

        private void HandleKeyboardShortcuts()
        {
            var e = Event.current;
            
            if (e.type == EventType.KeyDown)
            {
                if (e.control || e.command)
                {
                    switch (e.keyCode)
                    {
                        case KeyCode.S:
                            SaveCurrentStory();
                            e.Use();
                            break;
                            
                        case KeyCode.N:
                            CreateNewStory();
                            e.Use();
                            break;
                    }
                }
                
                if (e.keyCode == KeyCode.Delete && selectedNode != null)
                {
                    DeleteSelectedNode();
                    e.Use();
                }
            }
        }

        #endregion
    }
}
#endif




