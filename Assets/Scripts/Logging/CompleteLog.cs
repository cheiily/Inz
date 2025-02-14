using System;
using System.Collections.Generic;

namespace InzGame {
    [Serializable]
    public class CompleteLog {
        public string gameVariantString;
        public CustomerEvaluation.Method evaluationMethod;
        public string evaluationMethod_name;
        public List<LevelLogEntry> levelLog = new List<LevelLogEntry>();
    }
}