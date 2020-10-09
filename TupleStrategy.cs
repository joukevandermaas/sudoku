using System.Collections.Generic;

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
            for (int tupleSize = 2; tupleSize <= 4; tupleSize++)
            {
                var combinations = Helpers.GetCombinationIndices(Puzzle.LineLength, tupleSize);

                foreach (var region in puzzle.Regions)
                {
                    var (success, newPuzzle) = ScanRegion(puzzle, region, tupleSize, combinations);

                    if (success)
                    {
                        return (true, newPuzzle);
                    }
                }
            }

            return (false, puzzle);
        }

        private (bool, Puzzle) ScanRegion(Puzzle puzzle, Region region, int tupleSize, SudokuValues[] combinations)
        {
            for (int j = 0; j < combinations.Length; j++)
            {
                var comb = combinations[j];
                var indices = comb.ToIndices();

                if (region.AnyDigitPlaced(comb))
                {
                    // skip any tuples that include placed digits
                    continue;
                }

                var possibleValues = SudokuValues.None;

                foreach (var index in indices)
                {
                    var cell = region[index];
                    possibleValues = possibleValues.AddOptions(cell.Value);
                }

                var optionsCount = possibleValues.GetOptionCount();

                if (optionsCount == tupleSize)
                {
                    // we found a tuple!

                    var updatedCells = new List<Cell>();

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
