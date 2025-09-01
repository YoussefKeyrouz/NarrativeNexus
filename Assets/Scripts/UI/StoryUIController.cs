using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NarrativeNexus.Narrative;
using NarrativeNexus.Managers;

namespace NarrativeNexus.UI
{
    /// <summary>
    /// UI controller for the story gameplay screen
    /// </summary>
    public class StoryUIController : MonoBehaviour
    {
        [Header("Narrative System")]
        [SerializeField] private NarrativeManager narrative;

        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI storyText;
        [SerializeField] private Transform choicesContainer;
        [SerializeField] private Button choiceButtonPrefab;
        [SerializeField] private Image backgroundImage;

        [Header("UI Settings")]
        [SerializeField] private float typewriterSpeed = 50f;
        [SerializeField] private bool useTypewriterEffect = true;

        private List<Button> activeChoiceButtons = new List<Button>();
        private Coroutine typewriterCoroutine;

        #region Unity Lifecycle
        private void Start()
        {
            InitializeStory();
        }

        private void OnEnable()
        {
            if (narrative != null)
            {
                narrative.OnNodeChanged += OnNodeChanged;
            }
        }

        private void OnDisable()
        {
            if (narrative != null)
            {
                narrative.OnNodeChanged -= OnNodeChanged;
            }
        }

        private void OnDestroy()
        {
            // Clean up choice button listeners
            ClearChoiceButtons();
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize the story from GameManager's pending story
        /// </summary>
        private void InitializeStory()
        {
            if (narrative == null)
            {
                Debug.LogError("NarrativeManager not assigned in StoryUIController");
                return;
            }

            // Load pending story from GameManager
            if (GameManager.HasPendingStory())
            {
                var pendingStory = GameManager.PendingStory;
                Debug.Log($"Loading pending story: {pendingStory.Title}");
                
                narrative.LoadStory(pendingStory);
                narrative.StartStory();
                
                // Clear the pending story
                GameManager.ClearPendingStory();
            }
            else
            {
                Debug.LogWarning("No pending story found. Make sure to navigate here from the story selection screen.");
                
                // Show error message in UI
                if (storyText != null)
                {
                    storyText.text = "No story loaded. Please return to the main menu and select a story.";
                }
            }
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Handle node changes from the narrative manager
        /// </summary>
        private void OnNodeChanged(StoryNode node, List<Choice> availableChoices)
        {
            if (node == null) return;

            Debug.Log($"Rendering node: {node.Id}");

            // Update background if specified
            UpdateBackground(node.Background);

            // Update story text
            UpdateStoryText(node.Text);

            // Update choices
            UpdateChoices(availableChoices);
        }
        #endregion

        #region UI Updates
        /// <summary>
        /// Update the story text display
        /// </summary>
        private void UpdateStoryText(string text)
        {
            if (storyText == null)
            {
                Debug.LogError("Story text component not assigned");
                return;
            }

            // Stop any existing typewriter effect
            if (typewriterCoroutine != null)
            {
                StopCoroutine(typewriterCoroutine);
            }

            if (useTypewriterEffect && typewriterSpeed > 0)
            {
                typewriterCoroutine = StartCoroutine(TypewriterEffect(text));
            }
            else
            {
                storyText.text = text;
            }
        }

        /// <summary>
        /// Update the background image
        /// </summary>
        private void UpdateBackground(Sprite backgroundSprite)
        {
            if (backgroundImage != null && backgroundSprite != null)
            {
                backgroundImage.sprite = backgroundSprite;
                backgroundImage.color = Color.white;
            }
        }

        /// <summary>
        /// Update the choice buttons
        /// </summary>
        private void UpdateChoices(List<Choice> availableChoices)
        {
            if (choicesContainer == null)
            {
                Debug.LogError("Choices container not assigned");
                return;
            }

            if (choiceButtonPrefab == null)
            {
                Debug.LogError("Choice button prefab not assigned");
                return;
            }

            // Clear existing choice buttons
            ClearChoiceButtons();

            // Create buttons for available choices
            for (int i = 0; i < availableChoices.Count; i++)
            {
                CreateChoiceButton(availableChoices[i], i);
            }

            // If no choices available, show end message
            if (availableChoices.Count == 0)
            {
                CreateEndMessage();
            }
        }

        /// <summary>
        /// Create a button for a specific choice
        /// </summary>
        private void CreateChoiceButton(Choice choice, int index)
        {
            Button button = Instantiate(choiceButtonPrefab, choicesContainer);
            activeChoiceButtons.Add(button);

            // Set button text
            var textComponent = button.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = choice.Text;
            }

            // Set button name for debugging
            button.name = $"ChoiceButton_{index}";

            // Add click listener
            button.onClick.AddListener(() => OnChoiceSelected(index));
        }

        /// <summary>
        /// Create an end message when no choices are available
        /// </summary>
        private void CreateEndMessage()
        {
            Button endButton = Instantiate(choiceButtonPrefab, choicesContainer);
            activeChoiceButtons.Add(endButton);

            var textComponent = endButton.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = "Return to Main Menu";
            }

            endButton.name = "EndButton";
            endButton.onClick.AddListener(ReturnToMainMenu);
        }

        /// <summary>
        /// Clear all active choice buttons
        /// </summary>
        private void ClearChoiceButtons()
        {
            foreach (var button in activeChoiceButtons)
            {
                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                    DestroyImmediate(button.gameObject);
                }
            }
            activeChoiceButtons.Clear();
        }
        #endregion

        #region Choice Handling
        /// <summary>
        /// Handle choice selection
        /// </summary>
        private void OnChoiceSelected(int choiceIndex)
        {
            Debug.Log($"Choice selected: {choiceIndex}");
            
            if (narrative != null)
            {
                narrative.SelectChoice(choiceIndex);
            }
        }

        /// <summary>
        /// Return to the main menu
        /// </summary>
        private void ReturnToMainMenu()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
        #endregion

        #region Typewriter Effect
        /// <summary>
        /// Typewriter effect coroutine
        /// </summary>
        private System.Collections.IEnumerator TypewriterEffect(string text)
        {
            storyText.text = "";
            
            foreach (char c in text)
            {
                storyText.text += c;
                yield return new WaitForSeconds(1f / typewriterSpeed);
            }
            
            typewriterCoroutine = null;
        }
        #endregion

        #region Editor Validation
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (typewriterSpeed < 0)
            {
                typewriterSpeed = 0;
            }
        }
#endif
        #endregion
    }
}
