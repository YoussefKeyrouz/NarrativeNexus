using UnityEngine;

namespace NarrativeNexus.Narrative
{
    /// <summary>
    /// Represents a choice option that can be selected by the player
    /// </summary>
    [System.Serializable]
    public class Choice
    {
        [SerializeField] private string text;
        [SerializeField] private string targetNodeId;
        [SerializeField] private SerializableCondition condition;

        public string Text => text;
        public string TargetNodeId => targetNodeId;

        /// <summary>
        /// Checks if this choice is available based on the current game state
        /// </summary>
        public bool IsAvailable(IGameState gameState)
        {
            var conditionObj = condition?.GetCondition();
            return conditionObj == null || conditionObj.IsMet(gameState);
        }

        /// <summary>
        /// Constructor for creating choices in code
        /// </summary>
        public Choice(string choiceText, string target)
        {
            text = choiceText;
            targetNodeId = target;
            condition = new SerializableCondition();
        }

        /// <summary>
        /// Constructor with condition
        /// </summary>
        public Choice(string choiceText, string target, string flagKey, bool flagValue)
        {
            text = choiceText;
            targetNodeId = target;
            condition = new SerializableCondition();
            condition.SetFlagCondition(flagKey, flagValue);
        }

        /// <summary>
        /// Default constructor for Unity serialization
        /// </summary>
        public Choice()
        {
            text = "";
            targetNodeId = "";
            condition = new SerializableCondition();
        }
    }
}
