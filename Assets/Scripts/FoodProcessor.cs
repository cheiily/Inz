using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using DG.Tweening;
using InzGame;
using Unity.VisualScripting;
using UnityEngine;
using Buffer = InzGame.Buffer;

public class FoodProcessor : MonoBehaviour {

    public event EventHandler<float> OnClick;
    public event EventHandler<Tuple<float, bool>> OnProgressChange;
    public event EventHandler<List<Element>> OnBufferChange;
    public event EventHandler<Status> OnStatusChange;
    public event EventHandler<CookingAction> OnActionChange;
    public event Action<List<Element> /*elements*/, List<int> /*indices*/> OnMoveToMainBuffer;
    public event EventHandler<FoodProcessor> OnInit;
    public event EventHandler<bool> OnGracePeriodStateChange;

    public enum Status {
        FREE,
        ACTIVE,
        EXPIRING,
        DONE
    }
    public Status _status;

    public Status status {
        get {
            return _status;
        }
        set {
            _status = value;
            switch (value) {
                case Status.FREE:
                    _consumer.requirementMode = ElementConsumer.REQUIREMENT_MODE.CUSTOM;
                    break;
                case Status.ACTIVE:
                    progress = 0;
                    if (playMode != LevelData.PlayMode.CLICKER_DIEGETIC)
                        _consumer.requirementMode = ElementConsumer.REQUIREMENT_MODE.FAIL;
                    else
                        _consumer.requirementMode = ElementConsumer.REQUIREMENT_MODE.CUSTOM;
                    _buffer.Clear();
                    break;
                case Status.EXPIRING:
                    progress = 0;
                    _consumer.requirementMode = ElementConsumer.REQUIREMENT_MODE.CUSTOM;
                    break;
                case Status.DONE:
                    _consumer.requirementMode = ElementConsumer.REQUIREMENT_MODE.CUSTOM;
                    break;
            }
            OnStatusChange?.Invoke(this, value);
        }
    }

    public FoodProcessorPreset.FoodProcessorType type;

    public GameConfiguration config;
    public LevelData.PlayMode playMode;
    public Buffer mainBuffer;
    public FoodProcessorPreset preset;

    private CookingAction _currentAction;
    public CookingAction currentAction {
        get => _currentAction;
        set {
            _currentAction = value;
            OnActionChange?.Invoke(this, value);
        }
    }

    public float progress;
    public float progressSecPerTap = 1;
    public float dgGracePeriodSec = 2;
    public bool dcGracePeriodActive = false;

    public List<Element> _buffer;
    public ElementConsumer _consumer;
    public List<Element> moveToMainBuffer = new List<Element>();
    public List<int> moveToMainBufferIndices = new List<int>();

    public bool _initialized = false;

    void Start() {
        mainBuffer = GameObject.FindWithTag("Manager").GetComponent<Buffer>();
        config = GameObject.FindWithTag("Manager").GetComponent<GameManager>().config;
        _consumer = GetComponent<ElementConsumer>();
    }

    private FoodProcessorPreset getPreset() {
        return preset;
    }

    public void Initialize(FoodProcessorPreset preset) {
        playMode = GameObject.FindWithTag("Manager").GetComponent<GameManager>().currentLevel.playMode;

        this.preset = preset;
        _consumer.elements = preset.actions.SelectMany(action => action.input).ToList();

        if ( !_initialized ) {
            var highlightProxy = GetComponent<HighlightProxy>();
            if ( highlightProxy != null ) {
                highlightProxy.HighlightCheck = HighlightCheck;
            }

            _consumer.CustomRequirementCheck += delegate(List<Element> required, List<Element> bufferState) {
                if ( status == Status.ACTIVE ) { // only comes here if it's a clicker
                    progress += progressSecPerTap;
                    OnClick?.Invoke(this, progress / currentAction.duration);
                    return new List<Element>();
                }

                if ( status == Status.EXPIRING && playMode == LevelData.PlayMode.CLICKER_DIEGETIC && dcGracePeriodActive ) {
                    progress += progressSecPerTap;
                    OnClick?.Invoke(this, progress / currentAction.expiryDuration);
                    return new List<Element>();
                }

                if ( status == Status.DONE || status == Status.EXPIRING ) {
                    moveToMainBuffer.Add(_buffer[ 0 ]); // change to action.output if errors?
                    moveToMainBufferIndices.Add(0);
                    _buffer.RemoveAt(0);
                    currentAction = null;
                    status = Status.FREE;

                    progress = 0;
                    OnProgressChange?.Invoke(this, new Tuple<float, bool>(progress, false));
                    OnBufferChange?.Invoke(this, _buffer);
                }

                if ( Buffer.Count(bufferState) == 0 )
                    return new List<Element>();

                CookingAction maxCompletionAction = currentAction;
                HashSet<Element> maxCompletionElements = new HashSet<Element>();
                HashSet<Element> matchingIn = new HashSet<Element>();
                HashSet<Element>
                    matchingTotal =
                        new HashSet<Element>(); // buffer + matchingIn, kept separate for easy return of only contained elements
                float maxCompletion = 0;
                foreach (var action in this.preset.actions) {
                    matchingIn = new HashSet<Element>(bufferState.FindAll(element =>
                        action.GetInputSet().Contains(element) && !_buffer.Contains(element)));
                    matchingTotal = new HashSet<Element>(matchingIn);
                    matchingTotal.AddRange(_buffer.FindAll(element => action.GetInputSet().Contains(element)));
                    float completion = (float) matchingTotal.Count / action.input.Count;
                    if ( completion > maxCompletion || (maxCompletionAction != null &&
                                                        Math.Abs(completion - maxCompletion) < 0.00001 &&
                                                        config.elementProperties.GetFor(action.output).level >
                                                        config.elementProperties.GetFor(maxCompletionAction.output)
                                                              .level) ) {
                        maxCompletion = completion;
                        maxCompletionAction = action;
                        maxCompletionElements = matchingIn;
                    }
                }

                if ( _buffer.Count > 0 && currentAction != maxCompletionAction ) {
                    moveToMainBuffer.AddRange(_buffer.FindAll(element => !maxCompletionAction.input.Contains(element)));
                    moveToMainBufferIndices.AddRange(_buffer
                                                     .FindAll(element => !maxCompletionAction.input.Contains(element))
                                                     .Select(element => _buffer.IndexOf(element)));
                    _buffer.RemoveAll(element => !maxCompletionAction.input.Contains(element));
                }

                currentAction = maxCompletionAction;
                return new List<Element>(maxCompletionElements);
            };
            _consumer.OnElementsConsumed += delegate(List<Element> consumed) {
                _buffer.AddRange(consumed);
                if ( currentAction != null && currentAction.input.All(element => _buffer.Contains(element)) ) {
                    status = Status.ACTIVE;
                    _buffer.Clear();
                }

                if ( currentAction != null && status == Status.FREE ) {
                    status = Status.FREE; // toggle display refresh to show progress slider
                    progress = (float) _buffer.Count / currentAction.GetInputSet().Count;
                }

                OnBufferChange?.Invoke(this, _buffer);
                OnProgressChange?.Invoke(this, new Tuple<float, bool>(progress, false));

                foreach (var element in moveToMainBuffer) {
                    mainBuffer.Submit(element);
                }

                OnMoveToMainBuffer?.Invoke(moveToMainBuffer, moveToMainBufferIndices);
                moveToMainBuffer.Clear();
                moveToMainBufferIndices.Clear();
            };

            _initialized = true;
        }

        OnInit?.Invoke(this, this);
    }

    // Update is called once per frame
    void Update() {
        if (status is Status.ACTIVE or Status.EXPIRING) {
            if ( playMode != LevelData.PlayMode.CLICKER_DIEGETIC )
                progress += Time.deltaTime;
            if (status == Status.ACTIVE && progress >= currentAction.duration) {
                if ( playMode != LevelData.PlayMode.CLICKER_DIEGETIC ) {
                    status = currentAction.expiryDuration > 0 ? Status.EXPIRING : Status.DONE;
                } else {
                    status = currentAction.expiryDuration > 0 ? Status.EXPIRING : Status.DONE;
                    dcGracePeriodActive = true;
                    OnGracePeriodStateChange?.Invoke(this, dcGracePeriodActive);
                    DOVirtual.DelayedCall(dgGracePeriodSec, () => {
                        dcGracePeriodActive = false;
                        OnGracePeriodStateChange?.Invoke(this, dcGracePeriodActive);
                    });
                    // status = Status.DONE;
                }
                _buffer.Add(currentAction.output);
                OnBufferChange?.Invoke(this, _buffer);
            } else if (status == Status.EXPIRING && progress >= currentAction.expiryDuration) {
                status = Status.DONE;
                _buffer[ 0 ] = Element.SPALONE;
                OnBufferChange?.Invoke(this, _buffer);
            }
            OnProgressChange?.Invoke(this, new Tuple<float, bool>(progress / (status == Status.EXPIRING ? currentAction.expiryDuration : currentAction.duration), status == Status.EXPIRING));
        }
    }

    public void Clear() {
        _buffer.Clear();
        OnBufferChange?.Invoke(this, _buffer);
    }

    public List<Element> HighlightCheck() {
        CookingAction maxCompletionAction = currentAction;
        HashSet<Element> maxCompletionElements = new HashSet<Element>();
        HashSet<Element> matchingIn = new HashSet<Element>();
        HashSet<Element> matchingTotal = new HashSet<Element>(); // buffer + matchingIn, kept separate for easy return of only contained elements
        float maxCompletion = 0;
        foreach ( var action in preset.actions ) {
            matchingIn = new HashSet<Element>(mainBuffer.buffer.FindAll(element => action.GetInputSet().Contains(element) && !_buffer.Contains(element)));
            matchingTotal = new HashSet<Element>(matchingIn);
            matchingTotal.AddRange(_buffer.FindAll(element => action.GetInputSet().Contains(element)));
            float completion = (float) matchingTotal.Count / action.input.Count;
            if ( completion > maxCompletion || (maxCompletionAction != null && Math.Abs(completion - maxCompletion) < 0.00001 && config.elementProperties.GetFor(action.output).level > config.elementProperties.GetFor(maxCompletionAction.output).level) ) {
                maxCompletion = completion;
                maxCompletionAction = action;
                maxCompletionElements = matchingIn;
            }
        }

        return maxCompletionElements.ToList();
    }

    public bool WillSwapAction() {
        CookingAction maxCompletionAction = currentAction;
        HashSet<Element> matchingIn = new HashSet<Element>();
        HashSet<Element> matchingTotal = new HashSet<Element>(); // buffer + matchingIn, kept separate for easy return of only contained elements
        float maxCompletion = 0;
        foreach ( var action in preset.actions ) {
            matchingIn = new HashSet<Element>(mainBuffer.buffer.FindAll(element => action.GetInputSet().Contains(element) && !_buffer.Contains(element)));
            matchingTotal = new HashSet<Element>(matchingIn);
            matchingTotal.AddRange(_buffer.FindAll(element => action.GetInputSet().Contains(element)));
            float completion = (float) matchingTotal.Count / action.input.Count;
            if ( completion > maxCompletion || (maxCompletionAction != null && Math.Abs(completion - maxCompletion) < 0.00001 && config.elementProperties.GetFor(action.output).level > config.elementProperties.GetFor(maxCompletionAction.output).level) ) {
                maxCompletion = completion;
                maxCompletionAction = action;
            }
        }

        return currentAction != maxCompletionAction;
    }
}
