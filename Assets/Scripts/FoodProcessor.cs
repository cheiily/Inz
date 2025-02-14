using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using InzGame;
using Unity.VisualScripting;
using UnityEngine;
using Buffer = InzGame.Buffer;

public class FoodProcessor : MonoBehaviour {

    public event EventHandler<Tuple<float, bool>> OnProgressChange;
    public event EventHandler<List<Element>> OnBufferChange;
    public event EventHandler<Status> OnStatusChange;
    public event Action<List<Element> /*elements*/, List<int> /*indices*/> OnMoveToMainBuffer;

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
                    _consumer.requirementMode = ElementConsumer.REQUIREMENT_MODE.FAIL;
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
    public Buffer mainBuffer;
    public FoodProcessorPreset preset;
    public CookingAction currentAction;
    public float progress;

    public List<Element> _buffer;
    public ElementConsumer _consumer;
    public List<Element> moveToMainBuffer = new List<Element>();
    public List<int> moveToMainBufferIndices = new List<int>();

    void Start() {
        mainBuffer = GameObject.FindWithTag("Manager").GetComponent<Buffer>();
        config = GameObject.FindWithTag("Manager").GetComponent<GameManager>().config;
        _consumer = GetComponent<ElementConsumer>();
    }

    public void Initialize(FoodProcessorPreset preset) {
        this.preset = preset;

        _consumer.elements = preset.actions.SelectMany(action => action.input).ToList();
        _consumer.CustomRequirementCheck += delegate(List<Element> required, List<Element> bufferState) {
            if ( status == Status.DONE || status == Status.EXPIRING ) {
                moveToMainBuffer.Add(_buffer[0]); // change to action.output if errors?
                moveToMainBufferIndices.Add(0);
                _buffer.RemoveAt(0);
                status = Status.FREE;
                currentAction = null;

                progress = 0;
                OnProgressChange?.Invoke(this, new Tuple<float, bool>(progress, false));
                OnBufferChange?.Invoke(this, _buffer);
            }

            if (Buffer.Count(bufferState) == 0)
                return new List<Element>();

            // if ( currentAction != null ) {
            //     HashSet<Element> actionIn = currentAction.GetInputSet();
            //     HashSet<Element> compare = new HashSet<Element>(_buffer);
            //     List<Element> matchingInput = new List<Element>(bufferState.FindAll(element => actionIn.Contains(element)));
            //     compare.AddRange(matchingInput);
            //
            //     if ( actionIn == compare ) {
            //         return matchingInput;
            //     }
            // }

            CookingAction maxCompletionAction = currentAction;
            HashSet<Element> maxCompletionElements = new HashSet<Element>();
            HashSet<Element> matchingIn = new HashSet<Element>();
            HashSet<Element> matchingTotal = new HashSet<Element>(); // buffer + matchingIn, kept separate for easy return of only contained elements
            float maxCompletion = 0;
            foreach ( var action in preset.actions ) {
                matchingIn = new HashSet<Element>(bufferState.FindAll(element => action.GetInputSet().Contains(element) && !_buffer.Contains(element)));
                matchingTotal = new HashSet<Element>(matchingIn);
                matchingTotal.AddRange(_buffer.FindAll(element => action.GetInputSet().Contains(element)));
                float completion = (float) matchingTotal.Count / action.input.Count;
                if ( completion > maxCompletion || (maxCompletionAction != null && completion - maxCompletion < 0.00001 && config.elementProperties.GetFor(action.output).level > config.elementProperties.GetFor(maxCompletionAction.output).level) ) {
                    maxCompletion = completion;
                    maxCompletionAction = action;
                    maxCompletionElements = matchingIn;
                }
            }

            if ( _buffer.Count > 0 && currentAction != maxCompletionAction ) {
                moveToMainBuffer.AddRange(_buffer.FindAll(element => !maxCompletionAction.input.Contains(element)));
                moveToMainBufferIndices.AddRange(_buffer.FindAll(element => !maxCompletionAction.input.Contains(element)).Select(element => _buffer.IndexOf(element)));
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
                progress = (float)_buffer.Count / currentAction.GetInputSet().Count;
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
    }

    // Update is called once per frame
    void Update() {
        if (status is Status.ACTIVE or Status.EXPIRING) {
            progress += Time.deltaTime;
            if (status == Status.ACTIVE && progress >= currentAction.duration) {
                status = currentAction.expiryDuration > 0 ? Status.EXPIRING : Status.DONE;
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

    public void HighlightCheck() {

    }
}
