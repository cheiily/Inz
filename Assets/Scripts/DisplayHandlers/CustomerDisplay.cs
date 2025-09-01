using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using DG.Tweening;
using Misc;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace InzGame.DisplayHandlers {
    public class CustomerDisplay : MonoBehaviour {
        public Image personImage;
        public Image personImageNext_Dg;
        public Image orderImage;
        public List<Sprite> personSprites;

        public Slider _slider;
        public CustomerInstance _customer;
        public Animator _animator;
        public int _thresholdParam;
        public GameConfiguration _config;
        public Color[] _colors = new Color[] {
            Color.green, Color.yellow, Color.red, Color.clear
        };
        public int _prevThreshold = 0;

        public ParticlePlayer _particlePlayer;

        public CursorOverride _cursorOverride;
        public ChangeCursorOnHover _cursorManager;

        private GameManager _gameManager;

        private void Awake() {
            _customer = GetComponent<CustomerInstance>();
            _slider = transform.GetComponentInChildren<Slider>();
            _animator = transform.parent.GetComponent<Animator>();
            _thresholdParam = Animator.StringToHash("Threshold");
            _gameManager = GameObject.FindWithTag("Manager").GetComponent<GameManager>();
            _config = _gameManager.config;

            _particlePlayer = GameObject.FindWithTag("CustomerParticles").transform.GetChild(_customer.seat).GetComponent<ParticlePlayer>();
            // _particleAnimator = GameObject.FindWithTag("CustomerParticles").transform.GetChild(_customer.seat).GetComponent<Animator>();
            // _particleImages = _particleAnimator.gameObject.GetComponentsInChildren<Image>(true).ToList();
            // _particleParam = Animator.StringToHash("PlayParticles");

            _cursorOverride = GetComponentInChildren<CursorOverride>();
            _cursorManager = GetComponentInChildren<ChangeCursorOnHover>();

            _customer.BeforeCustomerRemove += PlayParticles;
            _customer.BeforeCustomerRemove += PlayCustomerLeave;

            _customer.OnPatienceChange += SetSliderValue;
            _customer.OnPatienceChange += SetAnimatorState;
            if ( PlayModeHelper.IsDiegetic(_gameManager.currentLevel.playMode) ) {
                _customer.OnPatienceChange += SetPersonImage_Dg;
            } else {
                _customer.OnPatienceChange += SetPersonImage;
            }

            // only possible due to separate builds -> for dynamic check at call time (also because object is awoken after level selection)
            if (_gameManager.currentLevel.playMode == LevelData.PlayMode.CLICKER_DIEGETIC) {
                orderImage.gameObject.SetActive(false);
                _customer.OnFirstClick += ShowOrderImage;
            }
        }

        public void SetSliderValue(object sender, Tuple<float, int> patienceTuple) {
            if ( PlayModeHelper.IsDiegetic(_gameManager.currentLevel.playMode) ) {
                _slider.gameObject.SetActive(false);
                return;
            }

            _slider.value = patienceTuple.Item1;
            _slider.fillRect.GetComponent<Image>().color = _colors[ patienceTuple.Item2 ];
            _slider.handleRect.GetComponent<Image>().sprite = _config.moodSprites[ patienceTuple.Item2 ];
        }

        public void SetAnimatorState(object sender, Tuple<float, int> patienceTuple) {
            _animator.SetInteger(_thresholdParam, patienceTuple.Item2);
        }

        public void SetPersonImage(object sender, Tuple<float, int> patienceTuple) {
            if ( patienceTuple.Item2 == _prevThreshold )
                return;

            personImage.sprite = personSprites[ patienceTuple.Item2 ];
            _prevThreshold = patienceTuple.Item2;
        }

        public void SetPersonImage_Dg(object sender, Tuple<float, int> patienceTuple) {
            if ( patienceTuple.Item2 != _prevThreshold && _customer.currentThreshold < _customer.thresholds.Count ) {
                personImage.sprite = personSprites[ patienceTuple.Item2 ];
                personImageNext_Dg.sprite = personSprites[ patienceTuple.Item2 + 1 ];
            }

            // tweening alpha
            var thresholdProgress = 1 - patienceTuple.Item1;
            personImageNext_Dg.color = new Color(1, 1, 1, thresholdProgress);

            _prevThreshold = patienceTuple.Item2;
        }

        public void PlayCustomerLeave(object sender, EventArgs _) {
            // _animator.SetInteger(_thresholdParam, 3);
            _animator.Play("Leave");
        }

        public void PlayParticles(object sender, EventArgs _) {
            var points = CustomerEvaluation.methods[ _gameManager.config.evaluationMethod ](_customer);
            if ( PlayModeHelper.IsDiegetic(_gameManager.currentLevel.playMode) ) {
                var numParticles =
                    _customer.currentThreshold != 3 ?
                        4 - _customer.currentThreshold
                        : 0;
                _particlePlayer.PlayParticles(numParticles, _config.diegeticParticleSprite, points / numParticles);
            } else {
                _particlePlayer.PlayParticles(4, _config.moodSprites[_customer.currentThreshold], points / 4);
            }
        }

        //todo fix NDT customers coming in pissed

        public void AdjustCursorOverrideUnityEvent() {
            if ( ElementConsumer._buffer.Contains(_customer.preset.order) ) {
                _cursorOverride.cursorHoverOverride = _config.cursorHover;
                _cursorOverride.cursorHotspot = _config.cursorHoverHotspot;
            } else {
                _cursorOverride.cursorHoverOverride = _config.cursorDefault;
                _cursorOverride.cursorHotspot = _config.cursorDefaultHotspot;
            }

            if (_cursorManager._pointerInside)
                _cursorManager.SetEffectiveHoverCursor();
        }

        public void ShowOrderImage(object sender, EventArgs _) {
            orderImage.gameObject.SetActive(true);
        }
    }
}