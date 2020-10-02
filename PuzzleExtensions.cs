using System;
using System.Collections.Generic;

namespace Sudoku
{
    internal static class PuzzleExtensions
    {
        public static (bool, Puzzle) ForEveryRegion(this Puzzle puzzle, Func<Region, IList<Cell>> action, bool stopAtSuccess = false)
        {
            var changedAny = false;

            foreach (var row in puzzle.GetRows())
            {
                UpdateIfNeeded(row);
            }

            if (changedAny && stopAtSuccess)
            {
                return (true, puzzle);
            }

            foreach (var col in puzzle.GetColumns())
            {
                UpdateIfNeeded(col);
            }

            if (changedAny && stopAtSuccess)
            {
                return (true, puzzle);
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
