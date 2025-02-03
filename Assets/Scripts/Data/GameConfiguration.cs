using InzGame;
using UnityEngine;

namespace Data {
    [CreateAssetMenu(fileName = "GameConfiguration", menuName = "Scriptable Objects/GameConfiguration")]
    public class GameConfiguration : ScriptableObject {
        public int bufferSize = 3;
        public ElementProperties elementProperties;
        public CustomerEvaluation.Method evaluationMethod;
        public float itemJumpDuration = 0.5f;
    }
}
