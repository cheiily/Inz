using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace InzGame.DisplayHandlers {
    public class CustomerDisplay : MonoBehaviour {
        public Image personImage;
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

        public Animator _particleAnimator;
        public List<Image> _particleImages;
        public int _particleParam;

        private void Awake() {
            _customer = GetComponent<CustomerInstance>();
            _slider = transform.GetComponentInChildren<Slider>();
            _animator = transform.parent.GetComponent<Animator>();
            _thresholdParam = Animator.StringToHash("Threshold");
            _config = GameObject.FindWithTag("Manager").GetComponent<GameManager>().config;

            _particleAnimator = GameObject.FindWithTag("CustomerParticles").transform.GetChild(_customer.seat).GetComponent<Animator>();
            _particleImages = _particleAnimator.gameObject.GetComponentsInChildren<Image>(true).ToList();
            _particleParam = Animator.StringToHash("PlayParticles");

            _customer.BeforeCustomerRemove += PlayParticles;
            _customer.BeforeCustomerRemove += PlayCustomerLeave;

            _customer.OnPatienceChange += SetSliderValue;
            _customer.OnPatienceChange += SetAnimatorState;
            _customer.OnPatienceChange += SetPersonImage;
        }

        public void SetSliderValue(object sender, Tuple<float, int> patienceTuple) {
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

        public void PlayCustomerLeave(object sender, EventArgs _) {
            // _animator.SetInteger(_thresholdParam, 3);
            _animator.Play("Leave");
        }

        public void PlayParticles(object sender, EventArgs _) {
            _particleImages.ForEach(image => image.sprite = _config.moodSprites[_customer.currentThreshold]);
            _particleAnimator.gameObject.SetActive(true);
            // _particleAnimator.Play("Particles");
            _particleAnimator.SetTrigger(_particleParam);
            DOVirtual.DelayedCall(1.75f, () => _particleAnimator.gameObject.SetActive(false));
        }
    }
}