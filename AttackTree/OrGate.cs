using DeadDog.PDF;

namespace AttackTree
{
    public class OrGate : GateObject
    {
        public OrGate(Vector1D gateHeight)
            : base(gateHeight)
        {
        }

        protected override void Render(ContentWriter cw, Vector2D offset)
        {
            var top = offset + new Vector2D(Size.X / 2, Size.Y);

            var left_topleft = offset + new Vector2D(Vector1D.Zero, Size.Y * 3 / 4);
            var left_bottomleft = offset + new Vector2D(Vector1D.Zero, Size.Y * 1 / 4);

            var right_topright = offset + new Vector2D(Size.X, Size.Y * 3 / 4);
            var right_bottomright = offset + new Vector2D(Size.X, Size.Y * 1 / 4);

            var bottomright = offset + new Vector2D(Size.X, Vector1D.Zero);

            var indent = Size.X / 5;

            var bMidR = offset + new Vector2D(Size.X - indent, Size.Y / 10);
            var bottommid = offset + new Vector2D(Size.X / 2, Size.Y / 10);
            var bMidL = offset + new Vector2D(indent, Size.Y / 10);

            cw.MoveTo(offset);
            cw.LineTo(left_bottomleft);
            cw.CurveFromTo(left_topleft, top);
            cw.CurveFromTo(right_topright, right_bottomright);
            cw.LineTo(bottomright);
            cw.CurveTo(bMidR, bottommid);
            cw.CurveFromTo(bMidL, offset);

            cw.CloseShape = true;
        }
    }
}
