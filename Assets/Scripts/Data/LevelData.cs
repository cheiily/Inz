using System.Collections.Generic;
using UnityEngine;

namespace Data {
    [CreateAssetMenu(fileName = "LevelData", menuName = "Scriptable Objects/LevelData")]
    public class LevelData : ScriptableObject {
        public enum PlayMode {
            TIMER,
            TIMER_DIEGETIC,
            CLICKER_DIEGETIC
        }

        public static bool IsDiegetic(PlayMode playMode) {
            return playMode is not PlayMode.TIMER;
        }

        public CustomerSpawningPattern customerSpawningPattern;
        public List<FoodProcessorPreset> availableProcessors;
        public Sprite recipeBook;
        public PlayMode playMode;
    }
}
