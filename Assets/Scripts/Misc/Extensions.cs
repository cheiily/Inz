using System.Collections.Generic;
using System.Linq;
using InzGame;
using UnityEngine;

namespace Misc {
    public static class Extensions {
        public static bool Contains(this List<Element> buffer, Element element) {
            if ( element == Element.ANY )
                return buffer.Count > 0;
            if ( element == Element.ALL )
                return true;
            if ( element == Element.NONE )
                return buffer.Count == 0;

            if ( buffer.Contains(Element.ALL) )
                return true;
            if ( buffer.Contains(Element.ANY) )
                return true;
            if ( buffer.Contains(Element.NONE) )
                return false;
            return buffer.Contains(element);
        }

        public static Vector3 Clamp(this Vector3 vec, Vector3 min, Vector3 max) {
            return new Vector3(
                Mathf.Clamp(vec.x, min.x, max.x),
                Mathf.Clamp(vec.y, min.y, max.y),
                Mathf.Clamp(vec.z, min.z, max.z)
            );
        }
    }
}