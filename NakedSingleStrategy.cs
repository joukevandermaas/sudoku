using System;
using System.Collections.Generic;

namespace Sudoku
{
    /// <summary>
    /// Removes options based on placed digits.
    /// </summary>
    internal class NakedSingleStrategy : ISolveStrategy
    {
        public (bool, Puzzle) Apply(Puzzle puzzle)
        {
            return puzzle.ForEveryRegion(ScanRegion);
        }

        private static List<Cell> ScanRegion(Region region)
        {
            var result = new List<Cell>();

            var toRemove = SudokuValues.None;

            foreach (var cell in region)
            {
                if (cell.IsResolved)
                {
                    toRemove = toRemove.AddOptions(cell.Value);
                }
            }

            foreach (var cell in region)
            {
                if (!cell.IsResolved && cell.Value.HasAnyOptions(toRemove))
                {
                    var newCell = cell.RemoveOptions(toRemove);
                    result.Add(newCell);
                }
            }

            return result;
        }
    }
}
