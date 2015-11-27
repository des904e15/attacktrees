using DeadDog.PDF;

namespace AttackTree
{
    public class GateObject : FillObject
    {
        private const double HEIGHT_TO_WIDTH = 0.8;
        private const double WIDTH_TO_HEIGHT = 1 / HEIGHT_TO_WIDTH;

        public GateObject(Vector1D gateHeight)
            : base(Vector2D.Zero, new Vector2D(gateHeight, gateHeight))
        {
        }

        protected sealed override void InnerBoundsChange(ref Vector1D offsetX, ref Vector1D offsetY, ref Vector1D width, ref Vector1D height)
        {
            if (height != this.Height)
                width = height * HEIGHT_TO_WIDTH;
            else if (width != this.Width)
                height = width * WIDTH_TO_HEIGHT;
        }
    }
}
