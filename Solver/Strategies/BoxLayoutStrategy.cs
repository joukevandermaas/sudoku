using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Sudoku
{
    /// <summary>
    /// Finds cases where all options for a digit in a row/column are in the
    /// same box. Other occurences of that digit can be removed from the box.
    /// </summary>
    public class BoxLayoutStrategy : ISolveStrategy
    {
        private BasicChangeSet _updates = new BasicChangeSet();

        public IChangeSet Apply(in Puzzle puzzle, RegionQueue changedRegions, SudokuValues changedDigits)
        {
            _updates.Clear();

            while (changedRegions.TryDequeueOfType(RegionType.Box, out int boxIndex))
            {
                var box = puzzle.Boxes[boxIndex];

                for (int digit = 1; digit <= Puzzle.LineLength; digit++)
                {
                    var value = SudokuValues.FromHumanValue(digit);

                    if (!changedDigits.HasAnyOptions(value))
                    {
                        continue;
                    }

                    var rows = SudokuValues.None;
                    var cols = SudokuValues.None;

                    for (var i = 0; i < Puzzle.LineLength; i++)
                    {
                        var cellValue = box[i];
                        var cellCoord = box.GetCoordinate(i);

                        if (!cellValue.IsSingle && cellValue.HasAnyOptions(value))
                        {
                            rows = rows.AddOptions(SudokuValues.FromIndex(cellCoord.Row));
                            cols = cols.AddOptions(SudokuValues.FromIndex(cellCoord.Column));
                        }
                    }

                    if (rows.IsSingle)
                    {
                        var region = puzzle.Rows[rows.ToIndex()];
                        RemoveFromOtherBoxesInRegion(region, value, boxIndex);

                        if (!_updates.IsEmpty)
                        {
#if DEBUG
                            Program.HighlightDigit = digit;
                            Program.AddDebugText($"{digit}s in {box} remove others in {region}.");
#endif
                        }
                    }
                    if (cols.IsSingle)
                    {
                        var region = puzzle.Columns[cols.ToIndex()];
                        RemoveFromOtherBoxesInRegion(region, value, boxIndex);

                        if (!_updates.IsEmpty)
                        {
#if DEBUG
                            Program.HighlightDigit = digit;
                            Program.AddDebugText($"{digit}s in {box} remove others in {region}.");
#endif
                        }

                    }
                }
            }

            return _updates;
        }

        private void RemoveFromOtherBoxesInRegion(Region region, SudokuValues value, int boxIndex)
        {
            for (int i = 0; i < Puzzle.LineLength; i++)
            {
                var coords = region.GetCoordinate(i);

                if (coords.Box == boxIndex)
                {
                    continue;
                }

                var cell = region[i];

                if (!cell.IsSingle && cell.HasAnyOptions(value))
                {
                    var update = new CellUpdate(value, coords);
                    _updates.Add(update);
                }
            }
        }

    }
}
