using Venomaus.FlowVitae.Grids;
using Venomaus.FlowVitae.Procedural;

namespace Assets.Generation.Scripts
{
    public class FlowGrid : GridBase<int, FlowCell>
    {
        public FlowGrid(int width, int height) : base(width, height)
        { }

        public FlowGrid(int width, int height, int chunkWidth, int chunkHeight, IProceduralGen<int, FlowCell> generator) 
            : base(width, height, chunkWidth, chunkHeight, generator)
        {
            if (generator == null)
                throw new System.Exception("Invalid generator.");
        }

        // Custom impl, to be able to expand
    }
}