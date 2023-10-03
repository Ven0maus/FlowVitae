using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Venomaus.FlowVitae.Cells;
using Venomaus.FlowVitae.Chunking.Generators;
using Venomaus.FlowVitae.Grids;
using Venomaus.FlowVitae.Helpers;

namespace Venomaus.FlowVitae.Chunking
{
    internal class ChunkLoader<TCellType, TCell, TChunkData>
        where TCellType : struct
        where TCell : class, ICell<TCellType>, new()
        where TChunkData : class, IChunkData
    {
        private readonly int _seed;
        private readonly int _chunkWidth, _chunkHeight;
        private readonly int _viewPortWidth, _viewPortHeight;
        private readonly IProceduralGen<TCellType, TCell, TChunkData> _generator;
        private readonly Func<int, int, TCellType, TCell?> _cellTypeConverter;
        private readonly ConcurrentDictionary<(int x, int y), TChunkData> _chunkDataCache;
        private readonly ConcurrentDictionary<(int x, int y), (TCellType[] chunkCells, TChunkData? chunkData)> _chunks;
        private readonly ConcurrentHashSet<(int x, int y)> _tempLoadedChunks;
        private ConcurrentDictionary<(int x, int y), ConcurrentDictionary<(int x, int y), TCell>>? _modifiedCellsInChunks;

        public bool UseThreading { get; set; } = true;

        public (int x, int y) CurrentChunk { get; private set; }
        public (int x, int y) CenterCoordinate { get; private set; }
        public bool RaiseOnlyOnCellTypeChange { get; set; } = true;

        public bool LoadChunksInParallel { get; set; } = true;

        private readonly CancellationToken _cancellationToken;
        private readonly int _chunksOutsideViewportRadiusToLoad;

        public ChunkLoader(int viewPortWidth, int viewPortHeight, int width, int height, IProceduralGen<TCellType, TCell, TChunkData> generator, Func<int, int, TCellType, TCell?> cellTypeConverter, CancellationToken cancellationToken, int chunksOutsideViewportRadiusToLoad)
        {
            _chunkWidth = width;
            _chunkHeight = height;
            _viewPortWidth = viewPortWidth;
            _viewPortHeight = viewPortHeight;
            _seed = generator.Seed;
            _generator = generator;
            _cellTypeConverter = cellTypeConverter;
            _cancellationToken = cancellationToken;
            _tempLoadedChunks = new ConcurrentHashSet<(int x, int y)>(new TupleComparer<int>());
            var initialAmount = GetChunksToLoad(0, 0).AllChunks.Count;
            _chunks = new ConcurrentDictionary<(int x, int y), (TCellType[], TChunkData?)>(Environment.ProcessorCount, initialAmount, new TupleComparer<int>());
            _chunkDataCache = new ConcurrentDictionary<(int x, int y), TChunkData>(new TupleComparer<int>());
            _chunksOutsideViewportRadiusToLoad = chunksOutsideViewportRadiusToLoad;
        }

        public int GetChunkSeed(int x, int y)
        {
            var chunkCoordinate = GetChunkCoordinate(x, y);
            return Fnv1a.Hash32(chunkCoordinate.x, chunkCoordinate.y, _seed);
        }

        public bool IsChunkLoaded(int x, int y)
        {
            var chunkCoordinate = GetChunkCoordinate(x, y);
            return _chunks.ContainsKey(chunkCoordinate);
        }

        public void ClearGridCache()
        {
            _modifiedCellsInChunks = null;
            _chunkDataCache.Clear();
        }

        public ChunkLoadInformation GetChunksToLoad(int viewPortCenterX, int viewPortCenterY, int rangeOutsideViewport = 1)
        {
            int rangeX = Math.Max((_viewPortWidth / 2) / _chunkWidth + rangeOutsideViewport, 1); // Calculate range in X direction
            int rangeY = Math.Max((_viewPortHeight / 2) / _chunkHeight + rangeOutsideViewport, 1); // Calculate range in Y direction

            var chunksInsideViewport = new List<(int x, int y)>();
            var chunksOutsideViewport = new List<(int x, int y)>();

            for (int xOffset = -rangeX; xOffset <= rangeX; xOffset++)
            {
                for (int yOffset = -rangeY; yOffset <= rangeY; yOffset++)
                {
                    int newX = viewPortCenterX + xOffset * _chunkWidth;
                    int newY = viewPortCenterY + yOffset * _chunkHeight;

                    // Use the GetChunkCoordinate method to calculate the chunk coordinates
                    (int chunkX, int chunkY) = GetChunkCoordinate(newX, newY);

                    // Calculate the bounds of the current chunk
                    int chunkLeft = chunkX;
                    int chunkRight = chunkX + _chunkWidth;
                    int chunkTop = chunkY;
                    int chunkBottom = chunkY + _chunkHeight;

                    // Check if any part of the chunk is within the viewport
                    if (chunkRight >= 0 && chunkLeft <= _viewPortWidth &&
                        chunkBottom >= 0 && chunkTop <= _viewPortHeight)
                    {
                        // At least part of the chunk is inside the viewport
                        chunksInsideViewport.Add((chunkX, chunkY));
                    }
                    else
                    {
                        // The entire chunk is outside the viewport
                        chunksOutsideViewport.Add((chunkX, chunkY));
                    }
                }
            }

            return new ChunkLoadInformation(chunksInsideViewport, chunksOutsideViewport);
        }

        /// <summary>
        /// Remaps the cell coordinate to the Grid's coordinate
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public (int x, int y) RemapChunkCoordinate(int x, int y)
        {
            var chunkCoordinate = GetChunkCoordinate(x, y);
            return RemapChunkCoordinate(x, y, chunkCoordinate);
        }

        private static (int x, int y) RemapChunkCoordinate(int x, int y, (int x, int y) chunkCoordinate)
        {
            return (x: Math.Abs(x - chunkCoordinate.x), y: Math.Abs(y - chunkCoordinate.y));
        }

        public void SetCurrentChunk(int x, int y,
            EventHandler<ChunkUpdateArgs>? onChunkLoad = null, EventHandler<ChunkUpdateArgs>? onChunkUnload = null)
        {
            CurrentChunk = GetChunkCoordinate(x, y);
            var chunksToLoad = GetChunksToLoad(x, y, _chunksOutsideViewportRadiusToLoad);

            if (!UseThreading)
            {
                // Load chunks inside the viewport on the main thread
                var comparer = new TupleComparer<int>();
                foreach (var chunk in chunksToLoad.AllChunks)
                {
                    if (LoadChunk(chunk.x, chunk.y, out _))
                        onChunkLoad?.Invoke(null, new ChunkUpdateArgs(chunk, _chunkWidth, _chunkHeight, chunksToLoad.ChunksInsideViewport.Contains(chunk, comparer)));
                }

                // Unload chunks that are no longer needed
                foreach (var chunk in _chunks.Where(chunk => !chunksToLoad.AllChunks.Contains(chunk.Key)).ToList())
                {
                    if (_chunks.TryRemove(chunk.Key, out _))
                        onChunkUnload?.Invoke(null, new ChunkUpdateArgs(chunk.Key, _chunkWidth, _chunkHeight, false));
                }
            }
            else
            {
                var chunksInsideViewport = chunksToLoad.ChunksInsideViewport;
                var chunksOutsideViewport = chunksToLoad.ChunksOutsideViewport;

                // Load chunks inside the viewport on the main thread
                foreach (var chunk in chunksInsideViewport)
                {
                    if (LoadChunk(chunk.x, chunk.y, out _))
                        onChunkLoad?.Invoke(null, new ChunkUpdateArgs(chunk, _chunkWidth, _chunkHeight, true));
                }

                // Unload chunks that are no longer needed
                foreach (var chunk in _chunks.Where(chunk => !chunksToLoad.AllChunks.Contains(chunk.Key)).ToList())
                {
                    if (_chunks.TryRemove(chunk.Key, out _))
                        onChunkUnload?.Invoke(null, new ChunkUpdateArgs(chunk.Key, _chunkWidth, _chunkHeight, false));
                }

                // Load chunks outside the viewport on a background thread
                _ = Task.Run(() =>
                {
                    try
                    {
                        if (LoadChunksInParallel)
                        {
                            Parallel.ForEach(chunksOutsideViewport, (chunk) =>
                            {
                                try
                                {
                                    _cancellationToken.ThrowIfCancellationRequested();
                                    if (LoadChunk(chunk.x, chunk.y, out _))
                                    {
                                        onChunkLoad?.Invoke(null, new ChunkUpdateArgs(chunk, _chunkWidth, _chunkHeight, false));
                                    }
                                }
                                catch (Exception)
                                {
                                    throw;
                                }
                            });
                        }
                        else
                        {
                            try
                            {
                                foreach (var chunk in chunksOutsideViewport)
                                {
                                    _cancellationToken.ThrowIfCancellationRequested();
                                    if (LoadChunk(chunk.x, chunk.y, out _))
                                    {
                                        onChunkLoad?.Invoke(null, new ChunkUpdateArgs(chunk, _chunkWidth, _chunkHeight, false));
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                throw;
                            }
                        }
                    }
                    catch (AggregateException ex)
                    {
                        Task.Run(() =>
                        {
                            throw ex;
                        }, _cancellationToken);
                        return;
                    }
                }, _cancellationToken);
            }
        }

        public (int x, int y)[] GetLoadedChunks()
        {
            return _chunks.Keys.ToArray();
        }

        public TChunkData? GetChunkData(int x, int y)
        {
            if (_chunkDataCache.TryGetValue((x, y), out TChunkData? chunkData))
                return chunkData;

            bool wasLoaded = LoadChunk(x, y, out var chunkCoordinate);
            chunkData = _chunks[chunkCoordinate].chunkData;
            if (wasLoaded)
                UnloadChunk(chunkCoordinate.x, chunkCoordinate.y);
            return chunkData;
        }

        public void StoreChunkData(TChunkData chunkData)
        {
            if (_chunkDataCache.ContainsKey(chunkData.ChunkCoordinate))
                return;
            _chunkDataCache.TryAdd(chunkData.ChunkCoordinate, chunkData);
        }

        public void RemoveChunkData(TChunkData chunkData, bool reloadChunk)
        {
            _chunkDataCache.Remove(chunkData.ChunkCoordinate, out _);
            if (reloadChunk)
            {
                UnloadChunk(chunkData.ChunkCoordinate.x, chunkData.ChunkCoordinate.y, true);
                LoadChunk(chunkData.ChunkCoordinate.x, chunkData.ChunkCoordinate.y, out _);
            }
        }

        public TCellType GetChunkCellType(int x, int y, bool loadChunk = false, Checker? isWorldCoordinateOnScreen = null, TCellType[]? screenCells = null, bool unloadChunkAfterLoad = true, bool checkModifiedCells = true, bool forceLoadScreenCells = false)
        {
            var chunkCoordinate = GetChunkCoordinate(x, y);
            var remappedCoordinate = RemapChunkCoordinate(x, y, chunkCoordinate);

            // Check if there are modified cell tiles within this chunk
            if (checkModifiedCells && _modifiedCellsInChunks != null && _modifiedCellsInChunks
                .TryGetValue(chunkCoordinate, out var cells) &&
                cells.TryGetValue(remappedCoordinate, out var cell))
            {
                // Return the modified cell if it exists
                return cell.CellType;
            }

            // Check if coordinate is within viewport
            var chunk = GetChunk(x, y, out _);
            if (isWorldCoordinateOnScreen != null && screenCells != null &&
                isWorldCoordinateOnScreen.Invoke(x, y, out (int x, int y)? screenCoordinate, out var screenWidth) &&
                screenCoordinate != null)
            {
                if (chunk != null && chunk.Value.chunkCells != null)
                {
                    // Adjust screen cell if it doesn't match with chunk cell && no modified cell was stored
                    var chunkCells = chunk.Value.chunkCells;
                    var screenCell = screenCells[screenCoordinate.Value.y * screenWidth + screenCoordinate.Value.x];
                    var chunkCell = chunkCells[remappedCoordinate.y * _chunkWidth + remappedCoordinate.x];
                    if (!screenCell.Equals(chunkCell))
                    {
                        screenCells[screenCoordinate.Value.y * screenWidth + screenCoordinate.Value.x] = chunkCell;
                    }
                }
                else if (forceLoadScreenCells)
                {
                    var screenCell = screenCells[screenCoordinate.Value.y * screenWidth + screenCoordinate.Value.x];
                    var chunkCell = GetCellTypeWithLoadOption(x, y, true, unloadChunkAfterLoad, chunkCoordinate, remappedCoordinate, out _);
                    if (!screenCell.Equals(chunkCell))
                    {
                        screenCells[screenCoordinate.Value.y * screenWidth + screenCoordinate.Value.x] = chunkCell;
                    }
                }

                return screenCells[screenCoordinate.Value.y * screenWidth + screenCoordinate.Value.x];
            }

            return GetCellTypeWithLoadOption(x, y, loadChunk, unloadChunkAfterLoad, chunkCoordinate, remappedCoordinate, out _);
        }

        private TCellType GetCellTypeWithLoadOption(int x, int y, bool loadChunk, bool unloadChunkAfterLoad, (int x, int y) chunkCoordinate, (int x, int y) remappedCoordinate, out (TCellType[]? chunkCells, TChunkData? chunkData)? chunk)
        {
            bool wasChunkLoaded = false;
            if (loadChunk)
                wasChunkLoaded = LoadChunk(x, y, out _);

            // Load chunk after all other options are validated
            chunk = GetChunk(x, y, out _);
            if (chunk == null || chunk.Value.chunkCells == null)
                throw new Exception("Something went wrong during chunk retrieval.");

            if (loadChunk && wasChunkLoaded && unloadChunkAfterLoad)
                UnloadChunk(x, y);
            else if (loadChunk && wasChunkLoaded && !unloadChunkAfterLoad)
                _tempLoadedChunks.Add(chunkCoordinate);

            // Return the non-modified cell
            return chunk.Value.chunkCells[remappedCoordinate.y * _chunkWidth + remappedCoordinate.x];
        }

        public TCell? GetChunkCell(int x, int y, bool loadChunk = false, Checker? isWorldCoordinateOnScreen = null, TCellType[]? screenCells = null, bool unloadChunkAfterLoad = true, bool forceLoadScreenCells = false)
        {
            var chunkCoordinate = GetChunkCoordinate(x, y);
            var remappedCoordinate = RemapChunkCoordinate(x, y, chunkCoordinate);

            // Check if there are modified cell tiles within this chunk
            if (_modifiedCellsInChunks != null && _modifiedCellsInChunks
                .TryGetValue(chunkCoordinate, out var cells) &&
                cells.TryGetValue(remappedCoordinate, out var cell))
            {
                // Return the modified cell if it exists
                return cell;
            }

            return _cellTypeConverter(x, y, GetChunkCellType(x, y, loadChunk, isWorldCoordinateOnScreen, screenCells, unloadChunkAfterLoad, false, forceLoadScreenCells));
        }

        public IEnumerable<TCell?> GetChunkCells(IEnumerable<(int x, int y)> positions, Checker? isWorldCoordinateOnScreen = null, TCellType[]? screenCells = null, bool forceLoadScreenCells = false)
        {
            var coords = positions.ToArray();
            if (!UseThreading)
            {
                foreach (var (x, y) in coords)
                {
                    yield return GetChunkCell(x, y, true, isWorldCoordinateOnScreen, screenCells, false, forceLoadScreenCells);
                }
            }
            else
            {
                if (LoadChunksInParallel)
                {
                    var cellDictionary = new ConcurrentDictionary<(int x, int y), TCell?>();
                    Parallel.ForEach(coords, pos =>
                    {
                        cellDictionary.TryAdd(pos, GetChunkCell(pos.x, pos.y, true, isWorldCoordinateOnScreen, screenCells, false, forceLoadScreenCells));
                    });

                    // Return cells in the same order as the input order
                    foreach (var pos in coords)
                    {
                        if (cellDictionary.TryGetValue(pos, out var cell))
                            yield return cell;
                    }
                }
                else
                {
                    foreach (var (x, y) in coords)
                    {
                        yield return GetChunkCell(x, y, true, isWorldCoordinateOnScreen, screenCells, false, forceLoadScreenCells);
                    }
                }
            }

            foreach (var (x, y) in _tempLoadedChunks)
                UnloadChunk(x, y);
            _tempLoadedChunks.Clear();
        }

        public bool HasStoredCell(int x, int y)
        {
            var chunkCoordinate = GetChunkCoordinate(x, y);
            var remappedCoordinate = RemapChunkCoordinate(x, y, chunkCoordinate);
            if (_modifiedCellsInChunks != null && _modifiedCellsInChunks.TryGetValue(chunkCoordinate, out var storedCells))
            {
                return storedCells.ContainsKey(remappedCoordinate);
            }
            return false;
        }

        public void SetChunkCell(TCell cell, bool storeState = false,
            EventHandler<CellUpdateArgs<TCellType, TCell>>? onCellUpdate = null,
            Checker? isWorldCoordinateOnScreen = null,
            TCellType[]? screenCells = null)
        {
            var chunkCoordinate = GetChunkCoordinate(cell.X, cell.Y);
            var remappedCoordinate = RemapChunkCoordinate(cell.X, cell.Y, chunkCoordinate);

            if (!storeState && _modifiedCellsInChunks != null && _modifiedCellsInChunks.TryGetValue(chunkCoordinate, out var storedCells))
            {
                storedCells.Remove(remappedCoordinate, out _);
                if (storedCells.Count == 0)
                    _modifiedCellsInChunks.Remove(chunkCoordinate, out _);
                if (_modifiedCellsInChunks.Count == 0)
                    _modifiedCellsInChunks = null;
            }
            else if (storeState)
            {
                // Check if there are modified cell tiles within this chunk
                if (_modifiedCellsInChunks == null)
                    _modifiedCellsInChunks = new ConcurrentDictionary<(int x, int y), ConcurrentDictionary<(int x, int y), TCell>>(new TupleComparer<int>());
                if (!_modifiedCellsInChunks.TryGetValue(chunkCoordinate, out storedCells))
                {
                    storedCells = new ConcurrentDictionary<(int x, int y), TCell>(new TupleComparer<int>());
                    _modifiedCellsInChunks.TryAdd(chunkCoordinate, storedCells);
                }
                storedCells[remappedCoordinate] = cell;
            }

            if (isWorldCoordinateOnScreen != null && screenCells != null &&
                isWorldCoordinateOnScreen(cell.X, cell.Y, out var screenCoordinate, out var screenWidth)
                && screenCoordinate != null)
            {
                var prev = screenCells[screenCoordinate.Value.y * screenWidth + screenCoordinate.Value.x];
                screenCells[screenCoordinate.Value.y * screenWidth + screenCoordinate.Value.x] = cell.CellType;

                if (!RaiseOnlyOnCellTypeChange || !prev.Equals(cell.CellType))
                    onCellUpdate?.Invoke(null, new CellUpdateArgs<TCellType, TCell>(screenCoordinate.Value, cell));

                var chunk = GetChunk(cell.X, cell.Y, out _);
                if (chunk?.chunkCells != null)
                {
                    chunk.Value.chunkCells[remappedCoordinate.y * _chunkWidth + remappedCoordinate.x] = cell.CellType;
                }
            }
            else if (IsChunkLoaded(chunkCoordinate.x, chunkCoordinate.y))
            {
                var (chunkCells, _) = _chunks[chunkCoordinate];
                if (chunkCells != null)
                {
                    chunkCells[remappedCoordinate.y * _chunkWidth + remappedCoordinate.x] = cell.CellType;
                }
            }
        }

        public delegate bool Checker(int x, int y, out (int x, int y)? coordinate, out int screenWidth);
        public void SetChunkCells(IEnumerable<TCell?> cells, Func<TCell?, bool>? storeCellStateFunc = null, EventHandler<CellUpdateArgs<TCellType, TCell>>? onCellUpdate = null, Checker? isWorldCoordinateOnScreen = null, TCellType[]? screenCells = null)
        {
            foreach (var cell in cells.Where(a => a != null).Cast<TCell>())
                SetChunkCell(cell, storeCellStateFunc?.Invoke(cell) ?? false, onCellUpdate: onCellUpdate, isWorldCoordinateOnScreen: isWorldCoordinateOnScreen, screenCells: screenCells);
        }

        private readonly object loadChunkLock = new object();
        public bool LoadChunk(int x, int y, out (int x, int y) chunkCoordinate)
        {
            chunkCoordinate = GetChunkCoordinate(x, y);

            if (!_chunks.ContainsKey(chunkCoordinate) ||
                _chunks[chunkCoordinate].chunkCells == null)
            {
                lock (loadChunkLock)
                {
                    if (_cancellationToken.IsCancellationRequested) return false;
                    // Recheck the condition inside the lock
                    if (!_chunks.ContainsKey(chunkCoordinate) ||
                        _chunks[chunkCoordinate].chunkCells == null)
                    {
                        GenerateChunk(chunkCoordinate);
                        return true;
                    }
                }
            }
            return false;
        }

        private readonly object unloadChunkLock = new object();
        public bool UnloadChunk(int x, int y, bool forceUnload = false)
        {
            var coordinate = GetChunkCoordinate(x, y);

            if (!_chunks.ContainsKey(coordinate))
            {
                return false; // The chunk doesn't exist, nothing to unload.
            }

            if (!forceUnload)
            {
                // Acquire a lock to ensure that the check and removal are atomic.
                lock (unloadChunkLock)
                {
                    if (!_chunks.ContainsKey(coordinate))
                    {
                        return false; // The chunk doesn't exist, nothing to unload.
                    }

                    // Check that we are not unloading current or neighbor chunks
                    var chunksToLoad = GetChunksToLoad(CenterCoordinate.x, CenterCoordinate.y, _chunksOutsideViewportRadiusToLoad);
                    if (chunksToLoad.AllChunks.Any(m => m.x == coordinate.x && m.y == coordinate.y))
                    {
                        return false; // Don't unload this chunk.
                    }
                    else
                    {
                        _chunks.Remove(coordinate, out _);
                        return true; // Unloaded successfully.
                    }
                }
            }
            else
            {
                // Unload the chunk without performing the check since forceUnload is true.
                _chunks.Remove(coordinate, out _);
                return true;
            }
        }

        /// <summary>
        /// Returns the base coordinate of the cell's chunk
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public (int x, int y) GetChunkCoordinate(int x, int y)
        {
            return GetCoordinateBySizeNoConversion(x, y, _chunkWidth, _chunkHeight);
        }

        public void CenterViewPort(int x, int y, int viewPortWidth, int viewPortHeight, (int x, int y) prevCenterCoordinate,
            (int x, int y) centerCoordinate, Checker isWorldCoordinateOnScreen, TCellType[] screenCells,
            EventHandler<CellUpdateArgs<TCellType, TCell>>? onCellUpdate, bool viewPortInitialized,
            EventHandler<ChunkUpdateArgs>? onChunkLoad, EventHandler<ChunkUpdateArgs>? onChunkUnload)
        {
            CenterCoordinate = centerCoordinate;

            var minX = x - (viewPortWidth / 2);
            var minY = y - (viewPortHeight / 2);

            // Update the current chunk only if the center isn't the current
            var centerChunk = GetChunkCoordinate(x, y);
            if (CurrentChunk.x != centerChunk.x || CurrentChunk.y != centerChunk.y)
                SetCurrentChunk(centerChunk.x, centerChunk.y, onChunkLoad, onChunkUnload);

            var diffX = -(centerCoordinate.x - prevCenterCoordinate.x);
            var diffY = -(centerCoordinate.y - prevCenterCoordinate.y);

            // This basically shifts the cells to the opposite direction as to where you are centering from->to.
            // Eg center from left to right, it will shift cells left
            // The inverse loop here makes sure that when going from 0,0 to right, it won't copy over the same value
            // If diffX or diffY > 0 then inverse the loop
            if (diffX >= 1 && diffY <= -1)
            {
                for (var xX = viewPortWidth - 1; xX >= 0; xX--)
                    for (var yY = 0; yY < viewPortHeight; yY++)
                        SyncViewportCellOnCenter(minX, minY, xX, yY, viewPortWidth, viewPortHeight,
                            centerCoordinate, diffX, diffY, viewPortInitialized, isWorldCoordinateOnScreen,
                            screenCells, onCellUpdate);
            }
            else if (diffX <= -1 && diffY >= 1)
            {
                for (var xX = 0; xX < viewPortWidth; xX++)
                    for (var yY = viewPortHeight - 1; yY >= 0; yY--)
                        SyncViewportCellOnCenter(minX, minY, xX, yY, viewPortWidth, viewPortHeight,
                            centerCoordinate, diffX, diffY, viewPortInitialized, isWorldCoordinateOnScreen,
                            screenCells, onCellUpdate);
            }
            else if (diffX > 0 || diffY > 0)
            {
                for (var xX = viewPortWidth - 1; xX >= 0; xX--)
                    for (var yY = viewPortHeight - 1; yY >= 0; yY--)
                        SyncViewportCellOnCenter(minX, minY, xX, yY, viewPortWidth, viewPortHeight,
                            centerCoordinate, diffX, diffY, viewPortInitialized, isWorldCoordinateOnScreen,
                            screenCells, onCellUpdate);
            }
            else if (diffX < 0 || diffY < 0)
            {
                for (var xX = 0; xX < viewPortWidth; xX++)
                    for (var yY = 0; yY < viewPortHeight; yY++)
                        SyncViewportCellOnCenter(minX, minY, xX, yY, viewPortWidth, viewPortHeight,
                            centerCoordinate, diffX, diffY, viewPortInitialized, isWorldCoordinateOnScreen,
                            screenCells, onCellUpdate);
            }

            foreach (var chunk in _tempLoadedChunks)
                UnloadChunk(chunk.x, chunk.y);
            _tempLoadedChunks.Clear();
        }

        private void SyncViewportCellOnCenter(int minX, int minY, int xX, int yY, int viewPortWidth, int viewPortHeight,
            (int x, int y) centerCoordinate, int diffX, int diffY, bool viewPortInitialized, Checker isWorldCoordinateOnScreen,
            TCellType[] screenCells, EventHandler<CellUpdateArgs<TCellType, TCell>>? onCellUpdate)
        {
            var cellX = minX + xX;
            var cellY = minY + yY;
            var newScreenCoordinate = WorldToScreenCoordinate(cellX, cellY, viewPortWidth, viewPortHeight, centerCoordinate);

            var prevValue = (x: cellX - diffX, y: cellY - diffY);
            var prevScreenCoord = WorldToScreenCoordinate(prevValue.x, prevValue.y, viewPortWidth, viewPortHeight, centerCoordinate);
            if (isWorldCoordinateOnScreen.Invoke(prevValue.x, prevValue.y, out _, out _) && viewPortInitialized)
            {
                var prevScreenType = screenCells[prevScreenCoord.y * viewPortWidth + prevScreenCoord.x];

                // Update viewport cell with previous cell
                screenCells[newScreenCoordinate.y * viewPortWidth + newScreenCoordinate.x] = prevScreenType;
                onCellUpdate?.Invoke(null, new CellUpdateArgs<TCellType, TCell>(newScreenCoordinate, _cellTypeConverter(cellX, cellY, prevScreenType)));
                return;
            }

            if (LoadChunk(cellX, cellY, out var chunkCoordinate))
                _tempLoadedChunks.Add(chunkCoordinate);

            var cellType = GetChunkCellType(cellX, cellY, false, isWorldCoordinateOnScreen, screenCells);
            screenCells[newScreenCoordinate.y * viewPortWidth + newScreenCoordinate.x] = cellType;
            onCellUpdate?.Invoke(null, new CellUpdateArgs<TCellType, TCell>(newScreenCoordinate, _cellTypeConverter(cellX, cellY, cellType)));
        }

        public static (int x, int y) WorldToScreenCoordinate(int x, int y, int viewPortWidth, int viewPortHeight, (int x, int y) centerCoordinate)
        {
            var halfCenterX = centerCoordinate.x - (viewPortWidth / 2);
            var halfCenterY = centerCoordinate.y - (viewPortHeight / 2);
            var modifiedPos = (x: x - halfCenterX, y: y - halfCenterY);
            return modifiedPos;
        }

        private (TCellType[]? chunkCells, TChunkData? chunkData)? GetChunk(int x, int y, out (int x, int y) chunkCoordinate)
        {
            chunkCoordinate = GetChunkCoordinate(x, y);
            // Cast required
            return _chunks.TryGetValue(chunkCoordinate, out var chunk) ? ((TCellType[]? chunkCells, TChunkData? chunkData)?)chunk : null;
        }

        private (TCellType[] chunkCells, TChunkData? chunkData) GenerateChunk((int x, int y) coordinate)
        {
            // Get a unique hash seed based on the chunk (x,y) and the main seed
            var chunkSeed = Fnv1a.Hash32(coordinate.x, coordinate.y, _seed);
            var chunk = _generator.Generate(chunkSeed, _chunkWidth, _chunkHeight, coordinate);

            if (chunk.chunkCells == null)
                throw new Exception("Generated chunk cannot be null.");
            else if (chunk.chunkCells.Length != (_chunkWidth * _chunkHeight))
                throw new Exception("Generated chunk length does not match the provided width and height.");

            // Update chunk data with the cached version
            if (_chunkDataCache.TryGetValue(coordinate, out var chunkData))
                chunk.chunkData = chunkData;

            // Set passed down chunk info
            if (chunk.chunkData != null)
            {
                chunk.chunkData.Seed = chunkSeed;
                chunk.chunkData.ChunkCoordinate = coordinate;
            }

            _chunks.AddOrUpdate(coordinate, (a) => chunk, (key, oldValue) => chunk);
            return chunk;
        }

        private static (int x, int y) GetCoordinateBySizeNoConversion(int x, int y, int width, int height)
        {
            if (x < 0 && x % width != 0) x -= width;
            if (y < 0 && y % height != 0) y -= height;
            var chunkX = width * (x / width);
            var chunkY = height * (y / height);
            return (chunkX, chunkY);
        }
    }
}
