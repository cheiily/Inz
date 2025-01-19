using System;
using System.Collections.Generic;
using Data;
using InzGame;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
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
    public float currentLevelTime;
    public List<CustomerSpawningPattern.SpawnPoint> currentLevelSpawns = new List<CustomerSpawningPattern.SpawnPoint>();

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

    public float points = 0;

    public void OnStartLevel(Button button) {
        foreach (var spawnPoint in currentLevel.customerSpawningPattern.regular_spawnPoints) {
            currentLevelSpawns.Add(new CustomerSpawningPattern.SpawnPoint(spawnPoint.timeSinceStart + Random.Range(-spawnPoint.randomVariance, spawnPoint.randomVariance), spawnPoint.customer));
        }

        var processors = GameObject.FindWithTag("Processors").GetComponentsInChildren<FoodProcessor>();
        foreach (var preset in currentLevel.availableProcessors) {
            foreach (var processor in processors) {
                if ( processor.type == preset.type ) {
                    processor.Initialize(preset);
                    break;
                }
            }
        }

        button.gameObject.SetActive(false);
        points = 0;
        gameState = GameState.PLAYING;
    }

    void Update() {
        if (gameState == GameState.PLAYING) {
            currentLevelTime += Time.deltaTime;

            List<CustomerSpawningPattern.SpawnPoint> toRemove = new List<CustomerSpawningPattern.SpawnPoint>();
            foreach (var spawn in currentLevelSpawns) {
                if ( counter.CanAdd() && currentLevelTime >= spawn.timeSinceStart ) {
                    counter.AddCustomer(spawn.customer);
                    toRemove.Add(spawn);
                }
            }
            toRemove.ForEach(elem => currentLevelSpawns.Remove(elem));
        }
    }
}
