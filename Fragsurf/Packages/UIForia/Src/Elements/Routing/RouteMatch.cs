using System;
using UIForia.Exceptions;
using UIForia.Routing;

namespace UIForia.Elements.Routing {

    public struct RouteMatch {

        public string url;
        public int matchProgress;
        public int parameterMatches;

        public RouteParameter p0;
        public RouteParameter p1;
        public RouteParameter p2;
        public RouteParameter p3;
        public RouteParameter p4;

        public RouteMatch(string url) {
            this.url = url;
            this.matchProgress = 0;
            this.parameterMatches = 0;
            this.p0 = default(RouteParameter);
            this.p1 = default(RouteParameter);
            this.p2 = default(RouteParameter);
            this.p3 = default(RouteParameter);
            this.p4 = default(RouteParameter);
        }

        public void SetParameter(RouteParameter parameter) {
            switch (parameterMatches) {
                case 0:
                    p0 = parameter;
                    break;
                case 1:
                    p1 = parameter;
                    break;
                case 2:
                    p2 = parameter;
                    break;
                case 3:
                    p3 = parameter;
                    break;
                case 4:
                    p4 = parameter;
                    break;
                default:
                    throw new IndexOutOfRangeException("Routes can only have up to 5 total dynamic parameters");
            }

            parameterMatches++;
        }

        public bool IsMatch => matchProgress >= 0;

        public void SetParameter(string pathSegment, string routeValue) {
            SetParameter(new RouteParameter(pathSegment, routeValue));
        }

        public static RouteMatch Match(string path, int ptr, RouteMatch toMatch) {
            RouteMatch clone = toMatch;

            if (path == null) {
                clone.matchProgress = -1;
                return clone;
            }

            while (clone.IsMatch && ptr < path.Length) {
                clone = MatchSegment(ref ptr, path, clone);
            }

            return clone;
        }

        public static RouteMatch Match(string path, RouteMatch toMatch) {
            return Match(path, 0, toMatch);
        }

        public static RouteMatch MatchSegment(ref int ptr, string path, RouteMatch toMatch) {
            int matchPtr = toMatch.matchProgress;
            while (ptr < path.Length) {
                char current = path[ptr];

                if (matchPtr >= toMatch.url.Length) {
                    toMatch.matchProgress = -1;
                    return toMatch;
                }

                char matchCurrent = toMatch.url[matchPtr];

                if (current == matchCurrent) {
                    ptr++;
                    matchPtr++;
                    continue;
                }

                if (current == ':') {
                    ptr++;
                    if (ptr == path.Length) {
                        throw new ParseException();
                    }

                    string name = ReadRouteSectionContents(ref ptr, path);
                    string value = ReadRouteSectionContents(ref matchPtr, toMatch.url);
                    toMatch.SetParameter(name, value);
                    continue;
                }

                toMatch.matchProgress = -1;
                return toMatch;
            }

            toMatch.matchProgress = matchPtr;

            return toMatch;
        }

        private static string ReadRouteSectionContents(ref int ptr, string path) {
            int startIndex = ptr;
            while (ptr < path.Length) {
                if (path[ptr] == '/') {
                    return path.Substring(startIndex, ptr - startIndex);
                }

                ptr++;
            }

            return path.Substring(startIndex, ptr - startIndex);
        }

        public RouteParameter GetParameter(string name) {
            if (p0.name == name) return p0;
            if (p1.name == name) return p1;
            if (p2.name == name) return p2;
            if (p3.name == name) return p3;
            if (p4.name == name) return p4;
            return default(RouteParameter);
        }

    }

}