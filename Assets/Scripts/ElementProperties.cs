using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using InzGame;
using UnityEngine;

[CreateAssetMenu(fileName = "ElementProperties", menuName = "Scriptable Objects/ElementProperties")]
public class ElementProperties : ScriptableObject
{
    [Serializable]
    public class ElementData {
        public Element element;
        public int level;
    }

    public List<ElementData> properties;

    public ElementData GetFor(Element element) {
        foreach (var data in properties) {
            if ( data.element == element )
                return data;
        }

        return new ElementData();
    }
}
