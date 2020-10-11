using System.Linq;

namespace Sudoku
{
    internal class SingleStrategy : ISolveStrategy
    {
        public (bool, Puzzle) Apply(Puzzle puzzle)
        {
            var cells = puzzle.Cells.ToArray();
            var regions = new UniqueQueue<Region>(Puzzle.LineLength);

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

            puzzle = new Puzzle(cells);

            return (anySuccess, puzzle);
        }

        private bool ScanRegion(UniqueQueue<Region> regions, Cell[] cells, Region region)
        {
            var placedDigits = SudokuValues.None;
            var removedOptions = false;

            for (int i = 0; i < Puzzle.LineLength; i++)
            {
                var cell = region[i];

                if (cell.IsResolved)
                {
                    placedDigits = placedDigits.AddOptions(cell.Value);
                }
            }

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
            }

            return removedOptions;
        }
    }
}
