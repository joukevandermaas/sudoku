using System;
using System.Collections.Generic;

namespace Sudoku
{
    /// <summary>
    /// Detects places where a value can only be in one cell in a given box/column/row,
    /// places that digit there.
    /// </summary>
    internal class SinglePositionStrategy : ISolveStrategy
    {
        public (bool, Puzzle) Apply(Puzzle puzzle)
        {
            return puzzle.ForEveryRegion(ScanRegion, stopAtSuccess: true);
        }

        private static List<Cell> ScanRegion(Region region)
        {
            var counts = new int[Puzzle.LineLength];
            var cells = new Cell[Puzzle.LineLength];
            var result = new List<Cell>();

            foreach (var cell in region)
            {
                if (cell.IsResolved)
                {
                    continue;
                }

                var options = cell.Value.ToHumanOptions();

                foreach (var option in options)
                {
                    counts[option - 1] += 1;
                    cells[option - 1] = cell;
                }
            }

            for (var i = 0; i < counts.Length; i++)
            {
                if (counts[i] == 1)
                {
                    var newCell = cells[i].SetValue(SudokuValues.FromHumanValue(i + 1));
                    result.Add(newCell);
                }
            }

            return result;
        }
    }
}
