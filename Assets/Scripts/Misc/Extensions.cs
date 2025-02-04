using System.Collections.Generic;
using System.Linq;
using InzGame;

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
    }
}