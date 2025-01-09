using UnityEngine;

namespace Misc {
    public class RetainedObject : MonoBehaviour {
        private void Awake() {
            DontDestroyOnLoad(this);
        }
    }
}
