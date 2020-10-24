namespace Sudoku
{

    public readonly struct EmptyChangeSet : IChangeSet
    {
        public bool IsEmpty => true;

        public void ApplyToPuzzle(MutablePuzzle puzzle)
        { }

        public void ApplyToRegionQueue(RegionQueue queue)
        { }
    }
}
