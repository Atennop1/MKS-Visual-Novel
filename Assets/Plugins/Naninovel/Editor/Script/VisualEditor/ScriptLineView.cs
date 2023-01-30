// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Naninovel
{
    public abstract class ScriptLineView : VisualElement
    {
        public static ScriptLineView FocusedLine { get; private set; }

        public int LineIndex { get => lineIndex; set { lineIndex = value; SetIndexLabel(value); } }

        protected readonly VisualElement Content;

        private const float contentHeight = 22f;
        private readonly Label lineIndexLabel;
        private int lineIndex;

        protected class DragManipulator : MouseManipulator
        {
            public static bool Active { get; private set; }

            private const float displacementPadding = 2f;
            private readonly VisualElement container, handle;
            private readonly ScriptLineView body;

            public DragManipulator (VisualElement container, ScriptLineView body, VisualElement handle)
            {
                this.container = container;
                this.body = body;
                this.handle = handle;
                Active = false;
            }

            protected override void RegisterCallbacksOnTarget ()
            {
                handle.RegisterCallback(new EventCallback<MouseDownEvent>(OnMouseDown), TrickleDown.NoTrickleDown);
                handle.RegisterCallback(new EventCallback<MouseMoveEvent>(OnMouseMove), TrickleDown.NoTrickleDown);
                handle.RegisterCallback(new EventCallback<MouseUpEvent>(OnMouseUp), TrickleDown.NoTrickleDown);
            }

            protected override void UnregisterCallbacksFromTarget ()
            {
                handle.UnregisterCallback(new EventCallback<MouseDownEvent>(OnMouseDown), TrickleDown.NoTrickleDown);
                handle.UnregisterCallback(new EventCallback<MouseMoveEvent>(OnMouseMove), TrickleDown.NoTrickleDown);
                handle.UnregisterCallback(new EventCallback<MouseUpEvent>(OnMouseUp), TrickleDown.NoTrickleDown);
            }

            private void OnMouseDown (MouseDownEvent evt)
            {
                if (evt.button != 0) return; // Allow drag only with the left mouse button.

                if (FocusedLine != null)
                    SetFocused(null);

                Active = true;

                handle.CaptureMouse();
                evt.StopImmediatePropagation();

                body.style.position = Position.Absolute;
                body.BringToFront();
                // Compensate loosing a child (pos = abs).
                container.style.paddingBottom = container.style.paddingBottom.value.value + contentHeight / 2 + displacementPadding;
                container.style.paddingTop = container.style.paddingTop.value.value + contentHeight / 2;

                HandleMouseMove(evt.mousePosition);
            }

            private void OnMouseMove (MouseMoveEvent evt) => HandleMouseMove(evt.mousePosition);

            private void HandleMouseMove (Vector2 mousePos)
            {
                if (!Active) return;

                var localMousePos = container.WorldToLocal(mousePos);

                // Move body.
                body.style.left = Mathf.Clamp(localMousePos.x - body.layout.width / 2, 0f, container.layout.width - body.layout.width);
                body.style.top = Mathf.Clamp(localMousePos.y - body.layout.height / 2, 0f, container.layout.height - body.layout.height);
                body.style.right = float.NaN;
                body.style.bottom = float.NaN;

                // Displace other children at the drop position.
                var displacement = body.layout.height / 2f + body.style.marginTop.value.value + body.style.marginBottom.value.value;
                foreach (var v in container.Children())
                {
                    if (v == body) continue;
                    if (localMousePos.y <= v.layout.center.y) v.style.top = displacement + displacementPadding;
                    else v.style.top = float.NaN;
                    if (localMousePos.y > v.layout.center.y) v.style.bottom = displacement;
                    else v.style.bottom = float.NaN;
                }
            }

            private void OnMouseUp (MouseUpEvent evt)
            {
                if (!Active) return;

                Active = false;

                if (handle.HasMouseCapture())
                    handle.ReleaseMouse();

                evt.StopImmediatePropagation();

                if (container.childCount > 1) // Placing the dragged body near the drop position.
                {
                    var dropPosition = container.WorldToLocal(evt.mousePosition);
                    var neighbor = container.Children().Where(v => v != body).OrderBy(v => Vector2.Distance(dropPosition, v.layout.center)).FirstOrDefault();
                    if (neighbor != null && neighbor.layout.center.y > dropPosition.y) body.PlaceBehind(neighbor);
                    else body.PlaceInFront(neighbor);
                    (container.parent as ScriptView)?.HandleLineReordered(body);
                }

                body.style.position = Position.Relative;
                body.style.left = float.NaN;
                body.style.top = float.NaN;
                body.style.right = float.NaN;
                body.style.bottom = float.NaN;

                // Revert child loosing compensation (pos = rel).
                container.style.paddingBottom = container.style.paddingBottom.value.value - contentHeight / 2 - displacementPadding;
                container.style.paddingTop = container.style.paddingTop.value.value - contentHeight / 2;

                // Revert drop position displacement.
                foreach (var v in container.Children())
                {
                    v.style.top = float.NaN;
                    v.style.bottom = float.NaN;
                }
            }
        }

        protected ScriptLineView (int lineIndex, VisualElement container)
        {
            SetFocused(null); // Otherwise we could transfer it to another script.

            this.lineIndex = lineIndex;

            styleSheets.Add(ScriptView.StyleSheet);
            if (EditorGUIUtility.isProSkin)
                styleSheets.Add(ScriptView.DarkStyleSheet);
            if (ScriptView.CustomStyleSheet)
                styleSheets.Add(ScriptView.CustomStyleSheet);

            lineIndexLabel = new Label();
            lineIndexLabel.name = "LineIndex";
            lineIndexLabel.AddToClassList(GetType().Name.GetBefore("Line") + "Index");
            Add(lineIndexLabel);
            SetIndexLabel(lineIndex);

            Content = new VisualElement();
            Content.name = "Content";
            Content.AddToClassList(GetType().Name.GetBefore("Line") + "Content");
            Content.AddManipulator(new DragManipulator(container, this, lineIndexLabel));
            Content.RegisterCallback<MouseEnterEvent>(HandleMouseEnterEvent);
            Content.RegisterCallback<MouseLeaveEvent>(HandleMouseLeaveEvent);
            Content.RegisterCallback<MouseDownEvent>(HandleMouseDownEvent);
            Add(Content);

            AddToClassList("ScriptLine");
        }

        public abstract string GenerateLineText ();

        public static void SetFocused (ScriptLineView line)
        {
            if (DragManipulator.Active) return;

            FocusedLine?.ApplyNotFocusedStyle();
            FocusedLine = line;
            line?.ApplyFocusedStyle();
        }

        protected virtual void ApplyFocusedStyle ()
        {
            if (DragManipulator.Active) return;

            Content.style.height = StyleKeyword.Auto;
            Content.style.minHeight = contentHeight;
            Content.style.flexWrap = Wrap.Wrap;
            ColorUtility.TryParseHtmlString(EditorGUIUtility.isProSkin ? "#3a79bb" : "#948a69", out var borderColor);
            Content.style.borderBottomColor = borderColor;
            Content.style.borderLeftColor = borderColor;
            Content.style.borderRightColor = borderColor;
            Content.style.borderTopColor = borderColor;
        }

        protected virtual void ApplyNotFocusedStyle ()
        {
            Content.style.height = contentHeight;
            Content.style.flexWrap = Wrap.NoWrap;
            Content.style.borderBottomColor = StyleKeyword.Undefined;
            Content.style.borderLeftColor = StyleKeyword.Undefined;
            Content.style.borderRightColor = StyleKeyword.Undefined;
            Content.style.borderTopColor = StyleKeyword.Undefined;
        }

        protected virtual void ApplyHoveredStyle () { }

        protected virtual void ApplyNotHoveredStyle () { }

        private void SetIndexLabel (int value)
        {
            if (lineIndexLabel != null)
                lineIndexLabel.text = (value + 1).ToString();
        }

        private void HandleMouseDownEvent (MouseDownEvent evt)
        {
            if (evt.button != 0) return;
            var deselect = FocusedLine == this && !(evt.target is LineTextField);
            SetFocused(deselect ? null : this);
            evt.StopImmediatePropagation();
        }

        private void HandleMouseEnterEvent (MouseEnterEvent evt)
        {
            ApplyHoveredStyle();
        }

        private void HandleMouseLeaveEvent (MouseLeaveEvent evt)
        {
            ApplyNotHoveredStyle();
        }
    }
}
