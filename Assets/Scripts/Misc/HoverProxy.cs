using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Misc {
    public class HoverProxy : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
        public UnityEvent OnPointerEnterEvent;
        public UnityEvent OnPointerExitEvent;

        public void OnPointerEnter(PointerEventData eventData) {
            OnPointerEnterEvent.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData) {
            OnPointerExitEvent.Invoke();
        }
    }
}