using System;
using UnityEngine;
using UnityEngine.UI;

namespace InzGame.DisplayHandlers {
    public class CustomerDisplay : MonoBehaviour {
        public Slider _slider;
        public CustomerInstance _customer;
        private Color[] colors = new Color[] {
            Color.green, Color.yellow, Color.red, Color.clear
        };

        private void Awake() {
            _customer = GetComponent<CustomerInstance>();
            _slider = transform.GetComponentInChildren<Slider>();

            _customer.OnPatienceChange += SetSliderValue;
        }

        public void SetSliderValue(object sender, Tuple<float, int> patienceTuple) {
            _slider.value = patienceTuple.Item1;
            _slider.fillRect.GetComponent<Image>().color = colors[ patienceTuple.Item2 ];
        }
    }
}