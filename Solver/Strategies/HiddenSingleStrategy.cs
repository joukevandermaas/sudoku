
namespace Sudoku
{
    public class HiddenSingleStrategy : ISolveStrategy
    {
        public void Apply(MutablePuzzle puzzle, RegionQueue changedRegions)
        {
            // keep scanning regions for naked singles, removing
            // options when digits are placed. when a digit is placed,
            // its box, row and column are added back to 'regions' so
            // they can be scanned again.
            while (changedRegions.TryDequeue(puzzle.Puzzle, out var region))
            {
                ScanRegion(changedRegions, puzzle, region);
            }
        }

        private void ScanRegion(RegionQueue regions, MutablePuzzle puzzle, Region region)
        {
            var placedDigits = region.GetPlacedDigits();

            for (int i = 1; i <= Puzzle.LineLength; i++)
            {
                var digit = SudokuValues.FromHumanValue(i);

                if (placedDigits.HasAnyOptions(digit))
                {
                    continue;
                }

                var positions = region.GetPositions(digit);

                if (positions.IsSingle)
                {
                    var index = positions.ToIndex();
                    var coords = region.GetCoordinate(index);

                    var update = new CellUpdate(digit.Invert(), coords);
                    puzzle.RemoveOptions(update);

                    regions.Enqueue(RegionType.Row, coords.Row);
                    regions.Enqueue(RegionType.Column, coords.Column);
                    regions.Enqueue(RegionType.Box, coords.Box);
                }
            }
        }

    }
}
