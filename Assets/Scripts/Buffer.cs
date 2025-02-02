using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using UnityEngine;

namespace InzGame {
    //todo decide behavior on submission when all elements are high/higher level
    public class Buffer : MonoBehaviour {
        public GameConfiguration config;

        public int size;
        public int count = 0;
        public List<Element> buffer;

        public event EventHandler<List<Element>> OnBufferChange;

        private void Awake() {
            for (int i = 0; i < size; i++)
                buffer.Add(Element.INVALID);
        }

        public void Submit(Element elem) {
            if ( count >= size ) {
                var removed = RemoveFirst(elem);
                if ( removed == Element.NONE ) {
                    Debug.Log("Buffer is full and all elements are of higher level. Cannot submit.");
                    return;
                }
            }

            buffer[ count ] = elem;
            count++;
            OnBufferChange?.Invoke(this, buffer);
        }

        private Element RemoveFirst(Element newElem) {
            if ( count == 0 )
                return Element.INVALID;

            int index = FindFirstLowestLevel();

            Element ret = buffer[ index ];
            if ( config.elementProperties.GetFor(ret).level > config.elementProperties.GetFor(newElem).level )
                return Element.NONE;
            // buffer[ index ] = Element.NONE;

            for (int i = index; i < size - 1; i++)
                buffer[ i ] = buffer[ i + 1 ];
            buffer[ size - 1 ] = Element.INVALID;

            count--;
            return ret;
        }

        private Element RemoveLast() {
            if ( count == 0 )
                return Element.INVALID;

            int index = FindLastLowestLevel();

            Element ret = buffer[ index ];
            buffer[ index ] = Element.INVALID;
            count--;
            return ret;
        }

        public Element Remove(int index) {
            Element ret = buffer[ index ];

            // buffer[ index ] = Element.NONE;
            for (int i = index; i < size - 1; i++) {
                buffer[ i ] = buffer[ i - 1 ];
            }
            buffer[ size - 1 ] = Element.INVALID;
            count--;
            OnBufferChange?.Invoke(this, buffer);
            return ret;
        }

        public void Remove(Element element) {
            if ( element == Element.ANY ) {
                Remove(FindFirstLowestLevel());
                return;
            } else if (element == Element.ALL) {
                Trash();
                return;
            }

            int idx = buffer.IndexOf(element);
            if (idx == -1)
                return;
            for (int i = idx; i < count - 1; i++)
                buffer[ i ] = buffer[ i + 1 ];
            buffer[ count - 1 ] = Element.INVALID;
            count--;
            OnBufferChange?.Invoke(this, buffer);
        }

        public void Trash() {
            count = 0;
            for (int i = 0; i < size; i++) {
                buffer[ i ] = Element.INVALID;
            }
            OnBufferChange?.Invoke(this, buffer);
        }

        public int FindFirstLowestLevel() {
            if (count == 0)
                return 0;

            int mini = 0;
            int minlevel = 100;
            for (int i = 0; i < count; i++) {
                if ( buffer[ i ] != Element.INVALID && config.elementProperties.GetFor(buffer[i]).level < config.elementProperties.GetFor(buffer[mini]).level )
                    mini = i;
            }

            return mini;
        }

        public int FindLastLowestLevel() {
            if (count == 0)
                return size - 1;

            int mini = count - 1;
            for (int i = count - 1; i > -1; i--) {
                if ( buffer[ i ] != Element.INVALID && config.elementProperties.GetFor(buffer[i]).level < config.elementProperties.GetFor(buffer[mini]).level )
                    mini = i;
            }

            return mini;
        }

        public bool Contains(Element element) {
            if ( element == Element.ANY )
                return count > 0;
            if ( element == Element.ALL )
                return true;
            if ( element == Element.NONE )
                return count == 0;
            if ( element == Element.INVALID )
                return false;
            return buffer.Contains(element);
        }

        public static int Count(List<Element> buffer) {
            return buffer.FindAll(element => element != Element.INVALID).Count;
        }

        public static int ForceRecount(Buffer buffer) {
            int cnt = buffer.buffer.FindAll(element => element != Element.INVALID).Count;
            buffer.count = cnt;
            return cnt;
        }
    }
}