namespace UIForia.Layout {

    public enum AlignmentDirection {

        Start = 0,
        End = 1

    }

    public enum AlignmentTarget {

        Unset = 0,
        LayoutBox,
        Parent,
        ParentContentArea,
        Template,
        TemplateContentArea,
        View,
        Screen,
        Mouse

    }

    public enum AlignmentBoundary {

        Unset = 0,
        Screen,
        Parent,
        ParentContentArea,
        Clipper,
        View,
        
    }

}