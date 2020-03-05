using System;
using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.PanAndZoom;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Xaml.Interactivity;

namespace SatPlaceClient.Behaviors
{
    public class CanvasDraggableBehavior : Behavior<Control>
    {
        private readonly Cursor HoverCursor = new Cursor(StandardCursorType.DragMove);
        private Avalonia.Vector? LastPoint;

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.PointerEnter += PointerEnter;
            AssociatedObject.PointerLeave += PointerLeave;

            AssociatedObject.PointerMoved += OnPointerMoved;
            AssociatedObject.PointerPressed += OnPointerPressed;
            AssociatedObject.PointerReleased += OnPointerReleased;
        }

        private void OnPointerMoved(object _, PointerEventArgs e)
        {
            if (LastPoint.HasValue && AssociatedObject.Parent is Canvas c)
            {
                e.Handled = true;

                var delta = e.GetPosition(AssociatedObject) - LastPoint.Value;
                var deltaRnd = new Vector2((float)Math.Round(delta.X), (float)Math.Round(delta.Y));
                var dimVector = new Vector2((float)c.Bounds.Width, (float)c.Bounds.Height);
                var assocObjDimVector = new Vector2((float)AssociatedObject.Bounds.Width, (float)AssociatedObject.Bounds.Height);
                var curVector = new Vector2((float)Canvas.GetLeft(AssociatedObject), (float)Canvas.GetTop(AssociatedObject));
                var newVector = Vector2.Clamp(curVector + deltaRnd, Vector2.Zero, dimVector - assocObjDimVector);

                Canvas.SetLeft(AssociatedObject, newVector.X);
                Canvas.SetTop(AssociatedObject, newVector.Y);
            }
        }
        
#pragma warning disable
        private void OnPointerPressed(object _, PointerPressedEventArgs e)
        {
            if (e.MouseButton != MouseButton.Left) return;
            e.Handled = true;
            LastPoint = e.GetPosition(AssociatedObject);
        }
#pragma warning restore

        private void OnPointerReleased(object _, PointerReleasedEventArgs e)
        {
            LastPoint = null;
        }

        private void PointerLeave(object sender, PointerEventArgs e)
        {
            AssociatedObject.Cursor = Cursor.Default;
        }

        private void PointerEnter(object sender, PointerEventArgs e)
        {
            AssociatedObject.Cursor = HoverCursor;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.PointerEnter -= PointerEnter;
            AssociatedObject.PointerLeave -= PointerLeave;

            AssociatedObject.PointerMoved += OnPointerMoved;
            AssociatedObject.PointerPressed += OnPointerPressed;
            AssociatedObject.PointerReleased += OnPointerReleased;
        }
    }
}
