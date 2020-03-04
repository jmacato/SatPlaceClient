using Avalonia.Controls.PanAndZoom; 
using Avalonia.Input;
using Avalonia.Xaml.Interactivity; 

namespace SatPlaceClient.Behaviors
{
    public class PanAndZoomBehavior : Behavior<ZoomBorder>
    {
        public PanAndZoomBehavior()
        {
            HandCursor = new Cursor(StandardCursorType.Hand);
        }

        private Cursor HandCursor { get; }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.KeyDown += KeyDown;
            AssociatedObject.PointerEnter += PointerEnter;
            AssociatedObject.PointerLeave += PointerLeave;
            AssociatedObject.Uniform();
        }

        private void PointerLeave(object sender, PointerEventArgs e)
        {
            AssociatedObject.Cursor = Cursor.Default;
        }

        private void PointerEnter(object sender, PointerEventArgs e)
        {
            AssociatedObject.Cursor = HandCursor;
        }

        private void KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.R)
            {
                AssociatedObject?.Reset();
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.KeyDown -= KeyDown;
            AssociatedObject.PointerEnter -= PointerEnter;
            AssociatedObject.PointerLeave -= PointerLeave;
        }
    }
}
