using System;
using System.Collections.Generic;
using Data;
using UnityEngine;
using UnityEngine.UI;

namespace InzGame.DisplayHandlers {
    public class BufferDisplay : MonoBehaviour {
        public Buffer _buffer;
        public GameConfiguration _config;
        public List<Image> anchors;

        private void Awake() {
            _buffer = GetComponent<Buffer>();
            _config = GetComponent<GameManager>().config;
            foreach (Transform child in GameObject.FindWithTag("Buffer Display").transform) {
                anchors.Add(child.GetComponent<Image>());
            }

            _buffer.OnBufferChange += SetSprites;

            foreach (var anchor in anchors) {
                anchor.sprite = null;
                anchor.color = Color.clear;
            }
        }

        public void SetSprites(object sender, List<Element> bufferState) {
            for (int i = 0; i < bufferState.Count; ++i) {
                if ( bufferState[ i ] is Element.NONE or Element.INVALID ) {
                    anchors[ i ].sprite = null;
                    anchors[ i ].color = Color.clear;
                } else {
                    anchors[ i ].sprite = _config.elementProperties.GetFor(bufferState[ i ]).sprite_element;
                    anchors[ i ].color = Color.white;
                }
            }
        }
    }
}