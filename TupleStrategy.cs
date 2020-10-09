using System;
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
            for (int tupleSize = 2; tupleSize <= 4; tupleSize++)
            {
                foreach (var region in puzzle.Regions)
                {
                    var (success, newPuzzle) = ScanRegion(puzzle, region, tupleSize);

                    if (success)
                    {
                        return (true, newPuzzle);
                    }
                }
            }

            return (false, puzzle);
        }

        private (bool, Puzzle) ScanRegion(Puzzle puzzle, Region region, int tupleSize)
        {
            var combinations = Helpers.GetCombinationIndices(Puzzle.LineLength, tupleSize);

            foreach (var combination in combinations)
            {
                if (combination.Select(i => region[i]).Any(c => c.IsResolved))
                { 
                    // skip any combinations of cells that are already resolved
                    continue;
                }

                var possibleValues = SudokuValues.None;

                for (var i = 0; i < tupleSize; i++)
                {
                    var index = combination[i];
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

                        if (otherCell.IsResolved || combination.Contains(i))
                        {
                            continue;
                        }

                        if (!otherCell.Value.HasAnyOptions(possibleValues))
                        {
                            continue;
                        }

                        updatedCells.Add(otherCell.RemoveOptions(possibleValues));
                    }

                    if (updatedCells.Any())
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
