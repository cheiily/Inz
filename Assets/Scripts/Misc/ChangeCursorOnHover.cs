using System;
using Data;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Misc {
    public class ChangeCursorOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
        public GameConfiguration _config;
        public CursorOverride _cursorOverride;
        public bool _pointerInside;

        private void Awake() {
            _config = GameObject.FindWithTag("Manager").GetComponent<GameManager>().config;
            _cursorOverride = GetComponent<CursorOverride>();
        }

        public void OnPointerEnter(PointerEventData eventData) {
            _pointerInside = true;
            SetEffectiveHoverCursor();
        }

        public void OnPointerExit(PointerEventData eventData) {
            _pointerInside = false;
            Cursor.SetCursor(_config.cursorDefault, new Vector2(20,15), CursorMode.Auto);
        }


        public void SetEffectiveHoverCursor() {
            if (_cursorOverride != null && _cursorOverride.cursorHoverOverride != null)
                Cursor.SetCursor(_cursorOverride.cursorHoverOverride, _cursorOverride.cursorHotspot, CursorMode.Auto);
            else
                Cursor.SetCursor(_config.cursorHover, _config.cursorHoverHotspot, CursorMode.Auto);
        }
    }
}