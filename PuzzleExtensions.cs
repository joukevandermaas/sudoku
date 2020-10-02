using System;
using System.Collections.Generic;

namespace Sudoku
{
    internal static class PuzzleExtensions
    {
        public static (bool, Puzzle) ForEveryRegion(this Puzzle puzzle, Func<Region, IList<Cell>> action)
        {
            var changedAny = false;

            foreach (var row in puzzle.GetRows())
            {
                UpdateIfNeeded(row);
            }
            foreach (var col in puzzle.GetColumns())
            {
                UpdateIfNeeded(col);
            }
            foreach (var box in puzzle.GetBoxes())
            {
                UpdateIfNeeded(box);
            }

            return (changedAny, puzzle);

            void UpdateIfNeeded(Region region)
            {
                var changedCells = action(region);
                if (changedCells.Count > 0)
                {
                    changedAny = true;
                    puzzle = puzzle.UpdateCells(changedCells);
                }
            }
        }
    }
}
