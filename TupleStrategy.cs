using System.Buffers;
using System.Collections.Generic;

namespace Sudoku
{
    /// <summary>
    /// Finds pairs, triples and quadruples of digits, removes those from other cells in the region
    /// note that quintuples and higher always correspond to one of the above tuples in the other cells
    /// </summary>
    internal class TupleStrategy : ISolveStrategy
    {
        public (bool, Puzzle) Apply(in Puzzle puzzle)
        {
            var updatedCells = new List<Cell>();
            var regions = puzzle.Regions;

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

            for (int j = 0; j < combinations.Length; j++)
            {
                var comb = combinations[j];

                if (placedDigits.HasAnyOptions(comb))
                {
                    // skip any tuples that include placed digits
                    continue;
                }

                var indices = ArrayPool<int>.Shared.Rent(tupleSize);
                var count = comb.CopyIndices(indices);
                var possibleValues = SudokuValues.None;

                for (int i = 0; i < count; i++)
                {
                    var cell = region[indices[i]];
                    possibleValues = possibleValues.AddOptions(cell.Value);
                }

                ArrayPool<int>.Shared.Return(indices);

                var optionsCount = possibleValues.GetOptionCount();

                if (optionsCount == tupleSize)
                {
                    // we found a tuple!

                    for (var i = 0; i < Puzzle.LineLength; i++)
                    {
                        var otherCell = region[i];

                        if (otherCell.IsResolved)
                        {
                            continue;
                        }

                        if (!otherCell.Value.HasAnyOptions(possibleValues))
                        {
                            continue;
                        }

                        if (comb.HasAnyOptions(SudokuValues.FromIndex(i)))
                        {
                            continue;
                        }

                        updatedCells.Add(otherCell.RemoveOptions(possibleValues));
                    }

                    if (updatedCells.Count > 0)
                    {
#if DEBUG
                        Program.DebugText = $"{possibleValues} tuple in {region}.";
#endif

                        return (true, puzzle.UpdateCells(updatedCells));
                    }
                }
            }

            return (false, puzzle);
        }
    }
}
