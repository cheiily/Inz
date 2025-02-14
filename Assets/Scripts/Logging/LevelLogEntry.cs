using System;
using System.Collections.Generic;

namespace InzGame {
    [Serializable]
    public class LevelLogEntry {
        public int level;
        public string levelAssetName;

        public int numTotal;
        public int numHappy;
        public int numMid;
        public int numSad;
        public int numLeave;

        public float avgPts;
        public float avgThreshold;
        public float avgLifetime;

        public float avgThresholdLength;
        public float avgLifetimeThreshold;

        public List<CustomerLogEntry> customerLog = new List<CustomerLogEntry>();
    }
}