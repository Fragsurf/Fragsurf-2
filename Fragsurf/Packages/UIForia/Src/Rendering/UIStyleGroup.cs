namespace UIForia.Rendering {

    public class UIStyleGroup {

        public string name { get; internal set; }
        public StyleType styleType { get; internal set; }
        public UIStyleRunCommand hover { get; internal set; }
        public UIStyleRunCommand normal { get; internal set; }
        public UIStyleRunCommand active { get; internal set; }
        public UIStyleRunCommand focused { get; internal set; }
        public UIStyleRule rule { get; internal set; }

        public bool isExported { get; internal set; }

        public bool HasAttributeRule => rule?.attributeName != null;

        public int CountRules() {
            if (rule == null) return 0;
            UIStyleRule r = rule;
            int count = 1;
            while (r.next != null) {
                count++;
                r = r.next;
            }

            return count;
        }

    }

}