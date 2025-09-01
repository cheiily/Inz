using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace InzGame.DisplayHandlers {
    public class SatisfactionMeterDisplay : MonoBehaviour {
        public Slider _slider;
        public GameManager _manager;

        private void Awake() {
            _manager = GameObject.FindWithTag("Manager").GetComponent<GameManager>();
            _slider = transform.GetComponentInChildren<Slider>();

            _manager.OnPointsAdded += SetOrUpdateTween;
        }


        public void SetOrUpdateTween(object _, Tuple<float, float> pointsTuple) {
            float currentPoints = pointsTuple.Item1;
            float maxPoints = pointsTuple.Item2;

            var active = DOTween.TweensByTarget(_slider);

            // DOVirtual.DelayedCall(1.25f /*~~customer particles anim length~~ idfk na oko to zmierzylem*/, () => {
                float addTime = 0;
                if ( active != null && active.Count > 0 ) {
                    addTime = active[ 0 ].Duration() - active[ 0 ].Elapsed();
                    _slider.DOKill();
                }

                _slider.DOValue(currentPoints / maxPoints, 1.0f + addTime); //todo smaller
            // });
        }

    }
}