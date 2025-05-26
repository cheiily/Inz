using System.Collections.Generic;
using InzGame;
using UnityEngine;

namespace Data {
    [CreateAssetMenu(fileName = "CookingAction", menuName = "Scriptable Objects/CookingAction")]
    public class CookingAction : ScriptableObject {
        public List<Element> input;
        public Element output;
        public float duration;
        public float expiryDuration;
        public List<Sprite> spritesExtraDisplay;

        public HashSet<Element> inputSet;
        // public List<Element> preview;

        public HashSet<Element> GetInputSet() {
            if (inputSet == null)
                inputSet = new HashSet<Element>(input);
            return inputSet;
        }
    }
}
