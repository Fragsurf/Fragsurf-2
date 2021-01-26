namespace UIForia.Routing {

    public struct RouteParameter {

        public readonly string name;
        public readonly string value;

        public RouteParameter(string name, string value) {
            this.name = name;
            this.value = value;
        }
        
        public RouteParameter(string name, int value) {
            this.name = name;
            this.value = value.ToString();
        }
        
        public RouteParameter(string name, float value) {
            this.name = name;
            this.value = value.ToString();
        }
        
        public RouteParameter(string name, bool value) {
            this.name = name;
            this.value = value.ToString();
        }

        public int AsInt => int.Parse(value);
        public float AsFloat => float.Parse(value);
        public bool AsBool => bool.Parse(value);
        public string AsString => value;

        public static implicit operator string(RouteParameter parameter) {
            return parameter.value;
        }

        public static implicit operator int(RouteParameter parameter) {
            return parameter.AsInt;
        }

        public static implicit operator float(RouteParameter parameter) {
            return parameter.AsFloat;
        }

        public static implicit operator bool(RouteParameter parameter) {
            return parameter.AsBool;
        }

    }

}