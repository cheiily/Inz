using System.Collections.Generic;
using UnityEngine;

namespace Data {
    [CreateAssetMenu(fileName = "LevelData", menuName = "Scriptable Objects/LevelData")]
    public class LevelData : ScriptableObject {
        public CustomerSpawningPattern customerSpawningPattern;
        public List<FoodProcessorPreset> availableProcessors;
    }
}
