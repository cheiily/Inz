using System;
using System.Collections.Generic;
using InzGame;
using UnityEngine;

namespace Data {
    [CreateAssetMenu(fileName = "ElementProperties", menuName = "Scriptable Objects/ElementProperties")]
    public class ElementProperties : ScriptableObject
    {
        [Serializable]
        public struct DiegeticSpriteData {
            public FoodProcessorPreset.FoodProcessorType foodProcessorType;
            public List<Sprite> sprites;
        }

        [Serializable]
        public class ElementData {
            public Element element;
            public int level;
            public Sprite sprite_element;
            public Sprite sprite_order_bubble;
            public List<DiegeticSpriteData> sprites_cooking_progress;
        }

        public List<ElementData> properties;

        public ElementData GetFor(Element element) {
            foreach (var data in properties) {
                if ( data.element == element )
                    return data;
            }

            return new ElementData();
        }

        public DiegeticSpriteData GetFor(FoodProcessorPreset.FoodProcessorType foodProcessorType, Element element) {
            ElementData data = GetFor(element);
            foreach (var diegeticSpriteData in data.sprites_cooking_progress) {
                if ( diegeticSpriteData.foodProcessorType == foodProcessorType )
                    return diegeticSpriteData;
            }

            return new DiegeticSpriteData();
        }

        public Element filter;
        public int _indexPreview;

        public ElementData found;

        private void OnValidate() {
            foreach (var data in properties) {
                if ( data.element == filter ) {
                    found = data;
                    _indexPreview = properties.IndexOf(data);

                    return;
                }
            }

            found = null;
        }
    }
}
