using System;
using System.Collections.Generic;
using Data;
using InzGame;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class CustomerInstance : MonoBehaviour {
    public event EventHandler<Tuple<float, int>> OnPatienceChange;
    public event EventHandler OnPatienceEnd;
    public event EventHandler OnCustomerRemove;
    private event EventHandler CustomerRemovePolicy;

    public CustomerPreset preset;
    public List<float> thresholds;
    public float timeInThreshold = 0;
    public int currentThreshold = 0;

    public Sprite sprite;
    public Image _image;
    public ElementConsumer _elementConsumer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        CustomerRemovePolicy += Policy_AddPointsThenDestroy;

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
            if (list.Count == 1 && list[0] == preset.order) {
                CustomerRemovePolicy?.Invoke(this, EventArgs.Empty);
            }
        };
    }

    // Update is called once per frame
    void Update() {
        bool do_destroy = false;

        timeInThreshold += Time.deltaTime;
        if (currentThreshold < thresholds.Count && timeInThreshold >= thresholds[currentThreshold]) {
            currentThreshold++;
            timeInThreshold = 0;
            if (currentThreshold >= thresholds.Count) {
                do_destroy = true;
            }
        }

        OnPatienceChange?.Invoke(this, new Tuple<float, int>(
            currentThreshold >= thresholds.Count ? 0 : 1 - timeInThreshold / thresholds[currentThreshold],
            currentThreshold
        ));
        if (do_destroy) {
            Debug.Log("Patience ended");
            OnPatienceEnd?.Invoke(this, EventArgs.Empty);
            CustomerRemovePolicy?.Invoke(this, EventArgs.Empty);
        }
    }

    public void Policy_AddPointsThenDestroy(object sender, EventArgs _) {
        OnCustomerRemove?.Invoke(this, EventArgs.Empty);
        var manager = GameObject.FindWithTag("Manager").GetComponent<GameManager>();
        var points = CustomerEvaluation.methods[ manager.config.evaluationMethod ](this);
        manager.points += points;
        Debug.Log("Adding points: " + points);
        Destroy(gameObject);
    }
}
