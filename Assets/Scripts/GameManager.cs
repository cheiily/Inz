using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data;
using InzGame;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour {
    public enum GameState {
        TITLE_SCREEN,
        LEVEL_SELECT,
        PLAYING,
        PAUSED,
        SUMMARY,
        CREDITS_SCREEN
    }

    public Counter counter;
    public GameConfiguration config;
    public LevelData currentLevel;
    public int currentLevelID;
    public float currentLevelTime;
    public List<CustomerSpawningPattern.SpawnPoint> currentLevelSpawns = new List<CustomerSpawningPattern.SpawnPoint>();
    public Image recipeImage;

    public GameObject summaryUI;
    public GameObject levelSelectUI;

    public event EventHandler<Tuple<float /* current */, float /* max */>> OnPointsAdded;

    public GameState _gameState;
    public GameState gameState {
        get {
            return _gameState;
        }
        set {
            _gameState = value;
            switch (value) {
                case GameState.TITLE_SCREEN:
                    break;
                case GameState.LEVEL_SELECT:
                    break;
                case GameState.PLAYING:
                    currentLevelTime = 0;
                    break;
                case GameState.PAUSED:
                    break;
                case GameState.SUMMARY:
                    break;
                case GameState.CREDITS_SCREEN:
                    break;
            }
        }
    }

    public float _points = 0;
    public List<int> _currentLevel_thresholdToAmount;
    public int _waitingCustomers = 0;

    public void StartLevel() {
        foreach (var spawnPoint in currentLevel.customerSpawningPattern.regular_spawnPoints) {
            currentLevelSpawns.Add(new CustomerSpawningPattern.SpawnPoint(spawnPoint.timeSinceStart + Random.Range(-spawnPoint.randomVariance, spawnPoint.randomVariance), spawnPoint.customer));
        }

        var processors = GameObject.FindWithTag("Processors").GetComponentsInChildren<FoodProcessor>();
        var levelPresets = currentLevel.availableProcessors.Select(preset => preset.type).ToList();
        foreach (var processor in processors) {
            var idx = 0;
            if ( (idx = levelPresets.IndexOf(processor.type)) != -1 )
                processor.Initialize(currentLevel.availableProcessors[idx]);
            else processor.gameObject.SetActive(false);
        }

        var inputs = LinqUtility.ToHashSet(currentLevel.availableProcessors.SelectMany(preset => preset.actions.SelectMany(action => action.GetInputSet())));
        var sources = GameObject.FindWithTag("Sources").GetComponentsInChildren<ElementSource>();

        foreach (var source in sources) {
            if ( !inputs.Contains(source.element) )
                source.gameObject.SetActive(false);
        }

        recipeImage.sprite = currentLevel.recipeBook;
        recipeImage.SetNativeSize();

        // button.gameObject.SetActive(false);
        _points = 0;
        _currentLevel_thresholdToAmount = new List<int>(4) {0, 0, 0, 0};
        gameState = GameState.PLAYING;
    }

    private void Awake() {
        gameState = GameState.LEVEL_SELECT;
        levelSelectUI.SetActive(true);
    }

    void Update() {
        if (gameState == GameState.PLAYING) {
            currentLevelTime += Time.deltaTime;

            _waitingCustomers = 0;
            List<CustomerSpawningPattern.SpawnPoint> toRemove = new List<CustomerSpawningPattern.SpawnPoint>();
            foreach (var spawn in currentLevelSpawns) {
                if ( counter.CanAdd() && currentLevelTime >= spawn.timeSinceStart ) {
                    counter.AddCustomer(spawn.customer);
                    toRemove.Add(spawn);
                } else if ( currentLevelTime >= spawn.timeSinceStart ) {
                    _waitingCustomers++;
                }
            }
            toRemove.ForEach(elem => currentLevelSpawns.Remove(elem));

            if ( currentLevelSpawns.Count == 0 && counter.numCustomers == 0 ) {
                StartCoroutine(WaitThenOpenSummary());
            }
        }
    }

    public void AddPoints(float pointsToAdd) {
        _points += pointsToAdd;
        OnPointsAdded?.Invoke(this, new Tuple<float, float>(_points, currentLevel.customerSpawningPattern.regular_spawnPoints.Count * 100));
    }

    public void LogCustomer(CustomerInstance customer) {
        _points += CustomerEvaluation.Invoke(config.evaluationMethod, customer);
        _currentLevel_thresholdToAmount[customer.currentThreshold]++;

        OnPointsAdded?.Invoke(this, new Tuple<float, float>(_points, currentLevel.customerSpawningPattern.regular_spawnPoints.Count * 100));
        // Debug.Log("Customer logged: " + customer);
    }

    public void ExitGame() {
        // todo log?
        Application.Quit();
    }


    public IEnumerator WaitThenOpenSummary() {
        gameState = GameState.PAUSED;
        yield return new WaitForSeconds(1);
        gameState = GameState.SUMMARY;
        summaryUI.SetActive(true);
        summaryUI.GetComponent<SummaryUIManager>().SetFor(_points, currentLevel.customerSpawningPattern.regular_spawnPoints.Count * 100, _currentLevel_thresholdToAmount);
        float scorePercentage = _points / (currentLevel.customerSpawningPattern.regular_spawnPoints.Count * 100);
        int scoreThreshold =
            scorePercentage >= config.totalMoodThresholds[0] ? 0
            : scorePercentage >= config.totalMoodThresholds[1] ? 1
            : scorePercentage >= config.totalMoodThresholds[2] ? 2
            : 3;
        levelSelectUI.GetComponent<LevelSelectUIManager>().SetDisplaysFor(currentLevelID, _points, currentLevel.customerSpawningPattern.regular_spawnPoints.Count * 100, scoreThreshold);
    }
}
