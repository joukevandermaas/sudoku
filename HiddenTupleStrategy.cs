
using System.Collections.Generic;

namespace Sudoku
{
    internal class HiddenTupleStrategy : ISolveStrategy
    {
        public (bool, Puzzle) Apply(Puzzle puzzle)
        {
            for (int tupleSize = 2; tupleSize < 5; tupleSize++)
            {
                var combinations = Helpers.GetCombinationIndices(Puzzle.LineLength, tupleSize);

                foreach (var region in puzzle.Regions)
                {
                    var (succes, newPuzzle) = FindHiddenTuple(puzzle, region, tupleSize, combinations);

                    if (succes)
                    {
                        return (true, newPuzzle);
                    }
                }
            }

            return (false, puzzle);
        }

        private (bool, Puzzle) FindHiddenTuple(Puzzle puzzle, Region region, int tupleSize, SudokuValues[] combinations)
        {
            for (int i = 0; i < combinations.Length; i++)
            {
                var comb = combinations[i];

                if (region.AnyDigitPlaced(comb))
                {
                    // skip any tuples that include placed digits
                    continue;
                }

                var positions = region.GetPositions(comb);

                if (positions.GetOptionCount() != tupleSize)
                {
                    // these digits are placeable in more than tupleSize spots, so not hidden tuple
                    continue;
                }

                // we found a hidden tuple!
                var updatedCells = new List<Cell>();
                var options = positions.ToHumanOptions();
                var opposite = new SudokuValues(~comb.Values);

                foreach (var opt in options)
                {
                    var cell = region[opt - 1];

                    if (cell.HasOptions(opposite))
                    {
                        updatedCells.Add(cell.RemoveOptions(opposite));
                    }
                }

                if (updatedCells.Count > 0)
                {
                    Program.DebugText = $"Hidden {comb} tuple in {region}";

                    var newPuzzle = puzzle.UpdateCells(updatedCells);
                    return (true, newPuzzle);
                }
            }

            return (false, puzzle);
        }

    }
}
