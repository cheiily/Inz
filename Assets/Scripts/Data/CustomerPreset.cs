using System;
using System.Collections.Generic;
using InzGame;
using UnityEngine;

namespace Data {
    [CreateAssetMenu(fileName = "CustomerPreset", menuName = "Scriptable Objects/CustomerPreset")]
    public class CustomerPreset : ScriptableObject {
        public enum RatingDropMode {
            REGULAR,
            RANDOM
        }

        [Serializable]
        public struct Random_ThresholdData {
            public float min;
            public float max;
        }

        public RatingDropMode ratingDropMode;
        public List<float> regular_ratingDropTimeThresholds;
        public List<Random_ThresholdData> random_ratingDropTimeThresholds;
        public Element order;
    }
}
