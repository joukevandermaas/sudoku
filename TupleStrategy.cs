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
            var combinations = GetCombinationIndices(tupleSize);

            foreach (var combination in combinations)
            {
                if (combination.Select(i => region[i]).Any(c => c.IsResolved))
                { 
                    // skip any combinations of cells that are already resolved
                    continue;
                }

                var possibleValues = SudokuValues.None;

                foreach (var index in combination)
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

        private static IEnumerable<int[]> GetCombinationIndices(int tupleSize)
        {
            return GetCombinationsHelper(0, new int[0], tupleSize);
        }

        private static IEnumerable<int[]> GetCombinationsHelper(int startDigit, int[] previousDigits, int tupleSize)
        {
            for (int i = startDigit; i < Puzzle.LineLength; i++)
            {
                var result = previousDigits.Concat(new[] { i }).ToArray();

                if (result.Length == tupleSize)
                {
                    yield return result;
                }
                else
                {
                    foreach (var tuple in GetCombinationsHelper(i + 1, result, tupleSize))
                    {
                        yield return tuple;
                    }
                }
            }
        }
    }
}
