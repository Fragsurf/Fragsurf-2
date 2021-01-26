namespace UIForia.Parsing {

    public enum UsingDeclarationType {

        Namespace,
        Element

    }
    
    public struct UsingDeclaration {

        public string name;
        public string pathName;
        public UsingDeclarationType type;
        public int lineNumber;

    }

}