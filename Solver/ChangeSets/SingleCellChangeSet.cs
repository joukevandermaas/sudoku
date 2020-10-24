namespace Sudoku
{

    public readonly struct SingleCellChangeSet : IChangeSet
    {
        private readonly CellUpdate _update;

        public SingleCellChangeSet(CellUpdate update)
        {
            _update = update;
        }

        public bool IsEmpty => false;

        public void ApplyToPuzzle(MutablePuzzle puzzle)
        {
            puzzle.RemoveOptions(_update);
        }

        public void ApplyToRegionQueue(RegionQueue queue)
        {
            queue.Enqueue(RegionType.Row, _update.Coordinate.Row);
            queue.Enqueue(RegionType.Column, _update.Coordinate.Column);
            queue.Enqueue(RegionType.Box, _update.Coordinate.Box);
        }
    }
}
