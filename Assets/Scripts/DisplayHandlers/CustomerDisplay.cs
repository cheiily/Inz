using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace InzGame.DisplayHandlers {
    public class CustomerDisplay : MonoBehaviour {
        public Slider _slider;
        public CustomerInstance _customer;
        public Animator _animator;
        public int _thresholdParam;
        private Color[] colors = new Color[] {
            Color.green, Color.yellow, Color.red, Color.clear
        };

        private void Awake() {
            _customer = GetComponent<CustomerInstance>();
            _slider = transform.GetComponentInChildren<Slider>();
            _animator = transform.parent.GetComponent<Animator>();
            _thresholdParam = Animator.StringToHash("Threshold");

            _customer.OnPatienceChange += SetSliderValue;
            _customer.OnPatienceChange += SetAnimatorState;
        }

        public void SetSliderValue(object sender, Tuple<float, int> patienceTuple) {
            _slider.value = patienceTuple.Item1;
            _slider.fillRect.GetComponent<Image>().color = colors[ patienceTuple.Item2 ];
        }

        public void SetAnimatorState(object sender, Tuple<float, int> patienceTuple) {
            _animator.SetInteger(_thresholdParam, patienceTuple.Item2);
        }
    }
}