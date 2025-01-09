using System.Collections.Generic;
using UnityEngine;

namespace Data {
    [CreateAssetMenu(fileName = "FoodProcessorPreset", menuName = "Scriptable Objects/FoodProcessorPreset")]
    public class FoodProcessorPreset : ScriptableObject {
        public enum FoodProcessorType {
            POT,
            PAN,
            CUTTING_BOARD,
        }

        public List<CookingAction> actions;
        public FoodProcessorType type;
    }
}
