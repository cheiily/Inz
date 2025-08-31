using System;
using Data;
using UnityEngine;
using UnityEngine.UI;

namespace InzGame.DisplayHandlers {
    public class ParticleInstance : MonoBehaviour {
        public enum State {
            READY,
            AWAITNG_CLICK,
            CLICKED,
            IN_JAR
        }


        public event EventHandler<State> OnStateChange;

        public State _state;
        public State state {
            get => _state;
            set {
                if (_state != value) {
                    _state = value;
                    OnStateChange?.Invoke(this, _state);
                }
            }
        }

        public Image _image;

        public GameManager _gameManager;

        private void Awake() {
            _gameManager = GameObject.FindWithTag("Manager").GetComponent<GameManager>();
            // possible due to separate builds & late awake
            if ( _gameManager.playMode != LevelData.PlayMode.CLICKER_DIEGETIC )
                GetComponent<Button>().enabled = false;
        }

        public void OnClickProgressAnimation() {
            if ( state == State.AWAITNG_CLICK )
                state = State.CLICKED;
        }

        public void SetSprite(Sprite sprite) {
            if ( _image == null )
                _image = GetComponent<Image>();
            _image.sprite = sprite;
        }
    }
}