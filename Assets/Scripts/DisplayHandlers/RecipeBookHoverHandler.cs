using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class RecipeBookHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    public Transform openPos;
    public Transform closedPos;

    public void OnPointerEnter(PointerEventData eventData) {
        transform.DOKill();
        transform.DOMove(openPos.position, 0.3f);
    }

    public void OnPointerExit(PointerEventData eventData) {
        transform.DOKill();
        transform.DOMove(closedPos.position, 0.3f);
    }
}