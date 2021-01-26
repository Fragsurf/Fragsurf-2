using System;
using UIForia.Attributes;
using UIForia.Text;

namespace UIForia.Elements {

    [TemplateTagName("Input")]
    [Template(TemplateType.Internal, "Elements/InputElement.xml")]
    public class InputElement<T> : UIInputElement where T : IEquatable<T> {

        public T value;

        public IInputFormatter formatter;
        public IInputSerializer<T> serializer;
        public IInputDeserializer<T> deserializer;

        public event Action<T> onSubmit;

        public int MaxLength = int.MaxValue;

        public override void OnCreate() {
            base.OnCreate();
            deserializer = deserializer ?? (IInputDeserializer<T>) GetDeserializer();
            serializer = serializer ?? (IInputSerializer<T>) GetSerializer();
            formatter = formatter ?? GetFormatter();
        }

        public void Reset() {
            selectionRange = new SelectionRange(0, text.Length);
            HandleCharactersDeletedForwards();
        }

        [OnPropertyChanged(nameof(value))]
        public void OnInputValueChanged() {
            string oldText = text;
            text = serializer.Serialize(value) ?? string.Empty;

            selectionRange = new SelectionRange(int.MaxValue);
            T v = deserializer.Deserialize(text);

            if (hasFocus) {
                ScrollToCursor();
            }

            value = v;

            if (oldText != text) {
                EmitTextChanged();
            }
        }

        protected override void HandleCharactersEntered(string characters) {
            string previous = text;
            text = SelectionRangeUtil.InsertText(text, ref selectionRange, characters);
            HandleTextChanged(previous);
        }

        protected override void HandleCharactersDeletedForwards() {
            string previous = text;
            text = SelectionRangeUtil.DeleteTextForwards(text, ref selectionRange);
            HandleTextChanged(previous);
        }

        protected override void HandleCharactersDeletedBackwards() {
            string previous = text;
            text = SelectionRangeUtil.DeleteTextBackwards(text, ref selectionRange);
            HandleTextChanged(previous);
        }

        protected override void HandleSubmit() {
            onSubmit?.Invoke(value);
        }

        private void HandleTextChanged(string previous) {
            string preFormat = text;

            if (formatter != null) { // todo -- handle when to format
                text = formatter.Format(text);
            }

            T newValue = deserializer.Deserialize(text);

            if (text.Length > MaxLength) {
                text = text.Substring(0, MaxLength);
                newValue = deserializer.Deserialize(text);
            }

            if (text != preFormat) {
                int diff = text.Length - preFormat.Length;
                selectionRange = new SelectionRange(selectionRange.cursorIndex + diff);
            }

            // todo -- fix this boxing!
            if (!Equals(value, newValue)) {
                value = newValue;
                // onValueChanged?.Invoke(value);
            }

            if (text != previous) {
                EmitTextChanged();
            }
        }

        public bool ShowPlaceholder => placeholder != null && string.IsNullOrEmpty(text);

        public override string GetDisplayName() {
            return $"Input<{typeof(T).Name}>";
        }

        protected object GetDeserializer() {
            if (typeof(T) == typeof(int)) {
                return InputDeserializers.IntDeserializer;
            }

            if (typeof(T) == typeof(float)) {
                return InputDeserializers.FloatDeserializer;
            }

            if (typeof(T) == typeof(double)) {
                return InputDeserializers.DoubleDeserializer;
            }

            if (typeof(T) == typeof(string)) {
                return InputDeserializers.StringDeserializer;
            }

            throw new Exception($"InputElement with generic type {typeof(T)} requires a custom serializer and deserializer in order to function because {typeof(T)} is not a float, int, or string");
        }

        protected object GetSerializer() {
            if (typeof(T) == typeof(int)) {
                return InputSerializers.IntSerializer;
            }

            if (typeof(T) == typeof(float)) {
                return InputSerializers.FloatSerializer;
            }

            if (typeof(T) == typeof(double)) {
                return InputSerializers.DoubleSerializer;
            }

            if (typeof(T) == typeof(string)) {
                return InputSerializers.StringSerializer;
            }

            throw new Exception($"InputElement with generic type {typeof(T)} requires a custom serializer and deserializer in order to function because {typeof(T)} is not a float, int, or string");
        }

        protected IInputFormatter GetFormatter() {
            if (typeof(T) == typeof(float)) {
                return InputFormatters.FloatFormatter;
            }

            if (typeof(T) == typeof(double)) {
                return InputFormatters.FloatFormatter;
            }

            if (typeof(T) == typeof(int)) {
                return InputFormatters.IntFormatter;
            }

            return null;
        }

    }

}