using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using static Misc.Extensions;

namespace InzGame.DisplayHandlers {
    public class ItemJumpTweener : MonoBehaviour {
        public BufferDisplay bufferDisplay;

        public GameConfiguration _config;
        public List<GameObject> itemPool;

        private void Awake() {
            _config = GameObject.FindWithTag("Manager").GetComponent<GameManager>().config;
            itemPool = new List<GameObject>();
            foreach (Transform child in transform) {
                itemPool.Add(child.gameObject);
            }
        }

        public void StartWith(Element elem, Transform from, Transform to) {
            GameObject item = itemPool.Find(obj => !obj.activeSelf);
            if (item == null) {
                Debug.Log("No available items in pool");
                return;
            }

            item.GetComponent<Image>().sprite = _config.elementProperties.GetFor(elem).sprite_element;

            item.transform.DOMove(to.position, _config.itemJumpDuration).SetEase(_config.itemJumpEase).From(from.position).OnComplete(() => {
                item.gameObject.SetActive(false);
                item.GetComponent<Image>().sprite = null;
            });
            item.SetActive(true);
        }

        public void Source(ElementSource source, int slot) {
            StartWith(source.element, source.transform, bufferDisplay.anchors[slot].transform);
        }

        public void Consumer(ElementConsumer consumer, List<Element> elements) {
            foreach (var displayTuple in bufferDisplay.m_displayBuffer) {
                if (displayTuple.Item1 != Element.INVALID && Misc.Extensions.Contains(elements, displayTuple.Item1)) {
                    StartWith(displayTuple.Item1, bufferDisplay.anchors[displayTuple.Item2].transform, consumer.transform);
                }
            }
        }

        public void Processor(List<Element> elements, List<int> indices, FoodProcessorDisplay processorDisplay) {
            for (int i = 0; i < elements.Count; i++) {
                var elem = elements[i];
                var idx = bufferDisplay._buffer.buffer.LastIndexOf(elem);

                StartWith(elem, processorDisplay._bufferImages[idx].transform, bufferDisplay.anchors[idx].transform);
            }
        }
    }
}