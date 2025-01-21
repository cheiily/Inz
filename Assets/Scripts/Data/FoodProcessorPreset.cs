using System.Collections.Generic;
using UnityEngine;

namespace Data {
    [CreateAssetMenu(fileName = "FoodProcessorPreset", menuName = "Scriptable Objects/FoodProcessorPreset")]
    public class FoodProcessorPreset : ScriptableObject {
        public enum FoodProcessorType {
            GARNEK,
            PATELNIA,
            DESKA_DO_KROJENIA,
            DESKA_DO_OBIERANIA,
            MISKA,
        }

        public List<CookingAction> actions;
        public FoodProcessorType type;
        public Sprite sprite;
    }
}
