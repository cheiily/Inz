using System;
using System.Collections.Generic;
using UnityEngine;

namespace Data {
    [CreateAssetMenu(fileName = "CustomerSpawningPattern", menuName = "Scriptable Objects/CustomerSpawningPattern")]
    public class CustomerSpawningPattern : ScriptableObject
    {
        public enum SpawningMode {
            REGULAR,
            RANDOM
        }

        [Serializable]
        public struct Regular_SpawnPoint {
            public float timeSinceStart;
            public float randomVariance;
            public CustomerPreset customer;
        }

        [Serializable]
        public struct SpawnPoint {
            public float timeSinceStart;
            public CustomerPreset customer;

            public SpawnPoint(float timeSinceStart, CustomerPreset customer) {
                this.timeSinceStart = timeSinceStart;
                this.customer = customer;
            }
        }

        public SpawningMode spawningMode;
        public List<Regular_SpawnPoint> regular_spawnPoints;
    }
}
