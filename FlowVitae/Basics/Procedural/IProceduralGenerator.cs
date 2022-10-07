namespace Venomaus.FlowVitae.Basics.Procedural
{
    /// <summary>
    /// Interface for procedural generation that can be supplied to <see cref="GridBase{TCellType, TCell}"/>
    /// </summary>
    /// <typeparam name="TCellType"></typeparam>
    /// <typeparam name="TCell"></typeparam>
    public interface IProceduralGen<TCellType, TCell>
        where TCellType : struct
        where TCell : class, ICell<TCellType>, new()
    {
        /// <summary>
        /// The procedural generation seed
        /// </summary>
        int Seed { get; }

        /// <summary>
        /// Generates procedural content based on <paramref name="seed"/> for an area of (<paramref name="width"/>,<paramref name="height"/>)
        /// </summary>
        /// <param name="seed">Seed used to generate chunks (based on <see cref="Seed"/>)</param>
        /// <param name="width">Area width</param>
        /// <param name="height">Area height</param>
        /// <returns><typeparamref name="TCellType"/>[width*height]</returns>
        TCellType[] Generate(int seed, int width, int height);
    }
}
