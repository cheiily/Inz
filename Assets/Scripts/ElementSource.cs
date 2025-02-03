using System;
using InzGame;
using InzGame.DisplayHandlers;
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
        GameObject.FindWithTag("ItemJumpTweener").GetComponent<ItemJumpTweener>().Source(this, Math.Clamp(buffer.count - 1, 0, buffer.size - 1));
    }
}
