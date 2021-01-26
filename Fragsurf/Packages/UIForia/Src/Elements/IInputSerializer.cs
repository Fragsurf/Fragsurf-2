namespace UIForia.Elements {

    public interface IInputSerializer<in T> {

        string Serialize(T input);

    }

}