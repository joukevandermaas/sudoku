using System.Buffers;
using System.Collections.Generic;
using System.Linq;

namespace Sudoku
{
    /// <summary>
    /// Finds pairs, triples and quadruples of digits, removes those from other cells in the region
    /// note that quintuples and higher always correspond to one of the above tuples in the other cells
    /// </summary>
    internal class TupleStrategy : ISolveStrategy
    {
        public (bool, Puzzle) Apply(Puzzle puzzle)
        {
            var updatedCells = new List<Cell>();
            var regions = puzzle.Regions.ToArray();

            for (int tupleSize = 2; tupleSize <= 4; tupleSize++)
            {
                var combinations = Helpers.GetCombinationIndices(Puzzle.LineLength, tupleSize);

                foreach (var region in regions)
                {
                    var (success, newPuzzle) = ScanRegion(updatedCells, puzzle, region, tupleSize, combinations);

                    if (success)
                    {
                        return (true, newPuzzle);
                    }
                }
            }

            return (false, puzzle);
        }

        private (bool, Puzzle) ScanRegion(List<Cell> updatedCells, Puzzle puzzle, Region region, int tupleSize, SudokuValues[] combinations)
        {
            for (int j = 0; j < combinations.Length; j++)
            {
                var comb = combinations[j];

                if (region.AnyDigitPlaced(comb))
                {
                    // skip any tuples that include placed digits
                    continue;
                }

                var indices = ArrayPool<int>.Shared.Rent(tupleSize);
                var count = comb.AddIndices(indices);
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
                        Program.DebugText = $"{possibleValues} tuple in {region}.";
                        return (true, puzzle.UpdateCells(updatedCells));
                    }
                }
            }

            return (false, puzzle);
        }
    }
}
