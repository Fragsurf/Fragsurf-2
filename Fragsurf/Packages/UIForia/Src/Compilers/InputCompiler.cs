using System;
using System.Collections.Generic;
using System.Reflection;
using UIForia.Attributes;
using UIForia.Exceptions;
using UIForia.UIInput;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Compilers {

    public struct InputHandlerDescriptor {

        public string eventName;
        public InputEventType handlerType;
        public KeyboardModifiers modifiers;
        public EventPhase eventPhase;
        public bool requiresFocus;

    }

    public struct InputHandler {

        public InputHandlerDescriptor descriptor;
        public MethodInfo methodInfo;
        public bool useEventParameter;
        public KeyCode keyCode;
        public char character;
        public Type parameterType;

    }

    public static class InputCompiler {

        private static readonly Dictionary<Type, StructList<InputHandler>> s_Cache = new Dictionary<Type, StructList<InputHandler>>();

        private static readonly char[] s_SplitDot = {'.'};

        public static InputHandlerDescriptor ParseMouseDescriptor(string input) {
            InputHandlerDescriptor retn = ParseDescriptor(input);

            retn.handlerType = ParseMouseInputEventType(retn.eventName);

            return retn;
        }

        public static InputHandlerDescriptor ParseKeyboardDescriptor(string input) {
            InputHandlerDescriptor retn = ParseDescriptor(input);

            retn.handlerType = ParseKeyboardInputEventType(retn.eventName);

            return retn;
        }


        public static InputHandlerDescriptor ParseDragDescriptor(string input) {
            InputHandlerDescriptor retn = ParseDescriptor(input);

            retn.handlerType = ParseDragInputEventType(retn.eventName);

            return retn;
        }

        private static InputHandlerDescriptor ParseDescriptor(string input) {
            InputHandlerDescriptor retn = default;
            retn.eventPhase = EventPhase.Bubble;
            retn.modifiers = KeyboardModifiers.None;
            retn.requiresFocus = false;
            input = input.ToLower();
            int dotIndex = input.IndexOf('.');
            if (dotIndex == -1) {
                retn.eventName = input;
                return retn;
            }
            else {
                string[] parts = input.Split(s_SplitDot, StringSplitOptions.RemoveEmptyEntries);

                retn.eventName = parts[0];

                for (int i = 1; i < parts.Length; i++) {
                    string part = parts[i];

                    switch (part) {
                        case "focus":
                            retn.requiresFocus = true;
                            break;
                        case "capture": {
                            retn.eventPhase = EventPhase.Capture;
                            break;
                        }
                        case "shift": {
                            retn.modifiers |= KeyboardModifiers.Shift;
                            break;
                        }
                        case "ctrl":
                        case "control": {
                            retn.modifiers |= KeyboardModifiers.Control;
                            break;
                        }
                        case "cmd":
                        case "command": {
                            retn.modifiers |= KeyboardModifiers.Command;
                            break;
                        }
                        case "alt": {
                            retn.modifiers |= KeyboardModifiers.Alt;
                            break;
                        }
                        default:
                            throw new ParseException("Invalid mouse modifier: " + part + " in input string: " + input);
                    }
                }
            }

            return retn;
        }

        private static InputEventType ParseKeyboardInputEventType(string input) {
            switch (input.ToLower()) {
                case "down":
                    return InputEventType.KeyDown;
                case "up":
                    return InputEventType.KeyUp;
                case "helddown":
                case "held":
                    return InputEventType.KeyHeldDown;
            }

            throw new CompileException("Invalid keyboard event in template: " + input);
        }

        private static InputEventType ParseMouseInputEventType(string input) {
            switch (input.ToLower()) {
                case "click":
                    return InputEventType.MouseClick;

                case "down":
                    return InputEventType.MouseDown;

                case "up":
                    return InputEventType.MouseUp;

                case "enter":
                    return InputEventType.MouseEnter;

                case "exit":
                    return InputEventType.MouseExit;

                case "helddown":
                    return InputEventType.MouseHeldDown;

                case "move":
                    return InputEventType.MouseMove;

                case "hover":
                    return InputEventType.MouseHover;

                case "scroll":
                    return InputEventType.MouseScroll;

                case "context":
                    return InputEventType.MouseContext;

                default:
                    throw new CompileException("Invalid mouse event in template: " + input);
            }
        }


        public static StructList<InputHandler> CompileInputAnnotations(Type targetType) {
            if (s_Cache.TryGetValue(targetType, out StructList<InputHandler> handlers)) {
                return handlers;
            }

            handlers = StructList<InputHandler>.Get();

            MethodInfo[] methods = ReflectionUtil.GetInstanceMethods(targetType);

            for (int i = 0; i < methods.Length; i++) {
                MethodInfo methodInfo = methods[i];

                object[] customAttributes = methodInfo.GetCustomAttributes(true);

                if (customAttributes.Length == 0) {
                    continue;
                }

                ParameterInfo[] parameters = methodInfo.GetParameters();

                GetMouseEventHandlers(methodInfo, parameters, customAttributes, handlers);

                GetKeyboardEventHandlers(methodInfo, parameters, customAttributes, handlers);

                GetDragCreators(methodInfo, parameters, customAttributes, handlers);

                GetDragHandlers(methodInfo, parameters, customAttributes, handlers);
            }

            if (handlers.size == 0) {
                handlers.Release();
                s_Cache[targetType] = null;
                return null;
            }

            s_Cache[targetType] = handlers;

            return handlers;
        }

        private static void GetDragCreators(MethodInfo methodInfo, ParameterInfo[] parameters, object[] customAttributes, StructList<InputHandler> handlers) {
            for (int i = 0; i < customAttributes.Length; i++) {
                OnDragCreateAttribute attr = customAttributes[i] as OnDragCreateAttribute;

                if (attr == null) {
                    continue;
                }

                if (parameters.Length > 1) {
                    throw CompileException.TooManyInputAnnotationArguments(methodInfo.Name, methodInfo.DeclaringType, typeof(OnDragCreateAttribute), typeof(MouseInputEvent), parameters.Length);
                }

                if (parameters.Length == 1) {
                    if (!typeof(MouseInputEvent).IsAssignableFrom(parameters[0].ParameterType)) {
                        throw CompileException.InvalidInputAnnotation(methodInfo.Name, methodInfo.DeclaringType, typeof(OnDragCreateAttribute), typeof(MouseInputEvent), parameters[0].ParameterType);
                    }
                }

                if (!typeof(DragEvent).IsAssignableFrom(methodInfo.ReturnType)) {
                    throw CompileException.InvalidDragCreatorAnnotationReturnType(methodInfo.Name, methodInfo.DeclaringType, methodInfo.ReturnType);
                }


                if (!methodInfo.IsPublic || methodInfo.IsStatic) {
                    throw new CompileException($"{methodInfo.DeclaringType}.{methodInfo} must be an instance method and marked as public in order to be used as a drag creator");
                }

                handlers.Add(new InputHandler() {
                    descriptor = new InputHandlerDescriptor() {
                        eventPhase = attr.phase,
                        modifiers = attr.modifiers,
                        requiresFocus = false,
                        handlerType = InputEventType.DragCreate
                    },
                    methodInfo = methodInfo,
                    parameterType = parameters.Length >= 1 ? parameters[0].ParameterType : null,
                    useEventParameter = parameters.Length == 1
                });
            }
        }

        private static void GetKeyboardEventHandlers(MethodInfo methodInfo, ParameterInfo[] parameters, object[] customAttributes, StructList<InputHandler> handlers) {
            for (int i = 0; i < customAttributes.Length; i++) {
                KeyboardInputBindingAttribute attr = customAttributes[i] as KeyboardInputBindingAttribute;

                if (attr == null) {
                    continue;
                }

                if (parameters.Length > 1 || (parameters.Length > 1 && parameters[0].ParameterType != typeof(KeyboardInputEvent))) {
                    throw new Exception("Method with attribute " + customAttributes.GetType().Name + " must take 0 arguments or 1 argument of type " + nameof(KeyboardInputEvent));
                }


                if (!methodInfo.IsPublic || methodInfo.IsStatic) {
                    throw new CompileException($"{methodInfo.DeclaringType}.{methodInfo} must be an instance method and marked as public in order to be used as an input handler");
                }

                handlers.Add(new InputHandler() {
                    descriptor = new InputHandlerDescriptor() {
                        eventPhase = attr.keyEventPhase,
                        modifiers = attr.modifiers,
                        requiresFocus = attr.requiresFocus,
                        handlerType = attr.eventType
                    },
                    keyCode = attr.key,
                    character = attr.character,
                    methodInfo = methodInfo,
                    useEventParameter = parameters.Length == 1
                });
            }
        }

        private static void GetDragHandlers(MethodInfo methodInfo, ParameterInfo[] parameters, object[] customAttributes, StructList<InputHandler> handlers) {
            for (int i = 0; i < customAttributes.Length; i++) {
                DragEventHandlerAttribute attr = customAttributes[i] as DragEventHandlerAttribute;

                if (attr == null) {
                    continue;
                }

                if (parameters.Length > 1 || (parameters.Length > 1 && parameters[0].ParameterType != typeof(DragEvent))) {
                    throw new Exception("Method with attribute " + customAttributes.GetType().Name + " must take 0 arguments or 1 argument of type " + nameof(DragEvent));
                }


                if (!methodInfo.IsPublic || methodInfo.IsStatic) {
                    throw new CompileException($"{methodInfo.DeclaringType}.{methodInfo} must be an instance method and marked as public in order to be used as an input handler");
                }

                handlers.Add(new InputHandler() {
                    descriptor = new InputHandlerDescriptor() {
                        eventPhase = attr.phase,
                        modifiers = attr.modifiers,
                        requiresFocus = false,
                        handlerType = attr.eventType
                    },
                    methodInfo = methodInfo,
                    useEventParameter = parameters.Length == 1
                });
            }
        }

        private static void GetMouseEventHandlers(MethodInfo methodInfo, ParameterInfo[] parameters, object[] customAttributes, StructList<InputHandler> handlers) {
            for (int i = 0; i < customAttributes.Length; i++) {
                MouseEventHandlerAttribute attr = customAttributes[i] as MouseEventHandlerAttribute;

                if (attr == null) {
                    continue;
                }

                if (parameters.Length > 1 || (parameters.Length > 1 && parameters[0].ParameterType != typeof(MouseInputEvent))) {
                    throw new Exception("Method with attribute " + customAttributes.GetType().Name + " must take 0 arguments or 1 argument of type " + nameof(MouseInputEvent));
                }

                if (!methodInfo.IsPublic || methodInfo.IsStatic) {
                    throw new CompileException($"{methodInfo.DeclaringType}.{methodInfo} must be an instance method and marked as public in order to be used as an input handler");
                }

                handlers.Add(new InputHandler() {
                    descriptor = new InputHandlerDescriptor() {
                        eventPhase = attr.phase,
                        modifiers = attr.modifiers,
                        requiresFocus = false,
                        handlerType = attr.eventType
                    },
                    methodInfo = methodInfo,
                    useEventParameter = parameters.Length == 1
                });
            }
        }

        public static InputEventType ParseDragInputEventType(string eventName) {
            switch (eventName) {
                case "create":
                    return InputEventType.DragCreate;
                case "move":
                    return InputEventType.DragMove;
                case "hover":
                    return InputEventType.DragHover;
                case "enter":
                    return InputEventType.DragEnter;
                case "exit":
                    return InputEventType.DragExit;
                case "drop":
                    return InputEventType.DragDrop;
                case "cancel":
                    return InputEventType.DragCancel;
                case "update":
                    return InputEventType.DragUpdate;
            }

            throw new CompileException("Invalid drag event name: " + eventName);
        }

    }

}