using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InzGame {
    //todo decide behavior on submission when all elements are high/higher level
    public class Buffer : MonoBehaviour {
        public GameConfiguration config;

        public int size;
        public int count = 0;
        public List<Element> buffer;

        private void Awake() {
            for (int i = 0; i < size; i++)
                buffer.Add(Element.NONE);
        }

        public void Submit(Element elem) {
            if ( count >= size ) {
                RemoveFirst();
            }

            buffer[ count ] = elem;
            count++;
        }

        public Element RemoveFirst() {
            if ( count == 0 )
                return Element.NONE;

            int index = FindFirstLowestLevel();

            Element ret = buffer[ index ];
            // buffer[ index ] = Element.NONE;

            for (int i = index; i < size - 1; i++)
                buffer[ i ] = buffer[ i + 1 ];
            buffer[ size - 1 ] = Element.NONE;

            count--;
            return ret;
        }

        public Element RemoveLast() {
            if ( count == 0 )
                return Element.NONE;

            int index = FindLastLowestLevel();

            Element ret = buffer[ index ];
            buffer[ index ] = Element.NONE;
            count--;
            return ret;
        }

        public Element Remove(int index) {
            Element ret = buffer[ index ];

            // buffer[ index ] = Element.NONE;
            for (int i = index; i < size - 1; i++) {
                buffer[ i ] = buffer[ i - 1 ];
            }
            buffer[ size - 1 ] = Element.NONE;
            count--;
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
            buffer[ count - 1 ] = Element.NONE;
            count--;
        }

        public void Trash() {
            count = 0;
            for (int i = 0; i < size; i++) {
                buffer[ i ] = Element.NONE;
            }
        }

        public int FindFirstLowestLevel() {
            if (count == 0)
                return 0;

            int mini = 0;
            int minlevel = 100;
            for (int i = 0; i < count; i++) {
                if ( buffer[ i ] != Element.NONE && config.elementProperties.GetFor(buffer[i]).level < config.elementProperties.GetFor(buffer[mini]).level )
                    mini = i;
            }

            return mini;
        }

        public int FindLastLowestLevel() {
            if (count == 0)
                return size - 1;

            int mini = count;
            for (int i = count - 1; i > -1; i--) {
                if ( buffer[ i ] != Element.NONE && config.elementProperties.GetFor(buffer[i]).level < config.elementProperties.GetFor(buffer[mini]).level )
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
                return false;
            return buffer.Contains(element);
        }

        public static int Count(List<Element> buffer) {
            return buffer.FindAll(element => element != Element.NONE).Count;
        }

        public static int ForceRecount(Buffer buffer) {
            int cnt = buffer.buffer.FindAll(element => element != Element.NONE).Count;
            buffer.count = cnt;
            return cnt;
        }
    }
}