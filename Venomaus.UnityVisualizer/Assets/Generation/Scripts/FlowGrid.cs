using Venomaus.FlowVitae.Grids;

namespace Assets.Generation.Scripts
{
    public class FlowGrid : GridBase<int, FlowCell>
    {
        public FlowGrid(int width, int height) : base(width, height)
        {
        }

        // Custom impl, to be able to expand
    }
}