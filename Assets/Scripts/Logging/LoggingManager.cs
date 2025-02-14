using System.Collections.Generic;
using System.Linq;
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

        public bool[] compiled = new bool[2];
        public string complete;

        public void OpenLevel(int level, string levelName) {
            if ( level == 1 ) {
                level1Log = new LevelLogEntry {
                    level = level,
                    levelAssetName = levelName
                };
            } else if ( level == 2 ) {
                level2Log = new LevelLogEntry {
                    level = level,
                    levelAssetName = levelName
                };
            }

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
            LevelLogEntry log = currentLevel == 1 ? level1Log : level2Log;
            log.level = currentLevel;
            log.customerLog = customerLog;

            log.numTotal = customerLog.Count;
            log.numHappy = customerLog.FindAll(entry => entry.threshold == 0).Count;
            log.numMid = customerLog.FindAll(entry => entry.threshold == 1).Count;
            log.numSad = customerLog.FindAll(entry => entry.threshold == 2).Count;
            log.numLeave = customerLog.FindAll(entry => entry.threshold == 3).Count;

            log.avgPts = customerLog.Average(entry => entry.points);
            log.avgThreshold = (float)customerLog.Average(entry => entry.threshold);
            log.avgLifetime = customerLog.Average(entry => entry.lifetime);

            log.avgThresholdLength = customerLog.SelectMany(entry => entry.ratingDropTimeThresholds).Average();
            log.avgLifetimeThreshold = log.avgLifetime / log.avgThreshold;

            if ( currentLevel == 1 ) {
                level1Log = log;
                level1 = JsonUtility.ToJson(log, true);
            }
            else if ( currentLevel == 2 ) {
                level2Log = log;
                level2 = JsonUtility.ToJson(log, true);
            }

            compiled[currentLevel - 1] = true;

            if ( compiled[0] && compiled[1] ) {
                CompileAll();
                Save();
            }
        }

        public void CompileAll() {
            var manager = GetComponent<GameManager>();

            completeLog = new CompleteLog();
            completeLog.gameVariantString = manager.config.gameVariantString;
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