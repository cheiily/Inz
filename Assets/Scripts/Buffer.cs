using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InzGame {
    public class Buffer : MonoBehaviour {
        public GameConfiguration config;

        public int size;
        public int count;
        public List<Element> buffer;

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

            int index = 0;
            if ( config.elementProperties.GetFor(buffer[ index ]).level != 0 ) {
                for (int i = index; i < count; i++) {
                    if ( config.elementProperties.GetFor(buffer[ i ]).level == 0 ) {
                        index = i;
                        break;
                    }
                }
            }

            Element ret = buffer[ index ];
            buffer[ index ] = Element.NONE;
            count--;
            return ret;
        }

        public Element RemoveLast() {
            if ( count == 0 )
                return Element.NONE;

            int index = count - 1;
            if ( config.elementProperties.GetFor(buffer[ index ]).level != 0 ) {
                for (int i = index; i > -1; i--) {
                    if ( config.elementProperties.GetFor(buffer[ i ]).level == 0 ) {
                        index = i;
                        break;
                    }
                }
            }

            Element ret = buffer[ index ];
            buffer[ index ] = Element.NONE;
            count--;
            return ret;
        }

        public Element Remove(int index) {
            Element ret = buffer[ index ];

            buffer[ index ] = Element.NONE;
            for (int i = index; i < size - 1; i++) {
                buffer[ i ] = buffer[ i - 1 ];
            }
            buffer[ size - 1 ] = Element.NONE;
            count--;
            return ret;
        }

        public void Trash() {
            count = 0;
            buffer.Clear();
        }
    }
}