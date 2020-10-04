using System.Linq;

namespace Sudoku
{
    internal class OneOptionStrategy : ISolveStrategy
    {
        public (bool, Puzzle) Apply(Puzzle puzzle)
        {
            // we copy the cells so we can mutate the array directly
            var cells = puzzle.Cells.ToArray();

            var anySuccess = false;
            foreach (var region in puzzle.Regions)
            {
                anySuccess = ScanRegion(cells, region) || anySuccess;
            }

            puzzle = new Puzzle(cells);

            return (anySuccess, puzzle);
        }

        private bool ScanRegion(Cell[] cells, Region region)
        {
            bool foundAny = false;
            for (var i = 1; i <= Puzzle.LineLength; i++)
            {
                var digit = SudokuValues.FromHumanValue(i);
                var count = 0;
                Cell foundCell = default;

                foreach (var cell in region)
                {
                    if (cell.HasOptions(digit))
                    {
                        count += 1;
                        foundCell = cell;
                    }
                }

                if (count == 1)
                {
                    cells[foundCell.Index] = foundCell.SetValue(digit);
                    foundAny = true;
                }
            }

            return foundAny;
        }
    }
}
