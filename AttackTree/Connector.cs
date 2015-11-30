using DeadDog.PDF;
using System;

namespace AttackTree
{
    public class Connector : StrokeObject
    {
        private Vector1D centerX;

        public Connector(Vector2D offset, Vector2D size, Vector1D centerX)
            : base(offset, size)
        {
            this.centerX = centerX;
            BorderColor = System.Drawing.Color.Black;
        }

        protected override void Render(ContentWriter cw, Vector2D offset)
        {
            var size = Size;
            var width = Math.Abs(size.X.Value(UnitsOfMeasure.Centimeters));
            if (width < 1)
                size.X = Vector1D.Zero;

            var p1 = offset + new Vector2D(Vector1D.Zero, size.Y);
            var p2 = offset + new Vector2D(Vector1D.Zero, size.Y / 2);
            var p3 = offset + new Vector2D(size.X, size.Y / 2);
            var p4 = offset + new Vector2D(size.X, Vector1D.Zero);

            cw.MoveTo(centerX, offset.Y + size.Y + Node.GateHeight + Node.Margin);
            cw.LineTo(centerX, offset.Y + size.Y + Node.GateHeight / 2);

            cw.MoveTo(offset.X, offset.Y + size.Y + Node.GateHeight / 2);
            cw.LineTo(p1);
            cw.LineTo(p2);
            cw.LineTo(p3);
            cw.LineTo(p4);
        }
    }
}
