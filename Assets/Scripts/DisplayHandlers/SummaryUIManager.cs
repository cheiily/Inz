using DG.Tweening;
using UnityEngine;

public class SummaryUIManager : MonoBehaviour {
    public Transform posOutside;
    public Transform posInside;

    public void OnEnable() {
        transform.DOMove(posInside.position, 0.5f).From(posOutside.position);
    }
}