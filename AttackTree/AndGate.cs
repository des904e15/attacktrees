using DeadDog.PDF;

namespace AttackTree
{
    public class AndGate : GateObject
    {
        public AndGate(Vector1D gateHeight)
            : base(gateHeight)
        {
        }

        protected override void Render(ContentWriter cw, Vector2D offset)
        {
            var topleft = offset + new Vector2D(Vector1D.Zero, Size.Y);
            var topright = offset + Size;

            var left = offset + new Vector2D(Vector1D.Zero, Size.Y / 2);
            var right = offset + new Vector2D(Size.X, Size.Y / 2);

            var top = offset + new Vector2D(Size.X / 2, Size.Y);
            var bottomright = offset + new Vector2D(Size.X, Vector1D.Zero);

            cw.MoveTo(offset);
            cw.LineTo(left);
            cw.CurveTo(topleft, top, top);
            cw.CurveTo(topright, right);
            cw.LineTo(bottomright);
            cw.LineTo(offset);

            cw.CloseShape = true;
        }
    }
}
