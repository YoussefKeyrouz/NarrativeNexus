using System.Collections.Generic;
using UnityEngine;

namespace NarrativeNexus.Narrative
{
    /// <summary>
    /// Represents a single node in a branching narrative story
    /// </summary>
    [System.Serializable]
    public class StoryNode
    {
        [SerializeField] private string id;
        [SerializeField] [TextArea(3, 10)] private string text;
        [SerializeField] private List<Choice> choices = new List<Choice>();
        [SerializeField] private Sprite background;

        public string Id => id;
        public string Text => text;
        public IReadOnlyList<Choice> Choices => choices;
        public Sprite Background => background;

        /// <summary>
        /// Gets all choices that are available based on the current game state
        /// </summary>
        public List<Choice> GetAvailableChoices(IGameState gameState)
        {
            var availableChoices = new List<Choice>();
            
            foreach (var choice in choices)
            {
                if (choice.IsAvailable(gameState))
                {
                    availableChoices.Add(choice);
                }
            }
            
            return availableChoices;
        }

        /// <summary>
        /// Checks if this is an end node (no choices available)
        /// </summary>
        public bool IsEndNode => choices.Count == 0;

        /// <summary>
        /// Constructor for creating nodes in code
        /// </summary>
        public StoryNode(string nodeId, string nodeText)
        {
            id = nodeId;
            text = nodeText;
            choices = new List<Choice>();
        }

        /// <summary>
        /// Default constructor for Unity serialization
        /// </summary>
        public StoryNode()
        {
            id = "";
            text = "";
            choices = new List<Choice>();
        }

        /// <summary>
        /// Add a choice to this node
        /// </summary>
        public void AddChoice(Choice choice)
        {
            if (choice != null)
            {
                choices.Add(choice);
            }
        }

        /// <summary>
        /// Remove a choice from this node
        /// </summary>
        public void RemoveChoice(Choice choice)
        {
            choices.Remove(choice);
        }

        /// <summary>
        /// Clear all choices
        /// </summary>
        public void ClearChoices()
        {
            choices.Clear();
        }
    }
}
