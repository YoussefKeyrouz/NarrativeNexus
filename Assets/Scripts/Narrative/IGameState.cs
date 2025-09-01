using System;
using System.Collections.Generic;

namespace NarrativeNexus.Narrative
{
    /// <summary>
    /// Interface for accessing game state (flags and stats)
    /// </summary>
    public interface IGameState
    {
        // Flag operations (string -> bool)
        bool GetFlag(string key);
        void SetFlag(string key, bool value);
        
        // Stat operations (string -> float)
        float GetStat(string key);
        void SetStat(string key, float value);
        
        // Events for state changes
        event Action<string, bool> OnFlagChanged;
        event Action<string, float> OnStatChanged;
        
        // Utility
        void ClearAll();
        Dictionary<string, bool> GetAllFlags();
        Dictionary<string, float> GetAllStats();
    }
}
