using UnityEngine;
using NarrativeNexus.Narrative;

namespace NarrativeNexus.Managers
{
    /// <summary>
    /// Lightweight static holder for passing data between scenes
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        private static GameManager instance;
        private static StoryData pendingStory;

        /// <summary>
        /// Story data to be loaded in the next scene
        /// </summary>
        public static StoryData PendingStory 
        { 
            get => pendingStory; 
            set => pendingStory = value; 
        }

        /// <summary>
        /// Singleton instance access
        /// </summary>
        public static GameManager Instance
        {
            get
            {
                if (instance == null)
                {
                    // Try to find existing instance
                    instance = FindObjectOfType<GameManager>();
                    
                    // Create new instance if none exists
                    if (instance == null)
                    {
                        GameObject go = new GameObject("GameManager");
                        instance = go.AddComponent<GameManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }

        #region Unity Lifecycle
        private void Awake()
        {
            // Ensure only one instance exists
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            // Log warning if we're in StoryGameplay scene but no pending story
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "StoryGameplay")
            {
                if (pendingStory == null)
                {
                    Debug.LogWarning("StoryGameplay scene loaded but no PendingStory set. Did you navigate here correctly?");
                }
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Clear the pending story (call after loading)
        /// </summary>
        public static void ClearPendingStory()
        {
            pendingStory = null;
        }

        /// <summary>
        /// Check if there's a pending story to load
        /// </summary>
        public static bool HasPendingStory()
        {
            return pendingStory != null;
        }
        #endregion
    }
}
