using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SummaryUIManager : MonoBehaviour {
    public Transform posOutside;
    public Transform posInside;

    public List<Image> buzkiImages;
    public TextMeshProUGUI pointsText;
    public List<TextMeshProUGUI> amountsTextsInThresholdOrder;

    public GameObject lvlSelectUI;

    public GameConfiguration _config;

    private void Awake() {
        _config = GameObject.FindWithTag("Manager").GetComponent<GameManager>().config;
    }

    public void OnEnable() {
        transform.DOMove(posInside.position, 0.5f).From(posOutside.position);
    }

    public void SetFor(float pts, float maxPts, List<int> amounts) {
        var pctg = pts / maxPts;
        for (int i = 0; i < _config.totalMoodThresholds.Count; i++) {
            if (pctg >= _config.totalMoodThresholds[i]) {
                buzkiImages[i].sprite = _config.totalMoodSprites[i];
            }
        }

        pointsText.text = $"{pts:.} / {maxPts}";

        for (int i = 0; i < amountsTextsInThresholdOrder.Count; i++) {
            amountsTextsInThresholdOrder[i].text = i >= amounts.Count ? "0" : amounts[i].ToString();
        }
    }

    public void GoToMenu() {
        lvlSelectUI.SetActive(true);
        gameObject.SetActive(false);
    }
}