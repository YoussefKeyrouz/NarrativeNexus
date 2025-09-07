#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using UnityEditor;
using NarrativeNexus.Narrative;
using NarrativeNexus.Narrative.Serialization;

namespace NarrativeNexus.Editor.StoryBuilder
{
    /// <summary>
    /// Editor-only bridge for handling JSON import/export operations with file dialogs
    /// </summary>
    public static class StoryJsonEditorBridge
    {
        private const string DEFAULT_EXPORT_FOLDER = "Assets/StreamingAssets/Stories";
        private const string DEFAULT_IMPORT_FOLDER = "Assets/StreamingAssets/Stories";
        private const string JSON_EXTENSION = "json";

        /// <summary>
        /// Show save dialog and export StoryData to JSON
        /// </summary>
        /// <param name="storyData">The StoryData to export</param>
        /// <returns>True if export was successful, false if cancelled or failed</returns>
        public static bool TryExport(StoryData storyData)
        {
            if (storyData == null)
            {
                EditorUtility.DisplayDialog("Export Error", "Cannot export null StoryData", "OK");
                return false;
            }

            // Ensure export directory exists
            if (!Directory.Exists(DEFAULT_EXPORT_FOLDER))
            {
                Directory.CreateDirectory(DEFAULT_EXPORT_FOLDER);
                AssetDatabase.Refresh();
            }

            // Generate default filename
            var defaultFilename = !string.IsNullOrEmpty(storyData.StoryId) 
                ? storyData.StoryId + ".json" 
                : "story.json";
            var defaultPath = Path.Combine(DEFAULT_EXPORT_FOLDER, defaultFilename);

            // Show save panel
            var savePath = EditorUtility.SaveFilePanel(
                "Export Story to JSON",
                DEFAULT_EXPORT_FOLDER,
                defaultFilename,
                JSON_EXTENSION
            );

            if (string.IsNullOrEmpty(savePath))
            {
                return false; // User cancelled
            }

            try
            {
                StoryJsonIO.ExportToFile(storyData, savePath);
                
                // Show success message
                var relativePath = GetRelativePath(savePath);
                if (EditorUtility.DisplayDialog(
                    "Export Successful", 
                    $"Story exported to:\n{relativePath}\n\nWould you like to reveal the file in the Project window?", 
                    "Reveal", 
                    "Close"))
                {
                    // Ping the file in the project window if it's within Assets
                    if (savePath.StartsWith(Application.dataPath))
                    {
                        var assetPath = "Assets" + savePath.Substring(Application.dataPath.Length);
                        var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
                        if (asset != null)
                        {
                            EditorGUIUtility.PingObject(asset);
                        }
                    }
                }

                return true;
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("Export Error", $"Failed to export story:\n{e.Message}", "OK");
                return false;
            }
        }

        /// <summary>
        /// Show open dialog and import StoryData from JSON
        /// </summary>
        /// <param name="storyData">The imported StoryData (if successful)</param>
        /// <returns>True if import was successful, false if cancelled or failed</returns>
        public static bool TryImport(out StoryData storyData)
        {
            storyData = null;

            // Show open panel
            var openPath = EditorUtility.OpenFilePanel(
                "Import Story from JSON",
                DEFAULT_IMPORT_FOLDER,
                JSON_EXTENSION
            );

            if (string.IsNullOrEmpty(openPath))
            {
                return false; // User cancelled
            }

            try
            {
                storyData = StoryJsonIO.ImportFromFile(openPath);

                if (storyData == null)
                {
                    EditorUtility.DisplayDialog("Import Error", "Failed to import story from JSON file", "OK");
                    return false;
                }

                // Show success message
                var relativePath = GetRelativePath(openPath);
                var assetPath = AssetDatabase.GetAssetPath(storyData);
                
                EditorUtility.DisplayDialog(
                    "Import Successful", 
                    $"Story imported from:\n{relativePath}\n\nStoryData asset: {assetPath}", 
                    "OK"
                );

                // Ping the created/updated asset
                EditorGUIUtility.PingObject(storyData);
                return true;
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("Import Error", $"Failed to import story:\n{e.Message}", "OK");
                return false;
            }
        }

        /// <summary>
        /// Show save dialog and export EditableStory to JSON (for direct editor use)
        /// </summary>
        /// <param name="editableStory">The EditableStory to export</param>
        /// <returns>True if export was successful, false if cancelled or failed</returns>
        public static bool TryExportEditable(StoryBuilder.EditableStory editableStory)
        {
            if (editableStory == null)
            {
                EditorUtility.DisplayDialog("Export Error", "Cannot export null story", "OK");
                return false;
            }

            // Convert to StoryData first
            var tempStoryData = StoryBuilderConverter.ToStoryData(editableStory, "");
            if (tempStoryData == null)
            {
                EditorUtility.DisplayDialog("Export Error", "Failed to convert story for export", "OK");
                return false;
            }

            // Use the regular export method
            var result = TryExport(tempStoryData);
            
            // Clean up temporary object
            Object.DestroyImmediate(tempStoryData);
            
            return result;
        }

        /// <summary>
        /// Show open dialog and import story as EditableStory (for direct editor use)
        /// </summary>
        /// <param name="editableStory">The imported EditableStory (if successful)</param>
        /// <returns>True if import was successful, false if cancelled or failed</returns>
        public static bool TryImportEditable(out StoryBuilder.EditableStory editableStory)
        {
            editableStory = null;

            // Show open panel
            var openPath = EditorUtility.OpenFilePanel(
                "Import Story from JSON",
                DEFAULT_IMPORT_FOLDER,
                JSON_EXTENSION
            );

            if (string.IsNullOrEmpty(openPath))
            {
                return false; // User cancelled
            }

            try
            {
                // Import as StoryData first
                var storyData = StoryJsonIO.ImportFromFile(openPath);
                if (storyData == null)
                {
                    EditorUtility.DisplayDialog("Import Error", "Failed to import story from JSON file", "OK");
                    return false;
                }

                // Convert to EditableStory
                editableStory = StoryBuilderConverter.FromStoryData(storyData);

                // Show success message
                var relativePath = GetRelativePath(openPath);
                EditorUtility.DisplayDialog(
                    "Import Successful", 
                    $"Story imported from:\n{relativePath}", 
                    "OK"
                );

                return true;
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("Import Error", $"Failed to import story:\n{e.Message}", "OK");
                return false;
            }
        }

        /// <summary>
        /// Create default directories if they don't exist
        /// </summary>
        public static void EnsureDirectoriesExist()
        {
            if (!Directory.Exists(DEFAULT_EXPORT_FOLDER))
            {
                Directory.CreateDirectory(DEFAULT_EXPORT_FOLDER);
            }

            var storyDataFolder = "Assets/StoryData";
            if (!Directory.Exists(storyDataFolder))
            {
                Directory.CreateDirectory(storyDataFolder);
            }

            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Get a relative path for display purposes
        /// </summary>
        private static string GetRelativePath(string fullPath)
        {
            if (fullPath.StartsWith(Application.dataPath))
            {
                return "Assets" + fullPath.Substring(Application.dataPath.Length);
            }
            return fullPath;
        }

        /// <summary>
        /// Get the default export path for a story
        /// </summary>
        public static string GetDefaultExportPath(string storyId)
        {
            EnsureDirectoriesExist();
            var filename = !string.IsNullOrEmpty(storyId) ? storyId + ".json" : "story.json";
            return Path.Combine(DEFAULT_EXPORT_FOLDER, filename);
        }

        /// <summary>
        /// Check if a JSON file exists for the given story ID
        /// </summary>
        public static bool JsonFileExists(string storyId)
        {
            if (string.IsNullOrEmpty(storyId))
                return false;

            var path = GetDefaultExportPath(storyId);
            return File.Exists(path);
        }
    }
}
#endif
