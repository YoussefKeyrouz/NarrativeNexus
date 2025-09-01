using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using NarrativeNexus.Narrative;
using NarrativeNexus.Managers;

namespace NarrativeNexus.UI
{
    /// <summary>
    /// UI controller for the story selection screen (MainMenu scene)
    /// </summary>
    public class StorySelectionUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform content;
        [SerializeField] private Button storyButtonPrefab;
        
        [Header("Story Data")]
        [SerializeField] private StoryData[] availableStories;

        private List<Button> instantiatedButtons = new List<Button>();

        #region Unity Lifecycle
        private void Start()
        {
            PopulateStoryList();
        }

        private void OnDestroy()
        {
            // Clean up button listeners
            foreach (var button in instantiatedButtons)
            {
                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                }
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Create buttons for each available story
        /// </summary>
        private void PopulateStoryList()
        {
            if (content == null)
            {
                Debug.LogError("Content transform not assigned in StorySelectionUI");
                return;
            }

            if (storyButtonPrefab == null)
            {
                Debug.LogError("Story button prefab not assigned in StorySelectionUI");
                return;
            }

            // Clear existing buttons
            ClearExistingButtons();

            // Create button for each story
            for (int i = 0; i < availableStories.Length; i++)
            {
                var storyData = availableStories[i];
                if (storyData == null) continue;

                CreateStoryButton(storyData, i);
            }

            Debug.Log($"Created {instantiatedButtons.Count} story buttons");
        }

        /// <summary>
        /// Create a button for a specific story
        /// </summary>
        private void CreateStoryButton(StoryData storyData, int index)
        {
            // Instantiate button
            Button button = Instantiate(storyButtonPrefab, content);
            instantiatedButtons.Add(button);

            // Set button text
            var textComponent = button.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = storyData.Title;
            }
            else
            {
                Debug.LogWarning($"No TextMeshProUGUI found in story button prefab for story: {storyData.Title}");
            }

            // Set button name for debugging
            button.name = $"StoryButton_{storyData.StoryId}";

            // Add click listener
            button.onClick.AddListener(() => OnStorySelected(storyData));

            // Validate story data and show warning if issues found
            var issues = storyData.ValidateStory();
            if (issues.Count > 0)
            {
                Debug.LogWarning($"Story '{storyData.Title}' has validation issues: {string.Join(", ", issues)}");
                
                // Optionally disable button if story has critical issues
                if (issues.Exists(issue => issue.Contains("Start node") || issue.Contains("empty")))
                {
                    button.interactable = false;
                    if (textComponent != null)
                    {
                        textComponent.color = Color.gray;
                        textComponent.text += " (Invalid)";
                    }
                }
            }
        }

        /// <summary>
        /// Clear all existing story buttons
        /// </summary>
        private void ClearExistingButtons()
        {
            foreach (var button in instantiatedButtons)
            {
                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                    DestroyImmediate(button.gameObject);
                }
            }
            instantiatedButtons.Clear();
        }

        /// <summary>
        /// Handle story selection
        /// </summary>
        private void OnStorySelected(StoryData selectedStory)
        {
            Debug.Log($"Selected story: {selectedStory.Title}");

            // Set pending story for next scene
            GameManager.PendingStory = selectedStory;

            // Load gameplay scene
            SceneManager.LoadScene("StoryGameplay");
        }
        #endregion

        #region Public Methods (for testing/debugging)
        /// <summary>
        /// Refresh the story list (useful for testing)
        /// </summary>
        public void RefreshStoryList()
        {
            PopulateStoryList();
        }

        /// <summary>
        /// Add a story to the available stories list
        /// </summary>
        public void AddStory(StoryData story)
        {
            if (story == null) return;

            var storyList = new List<StoryData>(availableStories);
            if (!storyList.Contains(story))
            {
                storyList.Add(story);
                availableStories = storyList.ToArray();
                RefreshStoryList();
            }
        }
        #endregion

        #region Editor Validation
#if UNITY_EDITOR
        private void OnValidate()
        {
            // Validate that all story references are not null
            if (availableStories != null)
            {
                for (int i = 0; i < availableStories.Length; i++)
                {
                    if (availableStories[i] == null)
                    {
                        Debug.LogWarning($"Story at index {i} is null in StorySelectionUI");
                    }
                }
            }
        }
#endif
        #endregion
    }
}
