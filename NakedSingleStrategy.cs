using System;
using System.Collections.Generic;

namespace Sudoku
{
    internal class NakedSingleStrategy : ISolveStrategy
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
            var temporaryCellsToChange = new List<Cell>();

            foreach (var container in containers)
            {
                var toRemove = 0;

                foreach (var cell in container)
                {
                    if (cell.IsResolved)
                    {
                        toRemove |= cell.Value;
                    }
                    else
                    {
                        temporaryCellsToChange.Add(cell);
                    }
                }

                foreach (var cell in temporaryCellsToChange)
                {
                    if ((cell.Value & toRemove) > 0)
                    {
                        var newCell = cell.RemovePossibleValues(toRemove);

                        if (newCell.IsResolved)
                        {
                            Console.WriteLine("We placed a digit!");
                        }

                        updatedValues.Add(newCell);
                    }
                }

                temporaryCellsToChange.Clear();
            }
        }
    }
}
