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
            var updatedValues = new List<Cell>();

            ScanContainers(puzzle.GetRows(), updatedValues);

            if (updatedValues.Count > 0)
            {
                return (true, puzzle.UpdateCells(updatedValues));
            }

            ScanContainers(puzzle.GetColumns(), updatedValues);

            if (updatedValues.Count > 0)
            {
                return (true, puzzle.UpdateCells(updatedValues));
            }

            ScanContainers(puzzle.GetBoxes(), updatedValues);

            if (updatedValues.Count > 0)
            {
                return (true, puzzle.UpdateCells(updatedValues));
            }

            return (false, puzzle);
        }

        private static void ScanContainers(IEnumerable<IEnumerable<Cell>> containers, List<Cell> updatedValues)
        {
            foreach (var container in containers)
            {
                var counts = new int[Puzzle.LineLength];
                var cells = new Cell[Puzzle.LineLength];

                foreach (var cell in container)
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
                        updatedValues.Add(newCell);
                    }
                }
            }
        }
    }
}
