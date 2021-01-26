namespace UIForia.Templates {

    public struct BlockDefinition {

        public readonly string id;
        public readonly VariableDefinition[] variableDefinitions;
        public readonly StyleDefinition[] styleDefinitions;
        
        public BlockDefinition(string id, VariableDefinition[] variableDefinitions, StyleDefinition[] styleDefinitions) {
            this.id = id;
            this.variableDefinitions = variableDefinitions;
            this.styleDefinitions = styleDefinitions;
        }

    }

}