using UnityEngine;

namespace NarrativeNexus.Narrative
{
    /// <summary>
    /// Interface for conditions that determine if a choice is available to the player
    /// </summary>
    public interface IChoiceCondition
    {
        bool IsMet(IGameState state);
    }

    /// <summary>
    /// Basic condition that checks if a flag has a specific value
    /// </summary>
    [System.Serializable]
    public class FlagCondition : IChoiceCondition
    {
        [SerializeField] private string flagKey;
        [SerializeField] private bool expectedValue;

        public FlagCondition(string key, bool expected)
        {
            flagKey = key;
            expectedValue = expected;
        }

        public bool IsMet(IGameState state)
        {
            return state.GetFlag(flagKey) == expectedValue;
        }
    }

    /// <summary>
    /// Serializable wrapper for choice conditions to support Unity serialization
    /// </summary>
    [System.Serializable]
    public class SerializableCondition
    {
        public enum ConditionType
        {
            None,
            Flag
        }

        [SerializeField] private ConditionType conditionType = ConditionType.None;
        [SerializeField] private string flagKey;
        [SerializeField] private bool flagValue;

        public IChoiceCondition GetCondition()
        {
            switch (conditionType)
            {
                case ConditionType.Flag:
                    return new FlagCondition(flagKey, flagValue);
                default:
                    return null;
            }
        }

        public void SetFlagCondition(string key, bool value)
        {
            conditionType = ConditionType.Flag;
            flagKey = key;
            flagValue = value;
        }
    }
}
