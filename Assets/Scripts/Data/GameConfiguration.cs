using System.Collections.Generic;
using DG.Tweening;
using InzGame;
using UnityEngine;

namespace Data {
    [CreateAssetMenu(fileName = "GameConfiguration", menuName = "Scriptable Objects/GameConfiguration")]
    public class GameConfiguration : ScriptableObject {
        [Header("Levels")]
        public LevelData levelTutorial;
        public LevelData level1;
        public LevelData level2;
        public string gameVariantString;

        [Header("System")]
        public int bufferSize = 3;
        public ElementProperties elementProperties;
        public Texture2D cursorDefault;
        public Vector2 cursorDefaultHotspot = new Vector2(20, 15);
        public Texture2D cursorHover;
        public Vector2 cursorHoverHotspot = new Vector2(15, 20);
        public Texture2D cursorGrab;
        public Vector2 cursorGrabHotspot = new Vector2(20, 20);

        [Header("Customers")]
        public CustomerEvaluation.Method evaluationMethod;
        public List<Sprite> moodSprites;
        public float entryWaitTime = 5f;

        [Header("Summary")]
        public List<float> totalMoodThresholds;
        public List<Sprite> totalMoodSprites;
        public Sprite buzkaPusta;

        [Header("Item Jump Tweener")]
        public float itemJumpDuration = 0.5f;
        public Ease itemJumpEase = Ease.OutExpo;
    }
}
