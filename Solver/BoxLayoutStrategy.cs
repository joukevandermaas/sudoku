using System.Collections.Generic;

namespace Sudoku
{
    /// <summary>
    /// Finds cases where all options for a digit in a row/column are in the
    /// same box. Other occurences of that digit can be removed from the box.
    /// </summary>
    internal class BoxLayoutStrategy : ISolveStrategy
    {
        public (bool, Puzzle) Apply(in Puzzle puzzle)
        {
            var updates = new List<Cell>(Puzzle.LineLength);

            foreach (var box in puzzle.Boxes)
            {
                var boxIndex = box[0].Box;

                for (int digit = 1; digit <= Puzzle.LineLength; digit++)
                {
                    var value = SudokuValues.FromHumanValue(digit);
                    var rows = SudokuValues.None;
                    var cols = SudokuValues.None;

                    for (var i = 0; i < Puzzle.LineLength; i++)
                    {
                        var cell = box[i];

                        if (cell.HasOptions(value))
                        {
                            rows = rows.AddOptions(SudokuValues.FromIndex(cell.Row));
                            cols = cols.AddOptions(SudokuValues.FromIndex(cell.Column));
                        }
                    }

                    if (rows.IsSingle)
                    {
                        var region = puzzle.Rows[rows.ToIndex()];
                        var (success, newPuzzle) = RemoveFromOtherBoxesInRegion(updates, puzzle, region, value, boxIndex);

                        if (success)
                        {
#if DEBUG
                            Program.HighlightDigit = digit;
                            Program.DebugText = $"{digit}s in {box} remove others in {region}.";
#endif

                            return (true, newPuzzle);
                        }
                    }
                    if (cols.IsSingle)
                    {
                        var region = puzzle.Columns[cols.ToIndex()];
                        var (success, newPuzzle) = RemoveFromOtherBoxesInRegion(updates, puzzle, region, value, boxIndex);

                        if (success)
                        {
#if DEBUG
                            Program.HighlightDigit = digit;
                            Program.DebugText = $"{digit}s in {box} remove others in {region}.";
#endif

                            return (true, newPuzzle);
                        }

                    }
                }
            }

            return (false, puzzle);
        }

        private (bool, Puzzle) RemoveFromOtherBoxesInRegion(List<Cell> updates, in Puzzle puzzle, Region region, SudokuValues value, int boxIndex)
        {
            for (int i = 0; i < Puzzle.LineLength; i++)
            {
                var cell = region[i];
                if (cell.Box == boxIndex)
                {
                    continue;
                }

                if (cell.HasOptions(value))
                {
                    updates.Add(cell.RemoveOptions(value));
                }
            }

            if (updates.Count > 0)
            {
                return (true, puzzle.UpdateCells(updates));
            }

            return (false, puzzle);
        }

    }
}
