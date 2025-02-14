using System;
using System.Collections.Generic;
using Data;
using UnityEngine.Serialization;

namespace InzGame {
    [Serializable]
    public class CustomerLogEntry {
        public int id;
        public Element order;
        public string order_name;
        public List<float> ratingDropTimeThresholds;
        public float time_entered;
        public float time_departed;
        public float lifetime;
        public float points;
        public int threshold;
    }
}