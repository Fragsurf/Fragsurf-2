namespace UIForia.UIInput {
    public interface IInputValidatable {
        void Validate();
    }
}

/*
 * This interface isn't adding any real value at the moment, but it's here as a placeholder for future functionality.
 * In the future, we'll probably want to borrow the structure of UIForia.Elements.SubmitEvent into a ValidationEvent class,
 * which we'll use to transport all validation-relevant information. Something like:
 *
 * class ValidationEvent : UIEvent {
 *     RecordError(element e, string message, Severity s){
 *         ...
 *     }
 * }
 *
 * class OtherClass {
 *     ValidationEvent ve ...;
 *     ve.RecordError(element, "message", Severity.Warning);
 * }
 *
 * https://github.com/klanggames/seed/pull/1526#discussion_r345271128
 */
