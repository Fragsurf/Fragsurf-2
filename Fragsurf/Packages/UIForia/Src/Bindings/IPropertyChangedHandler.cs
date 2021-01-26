namespace UIForia.Bindings {

    public interface IPropertyChangedHandler {

        void OnPropertyChanged(string propertyName, object oldValue);

    }

}