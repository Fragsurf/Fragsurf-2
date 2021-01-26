namespace UIForia.Elements {

    public interface IInputDeserializer<out T> {

        T Deserialize(string input);

    }

}