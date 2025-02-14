using System;
using System.Collections.Generic;
using System.Linq;
using InzGame;
using InzGame.DisplayHandlers;
using UnityEngine;
using Buffer = InzGame.Buffer;

public class ElementConsumer : MonoBehaviour
{
    public enum REQUIREMENT_MODE {
        ONE,
        ALL,
        NONE,
        FAIL,
        CUSTOM
    }
    public enum CONSUMPTION_MODE {
        KEEP,
        REMOVE,
        CUSTOM
    }
    public enum CONSUMPTION_RESULT {
        KEEP,
        REMOVE
    }

    public List<Element> elements;

    public REQUIREMENT_MODE requirementMode;
    public CONSUMPTION_MODE consumptionMode;

    public event Func<List<Element> /*required*/, List<Element> /*buffer state*/, List<Element> /*RETURN: accepted elements, contained by the buffer*/> CustomRequirementCheck;
    public event Func<Element, CONSUMPTION_RESULT> CustomConsumption;
    public event Action<List<Element>> OnElementsConsumed;
    public event Action<List<Element>> BeforeElementsConsumed;
    public HighlightProxy highlightProxy;

    public static Buffer _buffer;

    private void Awake() {
        if (_buffer == null)
            _buffer = GameObject.FindWithTag("Manager").GetComponent<Buffer>();

        BeforeElementsConsumed += TweenJumpItems;

        highlightProxy = gameObject.GetComponent<HighlightProxy>();
        if (GetComponent<FoodProcessor>() == null) {
            if (elements.Count == 1 && elements[0] == Element.ALL && requirementMode == REQUIREMENT_MODE.ALL)
                highlightProxy.HighlightCheck = TrashHighlightCheck;
            else highlightProxy.HighlightCheck = HighlightCheck;
        }

        OnElementsConsumed += _ => {
            highlightProxy.DoStopHighlight();
        };
    }

    public void AcceptBuffer() {
        if ( requirementMode == REQUIREMENT_MODE.FAIL )
            return;

        List<Element> elementsToRemove = new List<Element>();
        if ( requirementMode == REQUIREMENT_MODE.ALL ) {
            if (!elements.All(elem => _buffer.Contains(elem)))
                return;

            elementsToRemove = elements;
        } else if ( requirementMode == REQUIREMENT_MODE.ONE ) {
            elementsToRemove = elements.FindAll(element => _buffer.Contains(element));
        } else if ( requirementMode == REQUIREMENT_MODE.CUSTOM ) {
            elementsToRemove = CustomRequirementCheck?.Invoke(elements, _buffer.buffer);
        } else if ( requirementMode == REQUIREMENT_MODE.NONE ) {
            elementsToRemove = elements;
        }

        BeforeElementsConsumed?.Invoke(elementsToRemove);
        foreach (var element in elementsToRemove) {
            _buffer.Remove(element);
            var result = consumptionMode == CONSUMPTION_MODE.CUSTOM ? CustomConsumption?.Invoke(element) : (CONSUMPTION_RESULT) consumptionMode;
            if ( result == CONSUMPTION_RESULT.REMOVE )
                elements.Remove(element);
        }
        OnElementsConsumed?.Invoke(elementsToRemove);
    }

    public void TweenJumpItems(List<Element> elements) {
        GameObject.FindWithTag("ItemJumpTweener").GetComponent<ItemJumpTweener>().Consumer(this, elements);
    }

    public List<Element> HighlightCheck() {
        List<Element> ret = new List<Element>();

        if ( requirementMode == REQUIREMENT_MODE.FAIL )
            return ret;

        if ( requirementMode == REQUIREMENT_MODE.ALL && elements.All(elem => _buffer.Contains(elem)) ){
            ret = elements;
        } else if ( requirementMode == REQUIREMENT_MODE.ONE ) {
            ret = elements.FindAll(element => _buffer.Contains(element));
        } else if ( requirementMode == REQUIREMENT_MODE.CUSTOM ) {
            ret = CustomRequirementCheck?.Invoke(elements, _buffer.buffer);
        } else if ( requirementMode == REQUIREMENT_MODE.NONE ) {
            ret = elements;
        }

        return ret;
    }

    public List<Element> TrashHighlightCheck() {
        return _buffer.buffer.FindAll(element => element != Element.INVALID && element != Element.ALL && element != Element.NONE && element != Element.ANY);
    }
}
