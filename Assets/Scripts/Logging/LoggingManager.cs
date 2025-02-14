using System.Collections.Generic;
using UnityEngine;

namespace InzGame {
    public class LoggingManager : MonoBehaviour {
        public List<CustomerLogEntry> customerLog = new List<CustomerLogEntry>();
        public LevelLogEntry level1Log;
        public LevelLogEntry level2Log;
        public CompleteLog completeLog;

        public int currentLevel = 0;

        public string level1;
        public string level2;

        public string complete;

        public void OpenLevel(int level) {
            currentLevel = level;
            customerLog.Clear();
        }

        public CustomerLogEntry Get(int customerID) {
            if (currentLevel == 0) return null;

            CustomerLogEntry entry = customerLog.Find(entry => entry.id == customerID);
            if ( entry == null ) {
                entry = new CustomerLogEntry();
                entry.id = customerID;
                customerLog.Add(entry);
            }

            return entry;
        }

        public void Set(CustomerLogEntry entry) {
            if ( currentLevel == 0 ) {
                Debug.LogError("Couldn't set customer log entry, no level is open.");
                return;
            }

            int idx = customerLog.FindIndex(existing => existing.id == entry.id);
            if ( idx == -1 ) {
                customerLog.Add(entry);
            }
            else {
                customerLog[idx] = entry;
            }
        }

        public void CompileLevel() {
            LevelLogEntry log = new LevelLogEntry();
            log.level = currentLevel;
            log.customerLog = customerLog;

            if ( currentLevel == 1 ) {
                level1Log = log;
                level1 = JsonUtility.ToJson(log, true);
            }
            else if ( currentLevel == 2 ) {
                level2Log = log;
                level2 = JsonUtility.ToJson(log, true);
            }
        }

        public void CompileAll() {
            var manager = GetComponent<GameManager>();

            completeLog = new CompleteLog();
            completeLog.evaluationMethod = manager.config.evaluationMethod;
            completeLog.evaluationMethod_name = manager.config.evaluationMethod.ToString();
            completeLog.levelLog.Add(level1Log);
            completeLog.levelLog.Add(level2Log);
            complete = JsonUtility.ToJson(completeLog, true);
        }

        public void Save() {
            System.IO.File.WriteAllText("complete.log", complete);
        }

    }
}