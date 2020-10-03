﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Sudoku
{
    /// <summary>
    /// Finds singles, pairs, triples and quadruples of digits, removes those from other cells in the region
    /// note that quintuples and higher always correspond to one of the above tuples in the other cells
    /// </summary>
    internal class TupleStrategy : ISolveStrategy
    {
        public (bool, Puzzle) Apply(Puzzle puzzle)
        {
            // we copy the cells so we can mutate the array directly
            var cells = puzzle.Cells.ToArray();

            var anySuccess = false;
            for (int tupleSize = 1; tupleSize <= 4; tupleSize++)
            {
                for (int i = 0; i < Puzzle.LineLength; i++)
                {
                    var row = new Region(cells, RegionType.Row, i);
                    anySuccess = ScanRegion(cells, row, tupleSize) || anySuccess;

                    var col = new Region(cells, RegionType.Column, i);
                    anySuccess = ScanRegion(cells, col, tupleSize) || anySuccess;

                    var box = new Region(cells, RegionType.Box, i);
                    anySuccess = ScanRegion(cells, box, tupleSize) || anySuccess;
                }
            }

            puzzle = new Puzzle(cells);

            return (anySuccess, puzzle);
        }

        private static bool ScanRegion(Cell[] cells, Region region, int tupleSize)
        {
            var changedAnyCells = false;

            var combinations = GetCombinationIndices(tupleSize);

            foreach (var combination in combinations)
            {
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

                        cells[otherCell.Index] = otherCell.RemoveOptions(possibleValues);
                        changedAnyCells = true;
                    }
                }
            }

            return changedAnyCells;
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
