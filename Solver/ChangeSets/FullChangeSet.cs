namespace Sudoku
{
    public readonly struct FullPuzzleChangeSet : IChangeSet
    {
        private readonly MutablePuzzle _puzzle;

        public FullPuzzleChangeSet(MutablePuzzle puzzle)
        {
            _puzzle = puzzle;
        }

        public bool IsEmpty => false;

        public void ApplyToPuzzle(MutablePuzzle puzzle)
        {
            puzzle.Absorb(_puzzle);
        }

        public void ApplyToRegionQueue(RegionQueue queue)
        {
            queue.EnqueueAll(RegionQueue.GetFull());
        }
    }
}
