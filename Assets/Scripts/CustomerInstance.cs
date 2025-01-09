using System.Collections.Generic;
using Data;
using InzGame;
using UnityEngine;
using UnityEngine.UI;

public class CustomerInstance : MonoBehaviour {
    public CustomerPreset preset;
    public List<float> thresholds;
    public float timeInThreshold = 0;
    public int currentThreshold = 0;

    public Sprite sprite;
    public Image _image;
    public ElementConsumer _elementConsumer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        thresholds = new List<float>();
        if (preset.ratingDropMode == CustomerPreset.RatingDropMode.REGULAR) {
            foreach (var threshold in preset.regular_ratingDropTimeThresholds) {
                float variance = Random.Range(-threshold, threshold);
                thresholds.Add(threshold + variance);
            }
        } else {
            foreach (var data in preset.random_ratingDropTimeThresholds) {
                thresholds.Add(Random.Range(data.min, data.max));
            }
        }

        // _image = GetComponent<Image>();
        _image.sprite = sprite;

        // _elementConsumer = GetComponent<ElementConsumer>();
        _elementConsumer.elements = new List<Element> {preset.order};
        _elementConsumer.OnElementsConsumed += delegate(List<Element> list) {
            if (list.Count == 1 && list[0] == preset.order)
                Destroy(gameObject);
        };
    }

    // Update is called once per frame
    void Update() {
        timeInThreshold += Time.deltaTime;
        if (timeInThreshold >= thresholds[currentThreshold]) {
            currentThreshold++;
            timeInThreshold = 0;
            if (currentThreshold >= thresholds.Count) {
                // Customer leaves
                Destroy(gameObject);
                // todo short wait to show angry face then leave
            }
        }
    }
}
