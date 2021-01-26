namespace UIForia.Templates {

    public struct VariableDefinition {

        public readonly string name;
        public readonly string typeName;
        public readonly string defaultValue;
        
        public VariableDefinition(string name, string typeName, string defaultValue) {
            this.name = name;
            this.typeName = typeName;
            this.defaultValue = defaultValue;
        }

    }

}