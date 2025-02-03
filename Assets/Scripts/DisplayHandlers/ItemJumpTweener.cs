using System;
using System.Collections.Generic;
using Data;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

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

            item.transform.DOMove(to.position, _config.itemJumpDuration).From(from.position).OnComplete(() => {
                item.gameObject.SetActive(false);
                item.GetComponent<Image>().sprite = null;
            });
            item.SetActive(true);
        }

        public void Source(ElementSource source, int slot) {
            GameObject item = itemPool.Find(obj => !obj.activeSelf);
            if (item == null) {
                Debug.Log("No available items in pool");
                return;
            }

            item.GetComponent<Image>().sprite = _config.elementProperties.GetFor(source.element).sprite_element;

            item.transform.DOMove(bufferDisplay.anchors[slot].transform.position, _config.itemJumpDuration).From(source.transform.position).OnComplete(() => {
                item.gameObject.SetActive(false);
                item.GetComponent<Image>().sprite = null;
            });
            item.SetActive(true);
        }
    }
}