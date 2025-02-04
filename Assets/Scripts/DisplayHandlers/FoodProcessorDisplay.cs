using System;
using System.Collections.Generic;
using Data;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace InzGame.DisplayHandlers {
    public class FoodProcessorDisplay : MonoBehaviour {
        public GameConfiguration _config;
        public FoodProcessor _processor;
        public Slider _progressSlider;
        public List<Image> _bufferImages;
        public Animator _animator;
        public int _stateParam;

        private void Awake() {
            _config = GameObject.FindWithTag("Manager").GetComponent<GameManager>().config;
            _processor = GetComponent<FoodProcessor>();
            _progressSlider = transform.GetChild(0).GetComponent<Slider>();
            _animator = GetComponent<Animator>();
            _stateParam = Animator.StringToHash("Status");

            var anchor = transform.GetChild(1);
            foreach (Transform child in anchor.transform) {
                _bufferImages.Add(child.GetComponent<Image>());
            }

            _processor.OnBufferChange += SetBufferImages;
            _processor.OnProgressChange += SetSliderProgress;
            _processor.OnStatusChange += SetAnimatorState;
            _processor.OnMoveToMainBuffer += TweenItems;
        }

        public void SetSliderProgress(object sender, Tuple<float, bool> progressTuple) {
            _progressSlider.value = progressTuple.Item1;
            _progressSlider.fillRect.GetComponent<Image>().color = progressTuple.Item2 ? Color.red : Color.green;
        }

        public void SetBufferImages(object sender, List<Element> buffer) {
            for (int i = 0; i < 5; ++i) {
                if (i >= buffer.Count || buffer[i] is Element.NONE or Element.INVALID) {
                    _bufferImages[i].sprite = null;
                    _bufferImages[i].color = Color.clear;
                } else {
                    _bufferImages[i].sprite = _config.elementProperties.GetFor(buffer[i]).sprite_element;
                    _bufferImages[i].color = Color.white;
                }
            }
        }

        public void SetAnimatorState(object sender, FoodProcessor.Status state) {
            _animator.SetInteger(_stateParam, (int)state);
        }

        public void TweenItems(List<Element> elements, List<int> indices) {
            GameObject.FindWithTag("ItemJumpTweener").GetComponent<ItemJumpTweener>().Processor(elements, indices, this);
        }
    }
}