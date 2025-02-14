using System;
using System.Collections;
using System.Collections.Generic;
using Data;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace InzGame.DisplayHandlers {
    public class FoodProcessorDisplay : MonoBehaviour {
        public GameObject prop;

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
            _processor.OnStatusChange += ToggleSlider;
            _processor.OnMoveToMainBuffer += TweenItems;
            if (prop != null)
                _processor.OnStatusChange += ToggleProp;
        }

        public void SetSliderProgress(object sender, Tuple<float, bool> progressTuple) {
            _progressSlider.value = progressTuple.Item1;
            // _progressSlider.fillRect.GetComponent<Image>().color = progressTuple.Item2 ? Color.red : Color.green;
        }

        public void SetBufferImages(object sender, List<Element> buffer) {
            for (int i = 0; i < 5; ++i) {
                if (i >= buffer.Count || buffer[i] is Element.NONE or Element.INVALID) {
                    _bufferImages[ i ].DOKill();
                    _bufferImages[i].sprite = null;
                    _bufferImages[i].color = Color.clear;
                } else {
                    if (_processor.status is FoodProcessor.Status.DONE or FoodProcessor.Status.EXPIRING) {
                        SetSprite(i, buffer);
                        continue;
                    }
                    var i1 = i;
                    // this is specifically bound to the item in order to easily cancel the tween when needed
                    _bufferImages[i].DOColor(_bufferImages[i].color, _config.itemJumpDuration).From(_bufferImages[i].color)
                                    .OnComplete(() => SetSprite(i1, buffer));
                }
            }
        }

        public void SetSprite(int i, List<Element> buffer) {
            _bufferImages[i].sprite = _config.elementProperties.GetFor(buffer[i]).sprite_element;
            _bufferImages[i].preserveAspect = true;
            _bufferImages[i].color = Color.white;
        }

        public void SetAnimatorState(object sender, FoodProcessor.Status state) {
            _animator.SetInteger(_stateParam, (int)state);
        }

        public void ToggleProp(object sender, FoodProcessor.Status state) {
            switch (state) {
                case FoodProcessor.Status.ACTIVE:
                case FoodProcessor.Status.EXPIRING:
                    prop.SetActive(false);
                    break;
                case FoodProcessor.Status.DONE:
                case FoodProcessor.Status.FREE:
                    prop.SetActive(true);
                    break;
            }
        }

        public void ToggleSlider(object sender, FoodProcessor.Status state) {
            if (state == FoodProcessor.Status.FREE && _processor.currentAction == null) {
                _progressSlider.gameObject.SetActive(false);
                return;
            }

            _progressSlider.gameObject.SetActive(true);
            var fillImg = _progressSlider.fillRect.GetComponent<Image>();
            switch (state) {
                case FoodProcessor.Status.FREE:
                    fillImg.color = Color.cyan;
                    break;
                case FoodProcessor.Status.ACTIVE:
                    fillImg.color = Color.green;
                    break;
                case FoodProcessor.Status.EXPIRING:
                    fillImg.color = Color.red;
                    break;

                // don't change color if done, keep active/expired dependency
            }
        }

        public void TweenItems(List<Element> elements, List<int> indices) {
            GameObject.FindWithTag("ItemJumpTweener").GetComponent<ItemJumpTweener>().Processor(elements, indices, this);
        }
    }
}