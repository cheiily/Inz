using Data;
using UnityEngine;

namespace Misc {
    public class CursorOverride : MonoBehaviour {
        public GameConfiguration _config;

        private void Awake() {
            _config = GameObject.FindWithTag("Manager").GetComponent<GameManager>().config;
        }

        public Texture2D cursorHoverOverride;
        public Vector2 cursorHotspot;
    }
}