using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using DG.Tweening;
using Misc;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace InzGame.DisplayHandlers {
    public class BufferDisplay : MonoBehaviour {
        public Buffer _buffer;
        public GameConfiguration _config;
        public List<Image> displays;

        public List<GameObject> plates;
        public List<Transform> plateDefaultPositions;
        public List<Transform> plateHighlightPositions;

        public List<Tuple<Element, int>> m_displayBuffer;

        private void Awake() {
            _buffer = GetComponent<Buffer>();
            _config = GetComponent<GameManager>().config;
            foreach (Transform child in GameObject.FindWithTag("Buffer Display").transform) {
                displays.Add(child.GetComponent<Image>());
                child.GetComponent<Image>().preserveAspect = true;
            }

            _buffer.OnBufferChange += SetSprites;
            _buffer.OnSubmit += DelayShowSubmitted;

            foreach (var anchor in displays) {
                anchor.sprite = null;
                anchor.color = Color.clear;
            }

            m_displayBuffer = new List<Tuple<Element, int>>(_buffer.size);
            for (int i = 0; i < _buffer.size; i++) {
                m_displayBuffer.Add(new Tuple<Element, int>(Element.INVALID, i));
            }
        }

        public void SetSprites(object sender, List<Element> bufferState) {
            // // count distinct elements in buffer - necessary to account for multiple elements of the same type
            // Dictionary<Element, int> numElems = new Dictionary<Element, int>();
            // foreach (var element in bufferState) {
            //     numElems[element] = numElems.ContainsKey(element) ? numElems[element] + 1 : 1;
            // }
            // // remove stale displays
            // List<Element> mappingsToRemove = new List<Element>();
            // for (int i = 0; i < m_displayBuffer.Count; i++) {
            //     if (!numElems.ContainsKey(m_displayBuffer[i].Item1))
            //         mappingsToRemove.Add(m_displayBuffer[i].Item1);
            // }
            // // foreach (var index in mappingsToRemove) {
            //     m_displayBuffer.RemoveAll(tuple => mappingsToRemove.Contains(tuple.Item1));
            //     // m_displayBuffer.RemoveAt(index);
            // // }
            // // count existing elements in buffer
            // Dictionary<Element, int> numElemsDisplayed = new Dictionary<Element, int>();
            // foreach (var pushedPair in numElems) {
            //     // initialize to 0 for later checks
            //     numElemsDisplayed[pushedPair.Key] = 0;
            // }
            // foreach (var elem in m_displayBuffer) {
            //     numElemsDisplayed[elem.Item1] = numElemsDisplayed.ContainsKey(elem.Item1) ? numElemsDisplayed[elem.Item1] + 1 : 1;
            // }
            // // cull extra displays
            // foreach (var presentPair in numElemsDisplayed) {
            //     while (!numElems.ContainsKey(presentPair.Key) || numElems[presentPair.Key] < presentPair.Value) {
            //         m_displayBuffer.Remove(m_displayBuffer.First(tuple => tuple.Item1 == presentPair.Key));
            //         numElemsDisplayed[presentPair.Key]--;
            //     }
            // }
            // // add new displays
            // foreach (var pushedPair in numElems) {
            //     while (numElemsDisplayed[pushedPair.Key] < pushedPair.Value) {
            //         m_displayBuffer.Add(new Tuple<Element, int>(pushedPair.Key, FindFreeSeat(pushedPair.Key)));
            //         numElemsDisplayed[pushedPair.Key]++;
            //     }
            // }
            // // pad with NONEs
            // while (m_displayBuffer.Count < _buffer.size) {
            //     m_displayBuffer.Add(new Tuple<Element, int>(Element.NONE, FindFreeSeat(Element.NONE)));
            // }
            //
            //
            // // update sprites
            // foreach (var mapping in m_displayBuffer) {
            //     if (mapping.Item1 is Element.NONE or Element.INVALID) {
            //         anchors[mapping.Item2].sprite = null;
            //         anchors[mapping.Item2].color = Color.clear;
            //     } else {
            //         anchors[mapping.Item2].sprite = _config.elementProperties.GetFor(mapping.Item1).sprite_element;
            //         anchors[mapping.Item2].color = Color.white;
            //     }
            // }


            for (int i = 0; i < bufferState.Count; ++i) {
                if ( bufferState[ i ] is Element.NONE or Element.INVALID ) {
                    displays[ i ].sprite = null;
                    displays[ i ].color = Color.clear;
                } else {
                    displays[ i ].sprite = _config.elementProperties.GetFor(bufferState[ i ]).sprite_element;
                    // anchors[ i ].SetNativeSize();
                    displays[ i ].color = Color.white;
                }

                m_displayBuffer[ i ] = new Tuple<Element, int>(bufferState[ i ], i);
            }
        }

        public void DelayShowSubmitted(object sender, Tuple<Element, int> elem) {
            var anchor = displays[ elem.Item2 ];
            anchor.DOKill();
            anchor.DOColor(Color.clear, _config.itemJumpDuration).From(Color.clear).OnComplete(() => {
                anchor.sprite = _config.elementProperties.GetFor(elem.Item1).sprite_element;
                anchor.color = Color.white;
            });
        }

        private int FindFreeSeat(Element elem) {
            // true = taken
            List<bool> seatList = new List<bool>(_buffer.size);
            for (int i = 0; i < _buffer.size; i++) {
                seatList.Add(false);
            }

            if ( elem is Element.INVALID or Element.NONE ) {
                // if we're searching for a spot for invalid/none, it means we're cleaning up an old entry, in which case we can overwrite any spot
                foreach (var mapping in m_displayBuffer) {
                    if (mapping.Item1 is (Element.NONE or Element.INVALID))
                        seatList[ mapping.Item2 ] = true;
                }
            } else {
                foreach (var mapping in m_displayBuffer) {
                    if ( mapping.Item1 is not (Element.NONE or Element.INVALID) )
                        seatList[ mapping.Item2 ] = true;
                }
            }

            return seatList.IndexOf(false);
        }


        public void StartHighlight(List<Element> elements) {
            foreach (var displayTuple in m_displayBuffer) {
                if (Extensions.Contains(elements, displayTuple.Item1)) {
                    var plate = plates[displayTuple.Item2];
                    plate.transform.DOKill();
                    plate.transform.DOMove(plateHighlightPositions[displayTuple.Item2].position, _config.itemJumpDuration).SetEase(_config.itemJumpEase);
                    displays[displayTuple.Item2].transform.DOKill();
                    displays[displayTuple.Item2].transform.DOMove(plateHighlightPositions[displayTuple.Item2].position, _config.itemJumpDuration).SetEase(_config.itemJumpEase);
                }

                elements.Remove(displayTuple.Item1);
            }
        }

        public void StopHighlight() {
            for (int i = 0; i < plates.Count; i++) {
                var plate = plates[i];
                plate.transform.DOKill();
                plate.transform.DOMove(plateDefaultPositions[plates.IndexOf(plate)].position, _config.itemJumpDuration).SetEase(_config.itemJumpEase);
                displays[i].transform.DOKill();
                displays[i].transform.DOMove(plateDefaultPositions[plates.IndexOf(plate)].position, _config.itemJumpDuration).SetEase(_config.itemJumpEase);
            }
        }
    }
}