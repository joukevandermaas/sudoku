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
        public void Apply(MutablePuzzle puzzle, RegionQueue changedRegions)
        {
            while (changedRegions.TryDequeueOfType(RegionType.Box, out int boxIndex))
            {
                var box = puzzle.Puzzle.Boxes[boxIndex];
                var placedDigits = box.GetPlacedDigits();

                for (int digit = 1; digit <= Puzzle.LineLength; digit++)
                {
                    var value = SudokuValues.FromHumanValue(digit);

                    if (placedDigits.HasAnyOptions(value))
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
                        var region = puzzle.Puzzle.Rows[rows.ToIndex()];
                        var anyChanged = RemoveFromOtherBoxesInRegion(puzzle, region, value, boxIndex);

                        if (anyChanged)
                        {
#if DEBUG
                            Program.Debugger.AddAction($"{digit}s in {box} remove others in {region}.");
#endif
                            return;
                        }
                    }
                    if (cols.IsSingle)
                    {
                        var region = puzzle.Puzzle.Columns[cols.ToIndex()];
                        var anyChanged = RemoveFromOtherBoxesInRegion(puzzle, region, value, boxIndex);

                        if (anyChanged)
                        {
#if DEBUG
                            Program.Debugger.AddAction($"{digit}s in {box} remove others in {region}.");
#endif
                            return;
                        }

                    }
                }
            }
        }

        private bool RemoveFromOtherBoxesInRegion(MutablePuzzle puzzle, Region region, SudokuValues value, int boxIndex)
        {
            var anyChanged = false;
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
                    puzzle.RemoveOptions(update);
                    anyChanged = true;
                }
            }

            return anyChanged;
        }

    }
}
