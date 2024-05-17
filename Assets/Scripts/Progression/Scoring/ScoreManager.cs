using System.Collections.Generic;
using Events.GameEvents.Typed;
using Progression.Grading;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Progression.Scoring
{
    /// <summary>
    /// Manages the scoring system for the game.
    /// </summary>
    [CreateAssetMenu(fileName = "Score Manager", menuName = "Progression/Score Manager", order = 0)]
    public class ScoreManager : ScriptableObject
    {
        [Header("Game Data")] 
        [SerializeField] private GameProgressionData gameProgressionContainer;
        
        [Header("Score & Grading Settings")] 
        [SerializeField] private bool allowNegativeScore;
        [SerializeField] private List<LevelGradingSettingsEntry> gradingSettings;
        
        [Header("Events")] 
        [SerializeField] private IntGameEvent onUpdateScore;
        [SerializeField] private IntGameEvent onScoreChanged;
        [SerializeField] private GradeGameEvent onGradeChanged;
        
        /// <summary>
        /// The current grade of the player in the active scene.
        /// </summary>
        public Grade CurrentGrade { get; private set; }
        
        /// <summary>
        /// The current score of the player in the active scene.
        /// </summary>
        public int CurrentScore { get; private set; }
        
        private void OnEnable()
        {
            SceneManager.activeSceneChanged += ResetScoringSystem;
        }
        
        private void OnDisable()
        {
            SceneManager.activeSceneChanged -= ResetScoringSystem;
        }
        
        private void OnValidate()
        {
            onUpdateScore?.RemoveListener(UpdateScore);
            onUpdateScore?.AddListener(UpdateScore);
        }
        
        private void ResetScoringSystem(Scene arg0, Scene arg1)
        {
            CurrentScore = 0;
            
            UpdateGameProgression();
        }
        
        private void UpdateScore(int points)
        {
            var newScore = CurrentScore + points;
            
            if (newScore < 0 && allowNegativeScore)
            {
                CurrentScore = 0;
                
                UpdateGameProgression();
            }
            else
            {
                CurrentScore = newScore;
                
                UpdateGameProgression();
            }
        }
        
        private void UpdateGameProgression()
        {
            onScoreChanged?.Invoke(CurrentScore);
            
            var activeSceneName = SceneManager.GetActiveScene().name;
            var gradingSystem = gradingSettings.Find(entry => entry.LevelName == activeSceneName).GradingSystem;
            
            CurrentGrade = gradingSystem.CalculateGrade(CurrentScore);
            
            onGradeChanged?.Invoke(CurrentGrade);
            
            gameProgressionContainer.UpdateOrAddLevelScore(
                activeSceneName,
                new LevelScoreData(CurrentScore, CurrentGrade)
            );
        }
    }
}