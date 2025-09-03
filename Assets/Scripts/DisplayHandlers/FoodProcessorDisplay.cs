using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data;
using DG.Tweening;
using Misc;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using static Data.FoodProcessorPreset.FoodProcessorType;

namespace InzGame.DisplayHandlers {
    public class FoodProcessorDisplay : MonoBehaviour {
        [Header("Commons")]
        public GameConfiguration _config;
        public FoodProcessor _processor;
        public FoodProcessorPreset.FoodProcessorType _processorType;
        public CursorOverride _cursorOverride;
        public ChangeCursorOnHover _cursorManager;
        public HighlightProxy _highlightProxy;
        public GameManager _gameManager;

        [Header("Common UI")]
        public Animator processorAnimator;
        public int _processorAnimStateParam;

        [Header("Non-Diegetic UI")]
        public GameObject prop;
        public GameObject elemDisplayParent;
        public Slider progressSlider;
        public List<Image> _bufferImages;

        // ------------------------ Diegetic UI ------------------------
        [Header("Diegetic UI")]
        public GameObject diegeticElemDisplayParent;
        public List<Image> _diegeticBufferImages;
        public GameObject diegeticElemDisplayFadeExtraParent;
        public List<Image> _diegeticBufferFadeImages;
        public Image diegeticExtraImage;
        public Image diegeticExtraFadeImage;
        public Image diegeticPropImage;

        public CookingAction _currentAction;

        public Animator diegeticAnimator;
        public int _diegeticAnimStateParam;
        public Transform diegeticTweeningAnchor;

        private object _diegeticAnimatorTweenID;
        private object _processorAnimatorTweenID;
        // ------------------------------------------------------------

        private void Awake() {
            _config = GameObject.FindWithTag("Manager").GetComponent<GameManager>().config;
            _processor = GetComponent<FoodProcessor>();
            _processorAnimStateParam = Animator.StringToHash("Status");
            _diegeticAnimStateParam = Animator.StringToHash("DiegeticStatus");
            _cursorOverride = GetComponent<CursorOverride>();
            _cursorManager = GetComponent<ChangeCursorOnHover>();
            _highlightProxy = GetComponent<HighlightProxy>();
            _gameManager = GameObject.FindWithTag("Manager").GetComponent<GameManager>();

            foreach (Transform child in elemDisplayParent.transform) {
                _bufferImages.Add(child.GetComponent<Image>());
            }
            foreach (Transform child in diegeticElemDisplayParent.transform) {
                _diegeticBufferImages.Add(child.GetComponent<Image>());
            }
            foreach (Transform child in diegeticElemDisplayFadeExtraParent.transform) {
                _diegeticBufferFadeImages.Add(child.GetComponent<Image>());
            }

            _processor.OnInit += LoadProcessorParams;

            _processor.OnBufferChange += NDgUI_SetBufferImages;
            _processor.OnBufferChange += DgUI_SetBufferImages;
            _processor.OnBufferChange += AdjustCursorOverride;

            _processor.OnProgressChange += SetSliderProgress;
            _processor.OnClick += DgUI_AcceptProgress;

            _processor.OnActionChange += DgUI_SetAction;

            _processor.OnGracePeriodStateChange += AdjustCursorOverride;

            _processor.OnStatusChange += AdjustCursorOverride;
            _processor.OnStatusChange += SetAnimatorState;
            _processor.OnStatusChange += ToggleSlider;
            _processor.OnStatusChange += DgUI_HandleStateChange;

            _processor.OnMoveToMainBuffer += TweenItems;
            if ( prop != null )
                _processor.OnStatusChange += ToggleProp;
        }

        public void LoadProcessorParams(object sender, FoodProcessor processor) {
            _processorType = processor.preset.type;
            if ( _processor.playMode != LevelData.PlayMode.TIMER )
                processorAnimator.runtimeAnimatorController =
                    _config.diegeticAnimOverrides.Find(ovr => ovr.foodProcessorType == _processorType).animatorOverrideController;
            ClearImage(diegeticPropImage);
            ClearImage(diegeticExtraImage);
            ClearImage(diegeticExtraFadeImage);
            _diegeticBufferImages.ForEach(ClearImage);
            _diegeticBufferFadeImages.ForEach(ClearImage);
        }

        public void SetSliderProgress(object sender, Tuple<float, bool> progressTuple) {
            if ( _processor.playMode != LevelData.PlayMode.TIMER ) {
                progressSlider.gameObject.SetActive(false);
                return;
            }

            progressSlider.value = progressTuple.Item1;
        }

        public void NDgUI_SetBufferImages(object sender, List<Element> buffer) {
            if ( _processor.playMode != LevelData.PlayMode.TIMER )
                return;

            for (int i = 0; i < 5; ++i) {
                if ( i >= buffer.Count || buffer[ i ] is Element.NONE or Element.INVALID ) {
                    ClearImage(_bufferImages[ i ]);
                } else {
                    if ( _processor.status is FoodProcessor.Status.DONE or FoodProcessor.Status.EXPIRING ) {
                        SetImage(_bufferImages[ i ], _config.elementProperties.GetFor(buffer[ i ]).sprite_element);
                        continue;
                    }

                    var i1 = i;
                    // this is specifically bound to the item in order to easily cancel the tween when needed
                    _bufferImages[ i ].DOColor(_bufferImages[ i ].color, _config.itemJumpDuration)
                                      .From(_bufferImages[ i ].color)
                                      .OnComplete(() => SetImage(_bufferImages[ i1 ], _config.elementProperties.GetFor(buffer[ i1 ]).sprite_element));
                }
            }
        }

        public void DgUI_SetBufferImages(object sender, List<Element> buffer) {
            if (_processor.playMode == LevelData.PlayMode.TIMER)
                return;
            if ( _processor.status != FoodProcessor.Status.FREE )
                return;

            for (int i = 0; i < 5; ++i) {
                if ( i >= buffer.Count || buffer[ i ] is Element.NONE or Element.INVALID ) {
                    ClearImage(_diegeticBufferImages[ i ]);
                } else {
                    var i1 = i;
                    _diegeticBufferImages[ i ].DOColor(_diegeticBufferImages[ i ].color, _config.itemJumpDuration)
                                      .From(_diegeticBufferImages[ i ].color)
                                      .OnComplete(() => SetImage(_diegeticBufferImages[ i1 ], _config.elementProperties.GetFor(_processorType, buffer[ i1 ]).sprites[ 0 ]));
                }
            }

        }

        public void ValidateAndSetSprite(Image forDisp, Element ofElement) {
            // this happens on element entry, on action complete (result element being set) and on element exit to main buffer
            // i.e. state FREE (non-empty) or ACTIVE, EXPIRING or DONE, FREE (empty)

            Sprite sprite = null;

            if ( _processor.playMode is LevelData.PlayMode.TIMER )
                sprite = _config.elementProperties.GetFor(ofElement).sprite_element;
            // vvv diegetic vvv
            else if ( _processor.status is FoodProcessor.Status.DONE or FoodProcessor.Status.EXPIRING or FoodProcessor.Status.ACTIVE ) {
                return;
                // let state-change handle this
            } else
                sprite = _config.elementProperties.GetFor(_processorType, ofElement).sprites[ 0 ];

            SetImage(forDisp, sprite);
        }

        public void SetImage(Image display, Sprite sprite, bool transparent = false) {
            if ( sprite == null ) {
                ClearImage(display);
                return;
            }
            display.sprite = sprite;
            display.preserveAspect = true;
            if ( transparent )
                display.color = Color.clear;
            else
                display.color = Color.white;
        }

        public void ClearImage(Image forDisp) {
            forDisp.DOKill();
            forDisp.sprite = null;
            forDisp.color = Color.clear;
        }

        public void SetAnimatorState(object sender, FoodProcessor.Status state) {
            processorAnimator.SetInteger(_processorAnimStateParam, (int) state);
            diegeticAnimator.SetInteger(_diegeticAnimStateParam, (int) state);

            if ( _processor.playMode == LevelData.PlayMode.CLICKER_DIEGETIC ) {
                processorAnimator.speed = 0;
                diegeticAnimator.speed = 0;
            }
        }

        public void ToggleProp(object sender, FoodProcessor.Status state) {
            switch (state) {
                case FoodProcessor.Status.ACTIVE:
                case FoodProcessor.Status.EXPIRING:
                    prop.SetActive(false);
                    break;
                case FoodProcessor.Status.DONE:
                case FoodProcessor.Status.FREE:
                    prop.SetActive(true);
                    break;
            }
        }

        public void ToggleSlider(object sender, FoodProcessor.Status state) {
            if ( _processor.playMode != LevelData.PlayMode.TIMER ) {
                progressSlider.gameObject.SetActive(false);
                return;
            }

            Debug.Log("ToggleSlider received, state: " + state + ", currentAction null?: " +
                      (_processor.currentAction == null));

            if ( state == FoodProcessor.Status.FREE && _processor.currentAction == null ) {
                progressSlider.gameObject.SetActive(false);
                return;
            }

            progressSlider.gameObject.SetActive(true);
            var fillImg = progressSlider.fillRect.GetComponent<Image>();
            switch (state) {
                case FoodProcessor.Status.FREE:
                    fillImg.color = Color.cyan;
                    break;
                case FoodProcessor.Status.ACTIVE:
                    fillImg.color = Color.green;
                    break;
                case FoodProcessor.Status.EXPIRING:
                    fillImg.color = Color.red;
                    break;

                // don't change color if done, keep active/expired dependency
            }
        }

        public void TweenItems(List<Element> elements, List<int> indices) {
            GameObject.FindWithTag("ItemJumpTweener").GetComponent<ItemJumpTweener>()
                      .Processor(elements, indices, this);
        }

        public void AdjustCursorOverride(object sender, List<Element> elements) {
            AdjustCursorOverrideUnityEvent();
        }
        public void AdjustCursorOverride(object sender, FoodProcessor.Status status) {
            AdjustCursorOverrideUnityEvent();
        }
        public void AdjustCursorOverride(object sender, bool inGracePeriod) {
            AdjustCursorOverrideUnityEvent();
        }

        public void AdjustCursorOverrideUnityEvent(bool viaHover = false) {
            bool willSwapAction;
            List<Element> matchingIn = null;

            willSwapAction = _processor.WillSwapAction();
            matchingIn = _highlightProxy.HighlightCheck?.Invoke();


            if ( _processor.status is FoodProcessor.Status.DONE
                   || (_processor.status is FoodProcessor.Status.EXPIRING &&
                       (_gameManager.playMode != LevelData.PlayMode.CLICKER_DIEGETIC || !_processor.dcGracePeriodActive))
                   || (viaHover && _processor.status == FoodProcessor.Status.FREE && _processor._buffer.Count > 0 && willSwapAction) ) {
                _cursorOverride.cursorHoverOverride = _config.cursorGrab;
                _cursorOverride.cursorHotspot = _config.cursorGrabHotspot;
            } else if ((_processor._buffer.Count == 0 && _processor.mainBuffer.count == 0) || (!willSwapAction && (matchingIn == null || matchingIn.Count == 0))) {
                _cursorOverride.cursorHoverOverride = _config.cursorDefault;
                _cursorOverride.cursorHotspot = _config.cursorDefaultHotspot;
            } else {
                _cursorOverride.cursorHoverOverride = null;
            }

            if (_cursorManager._pointerInside)
                _cursorManager.SetEffectiveHoverCursor();
        }

        public void DgUI_SetAction(object sender, CookingAction action) {
            if ( _processor.playMode == LevelData.PlayMode.TIMER )
                return;

            _currentAction = action;

            if ( action == null ) {
                ClearImage(diegeticExtraImage);
                ClearImage(diegeticExtraFadeImage);
                return;
            }

            switch (_processorType) {
                case GARNEK: {
                    AdvanceExtraDisplayStep(0, 1, false);
                    // SetImage(diegeticExtraImage, action.spritesExtraDisplay[ 0 ]);
                    // SetImage(diegeticExtraFadeImage, action.spritesExtraDisplay[ 1 ]);
                    break;
                }
                case MISKA: {
                    SetImage(diegeticExtraImage, action.spritesExtraDisplay[ 0 ], true);
                    break;
                }
                default: break;
            }
        }

        public void DgUI_HandleStateChange(object sender, FoodProcessor.Status state) {
            if ( _processor.playMode == LevelData.PlayMode.TIMER )
                return;

            if (state is FoodProcessor.Status.FREE or FoodProcessor.Status.ACTIVE && _processorType is DESKA_DO_KROJENIA)
                ClearImage(_bufferImages[4]);

            switch (state) {
                case FoodProcessor.Status.FREE: {
                    // if the action is not null, allow SetSprites event to handle the display
                    // otherwise do a full clear
                    if ( _currentAction == null ) {
                        ClearImage(diegeticExtraImage);
                        ClearImage(diegeticExtraFadeImage);
                        for (int i = 0; i < 5; i++) {
                            ClearImage(_diegeticBufferImages[ i ]);
                            ClearImage(_diegeticBufferFadeImages[ i ]);
                        }
                    }

                    break;
                }

                case FoodProcessor.Status.ACTIVE: {
                    switch (_processorType) {
                        case MISKA: {
                            for (int i = 0; i < 5; i++) {
                                if ( i >= _currentAction.input.Count )
                                    ClearImage(_diegeticBufferImages[ i ]);
                                else
                                    SetImage(_diegeticBufferImages[ i ], _config.elementProperties.GetFor(MISKA, _currentAction.input[ i ]).sprites[ 0 ]);
                            }
                            SetImage(diegeticExtraImage, _currentAction.spritesExtraDisplay[ 0 ], true);
                            TweenMiska(GetActionStageTime());
                            break;
                        }
                        case GARNEK: {
                            // here fade water -> soup, don't change sprites assinged at SetAction
                            AdvanceDisplayStep(0, 1);
                            break;
                        }
                        case PATELNIA: {
                            AdvanceDisplayStep(0, 1);
                            break;
                        }
                        case DESKA_DO_KROJENIA: {
                            SetImage(_diegeticBufferImages[0], _config.elementProperties.GetFor(DESKA_DO_KROJENIA, _currentAction.input[0]).sprites[0]);
                            break;
                        }
                        default: break;
                    }
                    break;
                }

                case FoodProcessor.Status.EXPIRING: {
                    switch (_processorType) {
                        case MISKA: {
                            break;
                        }
                        case GARNEK: {
                            AdvanceDisplayStep(1, 2);
                            AdvanceExtraDisplayStep(1, 2);
                            break;
                        }
                        case PATELNIA: {
                            AdvanceDisplayStep(1, 2);
                            break;
                        }
                        case DESKA_DO_KROJENIA: {
                            break;
                        }
                        default: break;
                    }
                    break;
                }

                case FoodProcessor.Status.DONE: {
                    switch (_processorType) {
                        case MISKA: {
                            SetImage(diegeticExtraImage, _currentAction.spritesExtraDisplay[0]); // debug double call
                            _diegeticBufferImages.ForEach(ClearImage);
                            break;
                        }
                        case GARNEK: {
                            for (int i = 0; i < 5; i++) {
                                SetImage(_diegeticBufferImages[i], _diegeticBufferFadeImages[i].sprite);
                                ClearImage(_diegeticBufferFadeImages[i]);
                            }
                            SetImage(diegeticExtraImage, _currentAction.spritesExtraDisplay[2]);
                            ClearImage(diegeticExtraFadeImage);
                            break;
                        }
                        case PATELNIA: {
                            for (int i = 0; i < 5; i++) {
                                SetImage(_diegeticBufferImages[i], _diegeticBufferFadeImages[i].sprite);
                                ClearImage(_diegeticBufferFadeImages[i]);
                            }
                            break;
                        }
                        case DESKA_DO_KROJENIA: {
                            // jump to complete
                            ClearImage(_diegeticBufferImages[0]);
                            SetImage(_bufferImages[4], _config.elementProperties.GetFor(_currentAction.output).sprite_element);
                            break;
                        }
                        default: break;
                    }
                    break;
                }
            }
        }

        private void AdvanceDisplayStep(int iCurrentStep, int iNextStep, bool tween = true) {
            for (int i = 0; i < 5; i++) {
                if ( i >= _currentAction.input.Count ) {
                    ClearImage(_diegeticBufferImages[ i ]);
                    ClearImage(_diegeticBufferFadeImages[ i ]);
                    continue;
                }

                Sprite currentStep = _config.elementProperties.GetFor(_processorType, _currentAction.input[ i ]).sprites[ iCurrentStep ];
                Sprite nextStep = _config.elementProperties.GetFor(_processorType, _currentAction.input[ i ]).sprites[ iNextStep ];
                SetImage(_diegeticBufferImages[ i ], currentStep);
                SetImage(_diegeticBufferFadeImages[ i ], nextStep, !tween);
            }

            if ( tween )
                TweenFadeColors(GetActionStageTime());
        }

        private void AdvanceExtraDisplayStep(int iCurrentStep, int iNextStep, bool tween = true) {
            SetImage(diegeticExtraImage, _currentAction.spritesExtraDisplay[ iCurrentStep ]);
            SetImage(diegeticExtraFadeImage, _currentAction.spritesExtraDisplay[ iNextStep ], !tween);
            if ( tween )
                TweenFadeExtra(GetActionStageTime());
        }


        private float GetActionStageTime() {
            if ( _processor.currentAction == null )
                return 0;

            if ( _processor.status is FoodProcessor.Status.ACTIVE )
                return _processor.currentAction.duration;
            if ( _processor.status is FoodProcessor.Status.EXPIRING )
                return _processor.currentAction.expiryDuration;

            return 0;
        }

        private void TweenFadeExtra(float seconds) {
            if ( diegeticExtraImage.sprite != null ) {
                diegeticExtraImage.DOKill();
                diegeticExtraImage    .DOFade(0, seconds).From(1);
            }
            if ( diegeticExtraFadeImage.sprite != null ) {
                diegeticExtraFadeImage.DOKill();
                diegeticExtraFadeImage.DOFade(1, seconds).From(0);
            }
        }

        private void StopTweenFadeExtra() {
            diegeticExtraImage.DOKill();
            diegeticExtraFadeImage.DOKill();
        }

        private void TweenFadeColors(float seconds) {
            for (int i = 0; i < 5; i++) {
                if ( _diegeticBufferImages[ i ].sprite != null ) {
                    _diegeticBufferImages[ i ].DOKill();
                    _diegeticBufferImages[ i ].DOFade(0, seconds).From(1);
                }
                if ( _diegeticBufferFadeImages[ i ].sprite != null ) {
                    _diegeticBufferFadeImages[ i ].DOKill();
                    _diegeticBufferFadeImages[ i ].DOFade(1, seconds).From(0);
                }
            }
        }

        private void StopTweenFadeColors() {
            for (int i = 0; i < 5; i++) {
                _diegeticBufferImages[ i ].DOKill();
                _diegeticBufferFadeImages[ i ].DOKill();
            }
        }


        // Miska
        private void TweenMiska(float seconds) {
            for (int i = 0; i < 5; i++) {
                if ( _diegeticBufferImages[ i ].sprite != null ) {
                    _diegeticBufferImages[ i ].DOKill();
                    _diegeticBufferImages[ i ].DOFade(0, seconds).From(1);
                }
            }
            if ( diegeticExtraImage.sprite != null ) {
                diegeticExtraImage.DOKill();
                diegeticExtraImage.color = Color.white;
                diegeticExtraImage.DOFade(1, seconds).From(0);
            }
        }

        public void DDKSetCutPhase(int phase) {
            SetImage(_diegeticBufferImages[0], _config.elementProperties.GetFor(DESKA_DO_KROJENIA, _currentAction.input[0]).sprites[phase]);
        }



        public void DgUI_AcceptProgress(object sender, float progress) {
            if ( _processor.playMode != LevelData.PlayMode.CLICKER_DIEGETIC )
                return;
            Debug.Log("Clicker accepting progress: " + progress);

            float addDieg = 0, addProc = 0;
            var active = DOTween.TweensById(_diegeticAnimatorTweenID);
            if (active != null && active.Count > 0) {
                addDieg = active[0].Duration() - active[0].Elapsed();
            }
            active = DOTween.TweensById(_processorAnimatorTweenID);
            if (active != null && active.Count > 0) {
                addProc = active[0].Duration() - active[0].Elapsed();
            }

            DOTween.Kill(_diegeticAnimatorTweenID);
            DOTween.Kill(_processorAnimatorTweenID);
            _diegeticAnimatorTweenID = DOTween.To(() => diegeticAnimator.speed, (val) => diegeticAnimator.speed = val, 0, 1.0f + addDieg).From(1).id;
            _processorAnimatorTweenID = DOTween.To(() => processorAnimator.speed, (val) => processorAnimator.speed = val, 0, 1.0f + addProc).From(1).id;
        }
    }
}