using System.Linq;

namespace Sudoku
{
    internal class SingleStrategy : ISolveStrategy
    {
        public (bool, Puzzle) Apply(Puzzle puzzle)
        {
            var cells = puzzle.Cells.ToArray();

            var anySuccess = false;
            for (int tupleSize = 1; tupleSize <= 4; tupleSize++)
            {
                for (int i = 0; i < Puzzle.LineLength; i++)
                {
                    var row = new Region(cells, RegionType.Row, i);
                    anySuccess = ScanRegion(cells, row) || anySuccess;

                    var col = new Region(cells, RegionType.Column, i);
                    anySuccess = ScanRegion(cells, col) || anySuccess;

                    var box = new Region(cells, RegionType.Box, i);
                    anySuccess = ScanRegion(cells, box) || anySuccess;
                }
            }

            puzzle = new Puzzle(cells);

            return (anySuccess, puzzle);
        }

        private bool ScanRegion(Cell[] cells, Region region)
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
                    cells[cell.Index] = cell.RemoveOptions(placedDigits);
                    removedOptions = true;
                }
            }

            return removedOptions;
        }
    }
}
