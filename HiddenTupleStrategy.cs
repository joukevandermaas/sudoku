
using System.Buffers;
using System.Collections.Generic;
using System.Linq;

namespace Sudoku
{
    internal class HiddenTupleStrategy : ISolveStrategy
    {
        public (bool, Puzzle) Apply(in Puzzle puzzle)
        {
            var updatedCells = new List<Cell>();
            var regions = puzzle.Regions.ToArray();

            foreach (var region in regions)
            {
                var placedDigits = region.GetPlacedDigits();

                for (int tupleSize = 2; tupleSize <= 4; tupleSize++)
                {
                    var (success, newPuzzle) = FindTuple(puzzle, updatedCells, region, placedDigits, tupleSize);

                    if (success)
                    {
                        return (true, newPuzzle);
                    }
                }
            }

            return (false, puzzle);
        }

        private static (bool, Puzzle) FindTuple(in Puzzle puzzle, List<Cell> updatedCells, Region region, SudokuValues placedDigits, int tupleSize)
        {
            var combinations = Helpers.GetCombinationIndices(Puzzle.LineLength, tupleSize);

            for (int i = 0; i < combinations.Length; i++)
            {
                var comb = combinations[i];

                if (placedDigits.HasAnyOptions(comb))
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
#if DEBUG
                    Program.DebugText = $"Hidden {comb} tuple in {region}";
#endif

                    var newPuzzle = puzzle.UpdateCells(updatedCells);
                    return (true, newPuzzle);
                }
            }

            return (false, puzzle);
        }
    }
}
