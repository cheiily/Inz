using System.Collections.Generic;
using UnityEngine;

namespace Data {
    public static class PlayModeHelper {
        public static bool IsDiegetic(LevelData.PlayMode playMode) {
            return playMode is not LevelData.PlayMode.TIMER;
        }
    }

    [CreateAssetMenu(fileName = "LevelData", menuName = "Scriptable Objects/LevelData")]
    public class LevelData : ScriptableObject {
        public enum PlayMode {
            TIMER,
            TIMER_DIEGETIC,
            CLICKER_DIEGETIC
        }

        public CustomerSpawningPattern customerSpawningPattern;
        public List<FoodProcessorPreset> availableProcessors;
        public Sprite recipeBook;
        public PlayMode playMode;
    }
}
