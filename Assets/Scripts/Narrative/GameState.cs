using System;
using System.Collections.Generic;
using UnityEngine;

namespace NarrativeNexus.Narrative
{
    /// <summary>
    /// Concrete implementation of IGameState that manages flags and stats
    /// </summary>
    [System.Serializable]
    public class GameState : IGameState
    {
        [SerializeField] private Dictionary<string, bool> flags = new Dictionary<string, bool>();
        [SerializeField] private Dictionary<string, float> stats = new Dictionary<string, float>();

        public event Action<string, bool> OnFlagChanged;
        public event Action<string, float> OnStatChanged;

        #region Flag Operations
        public bool GetFlag(string key)
        {
            return flags.ContainsKey(key) ? flags[key] : false;
        }

        public void SetFlag(string key, bool value)
        {
            if (string.IsNullOrEmpty(key)) return;

            bool previousValue = GetFlag(key);
            flags[key] = value;

            if (previousValue != value)
            {
                OnFlagChanged?.Invoke(key, value);
            }
        }
        #endregion

        #region Stat Operations
        public float GetStat(string key)
        {
            return stats.ContainsKey(key) ? stats[key] : 0f;
        }

        public void SetStat(string key, float value)
        {
            if (string.IsNullOrEmpty(key)) return;

            float previousValue = GetStat(key);
            stats[key] = value;

            if (!Mathf.Approximately(previousValue, value))
            {
                OnStatChanged?.Invoke(key, value);
            }
        }
        #endregion

        #region Utility Methods
        public void ClearAll()
        {
            flags.Clear();
            stats.Clear();
        }

        public Dictionary<string, bool> GetAllFlags()
        {
            return new Dictionary<string, bool>(flags);
        }

        public Dictionary<string, float> GetAllStats()
        {
            return new Dictionary<string, float>(stats);
        }
        #endregion

        #region Serialization Support
        // Unity doesn't serialize Dictionary by default, so we need custom serialization
        [System.Serializable]
        public struct FlagEntry
        {
            public string key;
            public bool value;
        }

        [System.Serializable]
        public struct StatEntry
        {
            public string key;
            public float value;
        }

        [SerializeField] private FlagEntry[] serializedFlags = new FlagEntry[0];
        [SerializeField] private StatEntry[] serializedStats = new StatEntry[0];

        public void OnBeforeSerialize()
        {
            // Convert dictionaries to arrays for serialization
            serializedFlags = new FlagEntry[flags.Count];
            int i = 0;
            foreach (var kvp in flags)
            {
                serializedFlags[i] = new FlagEntry { key = kvp.Key, value = kvp.Value };
                i++;
            }

            serializedStats = new StatEntry[stats.Count];
            i = 0;
            foreach (var kvp in stats)
            {
                serializedStats[i] = new StatEntry { key = kvp.Key, value = kvp.Value };
                i++;
            }
        }

        public void OnAfterDeserialize()
        {
            // Convert arrays back to dictionaries after deserialization
            flags = new Dictionary<string, bool>();
            foreach (var entry in serializedFlags)
            {
                flags[entry.key] = entry.value;
            }

            stats = new Dictionary<string, float>();
            foreach (var entry in serializedStats)
            {
                stats[entry.key] = entry.value;
            }
        }
        #endregion
    }
}
