
using System.Buffers;
using System.Collections.Generic;
using System.Linq;

namespace Sudoku
{
    internal class HiddenTupleStrategy : ISolveStrategy
    {
        public (bool, Puzzle) Apply(Puzzle puzzle)
        {
            var updatedCells = new List<Cell>();
            var regions = puzzle.Regions.ToArray();

            for (int tupleSize = 2; tupleSize < 5; tupleSize++)
            {
                var combinations = Helpers.GetCombinationIndices(Puzzle.LineLength, tupleSize);

                foreach (var region in regions)
                {
                    var (succes, newPuzzle) = FindHiddenTuple(updatedCells, puzzle, region, tupleSize, combinations);

                    if (succes)
                    {
                        return (true, newPuzzle);
                    }
                }
            }

            return (false, puzzle);
        }

        private (bool, Puzzle) FindHiddenTuple(List<Cell> updatedCells, Puzzle puzzle, Region region, int tupleSize, SudokuValues[] combinations)
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
                var options = ArrayPool<int>.Shared.Rent(9);
                var count = positions.AddIndices(options);
                var opposite = new SudokuValues(~comb.Values);

                for (var index = 0; index < count; index++)
                {
                    var cell = region[options[index]];

                    if (cell.HasOptions(opposite))
                    {
                        updatedCells.Add(cell.RemoveOptions(opposite));
                    }
                }

                ArrayPool<int>.Shared.Return(options);

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
