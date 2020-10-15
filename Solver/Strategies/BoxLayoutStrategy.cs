using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Sudoku
{
    /// <summary>
    /// Finds cases where all options for a digit in a row/column are in the
    /// same box. Other occurences of that digit can be removed from the box.
    /// </summary>
    public class BoxLayoutStrategy : ISolveStrategy
    {
        public ChangeSet Apply(in Puzzle puzzle, RegionQueue unprocessedRegions)
        {
            var updates = new List<Cell>(Puzzle.LineLength);

            while (unprocessedRegions.TryDequeueOfType(RegionType.Box, out int boxIndex))
            {
                var box = puzzle.Boxes[boxIndex];

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
                        RemoveFromOtherBoxesInRegion(updates, region, value, boxIndex);

                        if (updates.Count > 0)
                        {
#if DEBUG
                            Program.HighlightDigit = digit;
                            Program.AddDebugText($"{digit}s in {box} remove others in {region}.");
#endif

                            return new ChangeSet(updates);
                        }
                    }
                    if (cols.IsSingle)
                    {
                        var region = puzzle.Columns[cols.ToIndex()];
                        RemoveFromOtherBoxesInRegion(updates, region, value, boxIndex);

                        if (updates.Count > 0)
                        {
#if DEBUG
                            Program.HighlightDigit = digit;
                            Program.AddDebugText($"{digit}s in {box} remove others in {region}.");
#endif
                            return new ChangeSet(updates);
                        }

                    }
                }
            }

            return ChangeSet.Empty;
        }

        private void RemoveFromOtherBoxesInRegion(List<Cell> updates, Region region, SudokuValues value, int boxIndex)
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
        }

    }
}
