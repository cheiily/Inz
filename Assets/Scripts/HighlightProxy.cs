using System;
using System.Collections.Generic;
using InzGame.DisplayHandlers;
using Misc;
using UnityEngine;
using UnityEngine.EventSystems;

namespace InzGame {
    public class HighlightProxy : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
        public BufferDisplay bufferDisplay;
        public Func<List<Element>> HighlightCheck;
        public CursorOverride _cursorOverride;
        public ChangeCursorOnHover _cursorManager;

        private void Awake() {
            bufferDisplay = GameObject.FindWithTag("Manager").GetComponent<BufferDisplay>();
            _cursorOverride = GetComponent<CursorOverride>();
            _cursorManager = GetComponent<ChangeCursorOnHover>();
        }

        public void OnPointerEnter(PointerEventData eventData) {
            DoHighlight();
        }

        public void OnPointerExit(PointerEventData eventData) {
            DoStopHighlight();
        }


        public void DoHighlight() {
            var elements = HighlightCheck?.Invoke();
            if (elements == null) return;

            bufferDisplay.StartHighlight(elements);
        }

        public void DoStopHighlight() {
            bufferDisplay.StopHighlight();
        }
    }
}