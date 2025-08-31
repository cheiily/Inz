using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using DG.Tweening;
using Misc;
using UnityEngine;
using Random = UnityEngine.Random;

namespace InzGame.DisplayHandlers {
    public class ParticlePlayer : MonoBehaviour {
        public Transform particleOrigin;
        public Vector3 particleMidpointShift = new(-200, 0);
        public Vector3 particleMidpointOffsetMax = new(150, 150);
        public Transform particleDestination;

        public float particleTravelTimeSec = 0.4f;
        public Ease particleTravelEase = Ease.InCubic;

        public List<ParticleInstance> _particles;

        public GameManager _gameManager;

        public Vector3 _particleMidpoint =>
            particleOrigin.position + particleMidpointShift + Vector3.Scale(Random.insideUnitSphere, particleMidpointOffsetMax);

        private void Awake() {
            _gameManager = GameObject.FindWithTag("Manager").GetComponent<GameManager>();
            _particles = new List<ParticleInstance>(GetComponentsInChildren<ParticleInstance>(true));
            _particles.ForEach(particle => particle.OnStateChange += ParticleStateCallback);
        }

        public void Progress(ParticleInstance particle) {
            switch (particle.state) {
                case ParticleInstance.State.READY: {
                    particle.gameObject.SetActive(true);
                    particle.transform.DOMove(_particleMidpoint, particleTravelTimeSec)
                            .SetEase(particleTravelEase)
                            .OnComplete(() => particle.state = ParticleInstance.State.AWAITNG_CLICK);
                    break;
                }
                case ParticleInstance.State.AWAITNG_CLICK: {
                    // if ( _gameManager.playMode != LevelData.PlayMode.CLICKER_DIEGETIC )
                        particle.state = ParticleInstance.State.CLICKED;
                    break;
                }
                case ParticleInstance.State.CLICKED: {
                    particle.transform.DOKill();
                    particle.transform.DOMove(particleDestination.position, particleTravelTimeSec)
                            .SetEase(particleTravelEase)
                            .OnComplete(() => particle.state = ParticleInstance.State.IN_JAR);
                    break;
                }
                case ParticleInstance.State.IN_JAR:
                    particle.gameObject.SetActive(false);
                    particle.transform.position = particleOrigin.position;
                    particle.state = ParticleInstance.State.READY;
                    --_gameManager.waitingParticles;
                    break;
            }
        }

        public void PlayParticles(int numParticles, Sprite particleSprite) {
            var play = new List<ParticleInstance>();
            foreach (var particle in _particles) {
                if (play.Count < numParticles && particle.state == ParticleInstance.State.READY)
                    play.Add(particle);
            }

            foreach (var particle in play) {
                particle.SetSprite(particleSprite);
                Progress(particle);
            }

            _gameManager.waitingParticles += play.Count;
        }

        public void ParticleStateCallback(object sender, ParticleInstance.State state) {
            switch (state) {
                case ParticleInstance.State.READY: return;
                case ParticleInstance.State.AWAITNG_CLICK: {
                    if ( _gameManager.playMode != LevelData.PlayMode.CLICKER_DIEGETIC )
                        Progress(sender as ParticleInstance);
                    return;
                }
                case ParticleInstance.State.CLICKED:
                case ParticleInstance.State.IN_JAR: {
                    Progress(sender as ParticleInstance);
                    return;
                }
            }
        }

    }
}