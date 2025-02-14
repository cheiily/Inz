using System;
using Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InzGame {
    public class LevelSelectUIManager : MonoBehaviour {
        public Button button1;
        public Button button2;
        public GameObject finishedLabel1;
        public GameObject finishedLabel2;

        public GameManager gameManager;

        public Image lvl1Buzka;
        public TextMeshProUGUI lvl1Wynik;
        public Image lvl2Buzka;
        public TextMeshProUGUI lvl2Wynik;

        private void Awake() {
            gameManager = GameObject.FindWithTag("Manager").GetComponent<GameManager>();

            lvl1Wynik.text = $"0 / {gameManager.config.level1.customerSpawningPattern.regular_spawnPoints.Count * 100}";
            lvl2Wynik.text = $"0 / {gameManager.config.level2.customerSpawningPattern.regular_spawnPoints.Count * 100}";
        }

        public void SetDisplaysFor(int level, float pts, float ptsMax, int threshold) {
            Image buzka = level == 1 ? lvl1Buzka : lvl2Buzka;
            TextMeshProUGUI wynik = level == 1 ? lvl1Wynik : lvl2Wynik;

            buzka.sprite = threshold == 3 ? gameManager.config.buzkaPusta : gameManager.config.totalMoodSprites[threshold];
            wynik.text = $"{pts:0.} / {ptsMax}";
        }

        public void LoadLevel(int level) {
            LevelData levelData;
            if ( level == 1 )
                levelData = gameManager.config.level1;
            else if ( level == 2 )
                levelData = gameManager.config.level2;
            else
                return;

            var buttonPressed = level == 1 ? button1 : button2;
            buttonPressed.interactable = false;
            var finishedLabel = level == 1 ? finishedLabel1 : finishedLabel2;
            finishedLabel.SetActive(true);

            gameManager.currentLevel = levelData;
            gameManager.currentLevelID = level;
            gameManager.StartLevel();
            gameObject.SetActive(false);
        }

    }
}