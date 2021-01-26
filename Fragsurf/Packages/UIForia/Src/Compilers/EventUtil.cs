using System;

namespace UIForia.Compilers {

    public static class EventUtil {

        public static void Subscribe(object target, string eventName, Delegate handler) {
            target.GetType().GetEvent(eventName).AddEventHandler(target, handler);
        }

    }

}