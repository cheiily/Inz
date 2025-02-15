using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InzGame {
    public class PrelevelUIManager : MonoBehaviour {
        public GameObject levelSelectUI;
        public TextMeshProUGUI numClientsText;
        public Image recipeBookImg;

        public Transform posOutside;
        public Transform posInside;

        public GameManager _manager;

        private void Awake() {
            _manager = GameObject.FindWithTag("Manager").GetComponent<GameManager>();
        }

        public void SetFor(int numClients, Sprite recipeBook) {
            levelSelectUI.transform.position = posInside.position;
            numClientsText.text = $"Klientów: {numClients}";
            recipeBookImg.sprite = recipeBook;
        }

        public void OnClick() {
            levelSelectUI.transform.DOMove(posOutside.position, 0.5f).From(posInside.position)
                         .OnComplete(() => {
                             gameObject.SetActive(false);
                             _manager.gameState = GameManager.GameState.PLAYING;
                         });

        }
    }
}