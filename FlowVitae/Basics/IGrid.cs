namespace Venomaus.FlowVitae.Basics
{

    /// <summary>
    /// Interface which contains basic grid functionality
    /// </summary>
    /// <typeparam name="TCellType">The cell type to be stored within the grid layout</typeparam>
    /// <typeparam name="TCell">The wrapper object used to wrap around the cell type</typeparam>
    public interface IGrid<TCellType, TCell>
        where TCellType : struct
        where TCell : class, ICell<TCellType>, new()
    {
    }
}
