using System.Collections.Generic;
using DG.Tweening;
using InzGame;
using UnityEngine;

namespace Data {
    [CreateAssetMenu(fileName = "GameConfiguration", menuName = "Scriptable Objects/GameConfiguration")]
    public class GameConfiguration : ScriptableObject {
        [Header("Levels")]
        public LevelData level1;
        public LevelData level2;
        public string gameVariantString;

        [Header("System")]
        public int bufferSize = 3;
        public ElementProperties elementProperties;

        [Header("Customers")]
        public CustomerEvaluation.Method evaluationMethod;
        public List<Sprite> moodSprites;

        [Header("Summary")]
        public List<float> totalMoodThresholds;
        public List<Sprite> totalMoodSprites;
        public Sprite buzkaPusta;

        [Header("Item Jump Tweener")]
        public float itemJumpDuration = 0.5f;
        public Ease itemJumpEase = Ease.OutExpo;
    }
}
