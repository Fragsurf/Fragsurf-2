using System;
using JetBrains.Annotations;
using UIForia.Attributes;
using UIForia.Rendering;
using UIForia.Text;
using UIForia.UIInput;
using UnityEngine;

#pragma warning disable 0649
namespace UIForia.Elements {

    // todo use StructList<char> instead of string to alloc less
    public abstract class UIInputElement : BaseInputElement, IFocusableEvented {

        private UITextElement textElement;

        // internal TextInfo textInfo;
        internal string text;

        private string m_placeholder;

        public string placeholder {
            get { return string.IsNullOrEmpty(m_placeholder) ? "" : m_placeholder; }
            set { m_placeholder = value; }
        }

        public event Action<FocusEvent> onFocus;
        public event Action<BlurEvent> onBlur;

        public bool autofocus;

        protected float holdDebounce = 0.05f;
        protected float timestamp;

        public float caretBlinkRate = 0.85f;
        protected float blinkStartTime;

        protected SelectionRange selectionRange;

        protected bool hasFocus;
        protected bool selectAllOnFocus;
        protected bool isSelecting;

        protected Vector2 textScroll = new Vector2(0, 0);

        protected float keyLockTimestamp;

        private bool isReady;

        public UIInputElement() {
            selectionRange = new SelectionRange(0);
        }

        protected static string clipboard {
            get { return GUIUtility.systemCopyBuffer; }
            set { GUIUtility.systemCopyBuffer = value; }
        }

        public override void OnCreate() {
            text = text ?? string.Empty;
            style.SetPainter("UIForia::Input", StyleState.Normal);
            application.InputSystem.RegisterFocusable(this);
        }

        public override void OnEnable() {
            textElement = FindById<UITextElement>("input-element-text");
            if (autofocus) {
                application.InputSystem.RequestFocus(this);
            }

            if (string.IsNullOrEmpty(m_placeholder)) {
                FindById("placeholder-text")?.SetAttribute("empty", "true");
            }
        }

        public override void OnUpdate() {
            if (isReady) {
                ScrollToCursor();
            }

            isReady = true;
        }

        protected void EmitTextChanged() {
            textElement.SetText(text);
        }

        protected abstract void HandleSubmit();
        protected abstract void HandleCharactersDeletedForwards();
        protected abstract void HandleCharactersDeletedBackwards();
        protected abstract void HandleCharactersEntered(string characters);

        public override void OnDestroy() {
            Blur();
            application.InputSystem.UnRegisterFocusable(this);
        }

        public override void OnDisable() {
            Blur();
        }

        public override string GetDisplayName() {
            return "InputElement";
        }

        [UsedImplicitly]
        [OnMouseClick]
        public void OnMouseClick(MouseInputEvent evt) {
            bool hadFocus = hasFocus;

            if (evt.IsConsumed || (!hasFocus && !Input.RequestFocus(this))) {
                return;
            }

            if (!hadFocus && selectAllOnFocus) {
                return;
            }

            blinkStartTime = Time.unscaledTime;
            Vector2 mouse = evt.MousePosition - layoutResult.screenPosition - layoutResult.ContentRect.position;
            mouse += textScroll;

            if (evt.IsDoubleClick) {
                selectionRange = textElement.textInfo.SelectWordAtPoint(mouse);
            }
            else if (evt.IsTripleClick) {
                selectionRange = textElement.textInfo.SelectLineAtPoint(mouse);
            }
            else {
                selectionRange = new SelectionRange(textElement.textInfo.GetIndexAtPoint(mouse));
                ScrollToCursor();
            }

            evt.StopPropagation();
        }

        [UsedImplicitly]
        [OnKeyDownWithFocus]
        [OnKeyHeldDownWithFocus]
        public void EnterText(KeyboardInputEvent evt) {
            evt.StopPropagation();
            if (HasDisabledAttr()) return;

            switch (evt.keyCode) {
                case KeyCode.Home:
                    HandleHome(evt);
                    break;
                case KeyCode.End:
                    HandleEnd(evt);
                    break;
                case KeyCode.Backspace:
                    HandleBackspace(evt);
                    break;
                case KeyCode.Delete:
                    HandleDelete(evt);
                    break;
                case KeyCode.LeftArrow:
                    HandleLeftArrow(evt);
                    break;
                case KeyCode.RightArrow:
                    HandleRightArrow(evt);
                    break;
                case KeyCode.C when evt.onlyControl && selectionRange.HasSelection:
                    HandleCopy(evt);
                    break;
                case KeyCode.V when evt.onlyControl && selectionRange.HasSelection:
                    HandlePaste(evt);
                    break;
                case KeyCode.X when evt.onlyControl && selectionRange.HasSelection:
                    HandleCut(evt);
                    break;
                case KeyCode.A when evt.onlyControl && selectionRange.HasSelection:
                    HandleSelectAll(evt);
                    //
                    break;
                default:
                    OnTextEntered(evt);
                    break;
            }
        }

        private void OnTextEntered(KeyboardInputEvent evt) {
            if (evt.ctrl) {
                return;
            }

            char c = evt.character;

            if (evt.keyCode == KeyCode.Return) {
                if (!InitKeyPress(evt)) {
                    return;
                }
                HandleSubmit();
                return;
            }

            if (evt.keyCode == KeyCode.Tab) {
                return;
            }

            if (c == '\n' || c == '\t') return;

            // assume we only ever use 1 text span for now, this should change in the future
            if (!textElement.textInfo.rootSpan.textStyle.fontAsset.HasCharacter(c)) {
                return;
            }

            HandleCharactersEntered(c.ToString());
            ScrollToCursor();
        }

        protected void ScrollToCursor() {
            if (!hasFocus || textElement?.textInfo == null) {
                return;
            }

            textElement.textInfo.Layout(Vector2.zero, float.MaxValue);
            Rect rect = VisibleTextRect;

            Vector2 cursor = textElement.textInfo.GetCursorPosition(selectionRange.cursorIndex);
            if (cursor.x - textScroll.x >= rect.width) {
                textScroll.x = (cursor.x - rect.width + rect.x);
            }
            else if (cursor.x - textScroll.x < rect.xMin) {
                textScroll.x = (cursor.x - rect.x);
                if (textScroll.x < 0) textScroll.x = 0;
            }

            if (VisibleTextRect.width >= textElement.layoutResult.ActualWidth) {
                textScroll.x = 0;
            }

            textElement.style.SetTransformPositionX(-textScroll.x, StyleState.Normal);
        }

        private void HandleHome(KeyboardInputEvent evt) {
            selectionRange = textElement.textInfo.MoveToStartOfLine(selectionRange, evt.shift);
            blinkStartTime = Time.unscaledTime;
            ScrollToCursor();
        }

        private void HandleEnd(KeyboardInputEvent evt) {
            selectionRange = textElement.textInfo.MoveToEndOfLine(selectionRange, evt.shift);
            blinkStartTime = Time.unscaledTime;
            ScrollToCursor();
        }

        private void HandleBackspace(KeyboardInputEvent evt) {
            if (!InitKeyPress(evt)) return;

            HandleCharactersDeletedBackwards();
            ScrollToCursor();
        }

        private bool InitKeyPress(in KeyboardInputEvent evt) {
            if (evt.eventType == InputEventType.KeyHeldDown) {
                if (!CanTriggerHeldKey()) return false;
                timestamp = Time.unscaledTime;
            }
            else {
                keyLockTimestamp = Time.unscaledTime;
            }

            blinkStartTime = Time.unscaledTime;
            return true;
        }

        private void HandleDelete(KeyboardInputEvent evt) {
            if (!InitKeyPress(evt)) return;
            if (evt.ctrl || evt.command) {
                selectionRange = new SelectionRange(selectionRange.cursorIndex, text.Length);
            }

            HandleCharactersDeletedForwards();
            ScrollToCursor();
        }

        private void HandleLeftArrow(KeyboardInputEvent evt) {
            if (!InitKeyPress(evt)) return;

            selectionRange = textElement.textInfo.MoveCursorLeft(selectionRange, evt.shift, evt.ctrl || evt.command);
            ScrollToCursor();
        }

        private void HandleRightArrow(KeyboardInputEvent evt) {
            if (!InitKeyPress(evt)) return;

            selectionRange = textElement.textInfo.MoveCursorRight(selectionRange, evt.shift, evt.ctrl || evt.command);
            ScrollToCursor();
        }

        private void HandleCopy(KeyboardInputEvent evt) {
            if (evt.onlyControl && selectionRange.HasSelection) {
                clipboard = textElement.textInfo.GetSelectedString(selectionRange);
                evt.StopPropagation();
            }
        }

        private void HandleCut(KeyboardInputEvent evt) {
            clipboard = textElement.textInfo.GetSelectedString(selectionRange);
            HandleCharactersDeletedBackwards();
            evt.StopPropagation();
        }

        private void HandlePaste(KeyboardInputEvent evt) {
            HandleCharactersEntered(clipboard);
            evt.StopPropagation();
        }

        private void HandleSelectAll(KeyboardInputEvent evt) {
            selectionRange = new SelectionRange(0, int.MaxValue);
            evt.StopPropagation();
        }

        public bool HasDisabledAttr() {
            return GetAttribute("disabled") != null;
        }

        public bool CanTriggerHeldKey() {
            if (GetAttribute("disabled") != null) return false;
            if (Time.unscaledTime - keyLockTimestamp < 0.5f) {
                return false;
            }

            if (Time.unscaledTime - timestamp < holdDebounce) {
                return false;
            }

            return true;
        }

        [UsedImplicitly]
        [OnDragCreate]
        public TextSelectDragEvent CreateDragEvent(MouseInputEvent evt) {
            if (evt.IsMouseRightDown) return null;
            if (!hasFocus) {
                application.InputSystem.RequestFocus(this);
            }

            TextSelectDragEvent retn = new TextSelectDragEvent(this);
            Vector2 mouseDownPosition = evt.LeftMouseDownPosition - layoutResult.screenPosition - layoutResult.ContentRect.position + textScroll;
            Vector2 mousePosition = evt.MousePosition - layoutResult.screenPosition - layoutResult.ContentRect.position + textScroll;
            int indexAtDownPoint = textElement.textInfo.GetIndexAtPoint(mouseDownPosition);

            int indexAtPoint = textElement.textInfo.GetIndexAtPoint(mousePosition);
            if (indexAtDownPoint < indexAtPoint) {
                selectionRange = new SelectionRange(indexAtPoint, indexAtDownPoint);
            }

            else {
                selectionRange = new SelectionRange(indexAtDownPoint, indexAtPoint);
            }

            return retn;
        }

        protected Rect VisibleTextRect {
            get { return layoutResult.ContentRect; }
        }

        public bool Focus() {
            if (GetAttribute("disabled") != null) {
                return false;
            }

            ScrollToCursor();
            hasFocus = true;
            onFocus?.Invoke(new FocusEvent());
            return true;
        }

        public void Blur() {
            hasFocus = false;
            selectionRange = new SelectionRange(selectionRange.cursorIndex);
            onBlur?.Invoke(new BlurEvent());
        }

        public class TextSelectDragEvent : DragEvent {

            protected readonly UIInputElement _uiInputElement;

            public TextSelectDragEvent(UIInputElement origin) {
                this._uiInputElement = origin;
                _uiInputElement.isSelecting = true;
            }

            public override void Update() {
                Vector2 mouse = MousePosition - _uiInputElement.layoutResult.screenPosition - _uiInputElement.layoutResult.ContentRect.position;
                mouse += _uiInputElement.textScroll;
                _uiInputElement.selectionRange = new SelectionRange(_uiInputElement.textElement.textInfo.GetIndexAtPoint(mouse), _uiInputElement.selectionRange.selectIndex > -1
                    ? _uiInputElement.selectionRange.selectIndex
                    : _uiInputElement.selectionRange.cursorIndex);
                _uiInputElement.ScrollToCursor();
            }

            public override void OnComplete() {
                _uiInputElement.isSelecting = false;
                _uiInputElement.selectionRange = new SelectionRange(_uiInputElement.selectionRange.selectIndex, _uiInputElement.selectionRange.cursorIndex);
            }

        }

        [CustomPainter("UIForia::Input")]
        public class InputElementPainter : StandardRenderBox {

            public Path2D path = new Path2D();

            public override void PaintBackground(RenderContext ctx) {
                base.PaintBackground(ctx);

                UIInputElement inputElement = (UIInputElement) element;

                path.Clear();
                path.SetTransform(inputElement.layoutResult.matrix.ToMatrix4x4());

                float blinkPeriod = 1f / inputElement.caretBlinkRate;

                bool blinkState = (Time.unscaledTime - inputElement.blinkStartTime) % blinkPeriod < blinkPeriod / 2;

                Rect contentRect = inputElement.layoutResult.ContentRect;

                var textInfo = inputElement.textElement.textInfo;

                // float baseLineHeight = textInfo.rootSpan.textStyle.fontAsset.faceInfo.LineHeight;
                // float scaledSize = textInfo.rootSpan.fontSize / textInfo.rootSpan.textStyle.fontAsset.faceInfo.PointSize;
                // float lh = baseLineHeight * scaledSize;

                if (!inputElement.isSelecting && inputElement.hasFocus && blinkState) {
                    path.BeginPath();
                    path.SetStroke(inputElement.style.CaretColor);
                    path.SetStrokeWidth(1f);
                    Vector2 p = textInfo.GetCursorPosition(inputElement.selectionRange.cursorIndex) - inputElement.textScroll;
                    path.MoveTo(inputElement.layoutResult.ContentRect.min + p);
                    path.VerticalLineTo(inputElement.layoutResult.ContentRect.yMax);
                    path.EndPath();
                    path.Stroke();
                }

                if (inputElement.selectionRange.HasSelection) {
                    RangeInt lineRange = new RangeInt(0, 1); //textInfo.GetLineRange(selectionRange));textInfo.GetLineRange(selectionRange);
                    path.BeginPath();
                    path.SetFill(inputElement.style.SelectionBackgroundColor);

                    if (lineRange.length > 1) {
                        // todo this doesn't really work yet
                        for (int i = lineRange.start + 1; i < lineRange.end - 1; i++) {
                            //                        Rect rect = textInfo.GetLineRect(i);
                            //                        rect.x += contentRect.x;
                            //                        rect.y += contentRect.y;
                            //                        path.Rect(rect);
                        }
                    }
                    else {
                        Rect rect = textInfo.GetLineRect(lineRange.start);
                        Vector2 cursorPosition = textInfo.GetCursorPosition(inputElement.selectionRange.cursorIndex) - inputElement.textScroll;
                        Vector2 selectPosition = textInfo.GetSelectionPosition(inputElement.selectionRange) - inputElement.textScroll;
                        float minX = Mathf.Min(cursorPosition.x, selectPosition.x);
                        float maxX = Mathf.Max(cursorPosition.x, selectPosition.x);
                        minX += contentRect.x;
                        maxX += contentRect.x;
                        rect.y += contentRect.y;
                        float x = Mathf.Max(minX, contentRect.x);
                        float cursorToContentEnd = contentRect.width;
                        float cursorToMax = maxX - x;
                        path.Rect(x, rect.y, Mathf.Min(cursorToContentEnd, cursorToMax), rect.height);
                    }

                    path.Fill();
                }

                ctx.DrawPath(path);
            }

        }

    }

}