using System;
using System.Linq;

namespace Sudoku
{
    internal class SingleStrategy : ISolveStrategy
    {
        public (bool, Puzzle) Apply(in Puzzle puzzle)
        {
            var cells = puzzle.Cells.ToArray();
            var regions = new RegionQueue();

            for (int i = 0; i < Puzzle.LineLength; i++)
            {
                regions.Enqueue(new Region(cells, RegionType.Row, i));
                regions.Enqueue(new Region(cells, RegionType.Column, i));
                regions.Enqueue(new Region(cells, RegionType.Box, i));
            }

            var anySuccess = false;

            // keep scanning regions for naked singles, removing
            // options when digits are placed. when a digit is placed,
            // its box, row and column are added back to 'regions' so
            // they can be scanned again.
            while (regions.TryDequeue(out var region))
            {
                anySuccess = ScanRegion(regions, cells, region) || anySuccess;
            }

            if (anySuccess)
            {
                var newPuzzle = new Puzzle(cells);
                return (true, newPuzzle);
            }

            return (false, puzzle);
        }

        private bool ScanRegion(RegionQueue regions, Cell[] cells, Region region)
        {
            var placedDigits = region.GetPlacedDigits();
            var removedOptions = false;

            for (int i = 0; i < Puzzle.LineLength; i++)
            {
                var cell = region[i];

                if (cell.HasOptions(placedDigits))
                {
                    var newCell = cell.RemoveOptions(placedDigits);
                    cells[cell.Index] = newCell;

                    if (newCell.IsResolved)
                    {
                        // since we have placed a digit we must
                        // scan these regions again
                        regions.Enqueue(new Region(cells, RegionType.Row, newCell.Row));
                        regions.Enqueue(new Region(cells, RegionType.Column, newCell.Column));
                        regions.Enqueue(new Region(cells, RegionType.Box, newCell.Box));
                    }

                    removedOptions = true;
                }

                // we now interpret i as a digit instead and check if it only
                // has one position in the region (then we can place it)
                var digit = SudokuValues.FromIndex(i);

                if (placedDigits.HasAnyOptions(digit) || removedOptions)
                {
                    continue;
                }

                var positions = region.GetPositions(digit);

                if (positions.IsSingle)
                {
                    var index = positions.ToIndex();
                    var currentCell = region[index];

                    cells[currentCell.Index] = currentCell.SetValue(digit);

                    regions.Enqueue(new Region(cells, RegionType.Row, currentCell.Row));
                    regions.Enqueue(new Region(cells, RegionType.Column, currentCell.Column));
                    regions.Enqueue(new Region(cells, RegionType.Box, currentCell.Box));

                    removedOptions = true;
                }
            }

            return removedOptions;
        }
    }
}
