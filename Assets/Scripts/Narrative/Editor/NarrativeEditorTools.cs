using UnityEngine;
using UnityEditor;
using System.IO;

namespace NarrativeNexus.Narrative.Editor
{
    /// <summary>
    /// Editor menu tools for creating sample narrative content
    /// </summary>
    public class NarrativeEditorTools
    {
        private const string STORY_DATA_PATH = "Assets/StoryData/";
        
        [MenuItem("Narrative Nexus/Create Sample Story")]
        public static void CreateSampleStory()
        {
            // Ensure directory exists
            if (!Directory.Exists(STORY_DATA_PATH))
            {
                Directory.CreateDirectory(STORY_DATA_PATH);
            }

            // Create the story data asset
            StoryData story = ScriptableObject.CreateInstance<StoryData>();
            
            // Set up story metadata
            var storyIdField = typeof(StoryData).GetField("storyId", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var titleField = typeof(StoryData).GetField("title", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var startNodeIdField = typeof(StoryData).GetField("startNodeId", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var nodesField = typeof(StoryData).GetField("nodes", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            storyIdField?.SetValue(story, "sample_forest_adventure");
            titleField?.SetValue(story, "Sample: Forest Adventure");
            startNodeIdField?.SetValue(story, "start");

            // Create nodes
            var nodesList = new System.Collections.Generic.List<StoryNode>();

            // Start node
            var startNode = new StoryNode("start", 
                "You find yourself at the edge of a mysterious forest. The ancient trees tower above you, their branches creating a canopy so thick that little sunlight reaches the forest floor. Two paths diverge before you.");
            startNode.AddChoice(new Choice("Take the left path through the dark woods", "dark_path"));
            startNode.AddChoice(new Choice("Take the right path toward the distant light", "light_path"));
            nodesList.Add(startNode);

            // Dark path node
            var darkNode = new StoryNode("dark_path", 
                "You venture into the darker part of the forest. The air grows cold and you hear strange whispers in the wind. Suddenly, you come across an old, gnarled tree with a hollow in its trunk. Something glimmers inside.");
            darkNode.AddChoice(new Choice("Investigate the hollow in the tree", "investigate_hollow"));
            darkNode.AddChoice(new Choice("Continue deeper into the dark forest", "deeper_dark"));
            darkNode.AddChoice(new Choice("Turn back and try the other path", "light_path"));
            nodesList.Add(darkNode);

            // Light path node
            var lightNode = new StoryNode("light_path", 
                "You follow the brighter path and soon emerge into a beautiful clearing filled with wildflowers. A crystal-clear stream runs through the center, and you can see fish swimming in its depths. You feel peaceful here.");
            lightNode.AddChoice(new Choice("Rest by the stream and enjoy the tranquility", "peaceful_end"));
            lightNode.AddChoice(new Choice("Follow the stream to see where it leads", "stream_adventure"));
            nodesList.Add(lightNode);

            // End nodes
            var investigateNode = new StoryNode("investigate_hollow", 
                "Inside the hollow, you discover an ancient amulet that glows with a soft blue light. As you touch it, you feel a surge of wisdom and understanding. The forest no longer seems threatening, and you realize you've found something truly special. You have gained the Forest Guardian's blessing!");
            nodesList.Add(investigateNode);

            var deeperDarkNode = new StoryNode("deeper_dark", 
                "The deeper you go, the more lost you become. Eventually, you realize you're walking in circles. As night falls, you see lights in the distance and follow them back to the forest edge. You've learned that sometimes the safest choice is to turn back.");
            nodesList.Add(deeperDarkNode);

            var peacefulEndNode = new StoryNode("peaceful_end", 
                "You spend the afternoon by the stream, feeling more relaxed than you have in years. As the sun begins to set, you make your way home with a renewed sense of peace and clarity. Sometimes the best adventures are the quiet ones.");
            nodesList.Add(peacefulEndNode);

            var streamNode = new StoryNode("stream_adventure", 
                "Following the stream leads you to a hidden waterfall. Behind the cascading water, you discover a small cave filled with beautiful crystals. You take one as a memento of your adventure and head home, knowing you'll return to this magical place again.");
            nodesList.Add(streamNode);

            nodesField?.SetValue(story, nodesList);

            // Save the asset
            string assetPath = STORY_DATA_PATH + "Sample_ForestAdventure.asset";
            AssetDatabase.CreateAsset(story, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Select the created asset
            Selection.activeObject = story;
            EditorGUIUtility.PingObject(story);

            Debug.Log($"Created sample story: {assetPath}");
        }

        [MenuItem("Narrative Nexus/Create Prefab Setup README")]
        public static void CreatePrefabSetupReadme()
        {
            string readmePath = "Assets/Prefabs/UI/README_PrefabSetup.txt";
            
            // Ensure directory exists
            string directory = Path.GetDirectoryName(readmePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string readmeContent = @"# UI Prefab Setup Instructions

## ChoiceButton_Prefab Setup

1. Create a Button in the Hierarchy (UI > Button - TextMeshPro)
2. Rename it to 'ChoiceButton_Prefab'
3. Ensure the Button has the following structure:
   - ChoiceButton_Prefab (Button component)
     - Text (TMP) (TextMeshProUGUI component)

4. Configure the Button:
   - Set appropriate colors for Normal, Highlighted, Pressed states
   - Adjust the Image component's Source Image if desired

5. Configure the TextMeshProUGUI:
   - Set appropriate font size (recommended: 18-24)
   - Set text alignment to Center/Middle
   - Set text color to a readable color (e.g., black or white)
   - Enable 'Auto Size' if you want text to scale

6. Drag the configured Button from Hierarchy to Assets/Prefabs/UI/ to create the prefab

## StoryButton_Prefab Setup (for MainMenu)

1. Create a Button in the Hierarchy (UI > Button - TextMeshPro)
2. Rename it to 'StoryButton_Prefab'
3. Follow similar structure as ChoiceButton_Prefab
4. Consider making this button larger/more prominent for story selection
5. Save as prefab in Assets/Prefabs/UI/

## Canvas Setup Tips

### MainMenu Scene:
- Canvas (Screen Space - Overlay)
  - Panel_Background (Image - full screen)
    - ScrollRect_Stories
      - Viewport
        - Content (Vertical Layout Group + Content Size Fitter)

### StoryGameplay Scene:
- Canvas (Screen Space - Overlay)
  - Panel_Story (Image - background panel)
    - StoryText (TextMeshProUGUI - story content)
    - ChoicesContainer (Vertical Layout Group - for choice buttons)
  - BackgroundImage (Image - optional, for story backgrounds)

## Component Settings:

### Vertical Layout Group (for choice containers):
- Spacing: 10-15
- Child Alignment: Upper Center
- Control Child Size: Width = true, Height = false
- Use Child Scale: false
- Child Force Expand: Width = true, Height = false

### Content Size Fitter (for scroll content):
- Horizontal Fit: Unconstrained
- Vertical Fit: Preferred Size

### ScrollRect:
- Content: Assign the Content GameObject
- Viewport: Assign the Viewport GameObject
- Horizontal: false
- Vertical: true
";

            File.WriteAllText(readmePath, readmeContent);
            AssetDatabase.Refresh();

            // Select and ping the README file
            var readme = AssetDatabase.LoadAssetAtPath<TextAsset>(readmePath);
            Selection.activeObject = readme;
            EditorGUIUtility.PingObject(readme);

            Debug.Log($"Created prefab setup README: {readmePath}");
        }

        [MenuItem("Narrative Nexus/Validate All Stories")]
        public static void ValidateAllStories()
        {
            string[] guids = AssetDatabase.FindAssets("t:StoryData");
            int validStories = 0;
            int totalIssues = 0;

            Debug.Log("=== Validating All Story Data Assets ===");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                StoryData story = AssetDatabase.LoadAssetAtPath<StoryData>(path);
                
                if (story != null)
                {
                    var issues = story.ValidateStory();
                    if (issues.Count == 0)
                    {
                        validStories++;
                        Debug.Log($"✓ {story.Title} - No issues found");
                    }
                    else
                    {
                        totalIssues += issues.Count;
                        Debug.LogWarning($"⚠ {story.Title} - {issues.Count} issues:\n  " + 
                                       string.Join("\n  ", issues));
                    }
                }
            }

            Debug.Log($"=== Validation Complete ===\nValid Stories: {validStories}/{guids.Length}\nTotal Issues: {totalIssues}");
        }
    }
}
