using InzGame.DisplayHandlers;
using UnityEngine;
using UnityEngine.UI;

namespace InzGame {
    [UnityEditor.CustomEditor(typeof(FoodProcessorDisplay))]
    public class FoodProcessorDisplayDebugEditor : UnityEditor.Editor {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            FoodProcessorDisplay foodProcessorDisplay = (FoodProcessorDisplay)target;

            if (GUILayout.Button("Set White to All")) {
                foreach (var display in foodProcessorDisplay._diegeticBufferImages) {
                    if (display != null) {
                        display.color = Color.white;
                    }
                }
            }
            if (GUILayout.Button("Set Clear to All")) {
                foreach (var display in foodProcessorDisplay._diegeticBufferImages) {
                    if (display != null) {
                        display.color = Color.clear;
                    }
                }
            }
        }
    }
}