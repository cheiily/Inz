using System;
using System.Collections;
using System.Collections.Generic;
using Data;
using InzGame;
using UnityEngine;
using UnityEngine.Serialization;
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
    public bool doCountdown = true;

    public ElementConsumer _elementConsumer;
    public GameConfiguration _config;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        _config = GameObject.FindWithTag("Manager").GetComponent<GameManager>().config;

        CustomerRemovePolicy += PolicyModification_Delay(Policy_AddPointsThenDestroy, _config.itemJumpDuration);

        thresholds = new List<float>();
        if (preset.ratingDropMode == CustomerPreset.RatingDropMode.REGULAR) {
            foreach (var threshold in preset.regular_ratingDropTimeThresholds) {
                thresholds.Add(threshold);
                // float variance = Random.Range(-threshold, threshold);
                // thresholds.Add(threshold + variance);
            }
        } else {
            foreach (var data in preset.random_ratingDropTimeThresholds) {
                thresholds.Add(Random.Range(data.min, data.max));
            }
        }

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

        if (doCountdown)
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
        // manager.AddPoints(points);
        manager.LogCustomer(this);
        Debug.Log("Adding points: " + points);
        Destroy(gameObject);
    }

    public EventHandler PolicyModification_Delay(EventHandler policy, float seconds) {
        return (sender, args) => {
            doCountdown = false;
            StartCoroutine(PolicyExecutor_Delay(policy, sender, args, seconds));
        };
    }

    private IEnumerator PolicyExecutor_Delay(EventHandler policy, object sender, EventArgs args, float seconds) {
        yield return new WaitForSeconds(seconds);
        policy(sender, args);
    }
}
