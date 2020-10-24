
namespace Sudoku
{
    public class GuessStrategy : ISolveStrategy
    {
        private Solver _solver;
        private int depth = 0;

        public GuessStrategy(Solver solver)
        {
            _solver = solver;
        }

        public void Apply(MutablePuzzle puzzle, RegionQueue changedRegions)
        {
            if (++depth > 10) return;

            while (changedRegions.TryDequeue(puzzle, out var region))
            {
                for (int i = 0; i < Puzzle.LineLength; i++)
                {
                    var value = region[i];
                    var coords = region.GetCoordinate(i);

                    if (value.GetOptionCount() != 2)
                    {
                        continue;
                    }

                    // if there are are only 2 bits set, bitwise and
                    // with the value - 1 will return the highest option
                    var highestValue = new SudokuValues(value.Values & (value.Values - 1));

#if DEBUG
                    Program.Debugger.AddAction($"Attempting {highestValue.Invert().Intersect(value)} in {coords}");
#endif

                    var oldSolver = _solver;
                    var changeset = new SingleCellChangeSet(new CellUpdate(highestValue, coords));
                    var newSolver = new Solver(changeset, _solver);

                    _solver = newSolver;

                    var (result, _) = newSolver.Solve();

                    _solver = oldSolver;

#if DEBUG
                    Program.Debugger.AddAction($"Result: {result}");
#endif

                    switch (result)
                    {
                        case SolveResult.Success:
                            puzzle.Absorb(newSolver.Puzzle);
                            return;
                        case SolveResult.Invalid:
                            // we picked the wrong digit
                            puzzle.RemoveOptions(new CellUpdate(highestValue.Invert(), coords));
                            return;
                    }
                }
            }
        }
    }
}