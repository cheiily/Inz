using System;
using System.Collections.Generic;

namespace InzGame {
    [Serializable]
    public class LevelLogEntry {
        public int level;
        public List<CustomerLogEntry> customerLog = new List<CustomerLogEntry>();
    }
}