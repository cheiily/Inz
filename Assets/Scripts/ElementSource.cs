using System;
using InzGame;
using UnityEngine;
using Buffer = InzGame.Buffer;

public class ElementSource : MonoBehaviour {
    public Element element;
    public Buffer buffer;

    public void Awake() {
        buffer = GameObject.FindWithTag("Manager").GetComponent<Buffer>();
    }

    public void OnTap() {
        buffer.Submit(element);
    }
}
