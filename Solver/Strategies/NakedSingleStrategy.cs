namespace Sudoku
{
    public class NakedSingleStrategy : ISolveStrategy
    {
        public void Apply(MutablePuzzle puzzle, RegionQueue changedRegions)
        {
            // keep scanning regions for naked singles, removing
            // options when digits are placed. when a digit is placed,
            // its box, row and column are added back to 'regions' so
            // they can be scanned again.
            while (changedRegions.TryDequeue(puzzle, out var region))
            {
                ScanRegion(changedRegions, puzzle, region);
            }
        }

        private void ScanRegion(RegionQueue regions, MutablePuzzle puzzle, Region region)
        {
            var placedDigits = region.GetPlacedDigits();

            for (int i = 0; i < Puzzle.LineLength; i++)
            {
                var cell = region[i];

                if (!cell.IsSingle && cell.HasAnyOptions(placedDigits))
                {
                    var coords = region.GetCoordinate(i);

                    puzzle.RemoveOptions(coords, placedDigits);

                    var newCell = cell.RemoveOptions(placedDigits);
                    if (newCell.IsSingle)
                    {
                        // since we have placed a digit we must
                        // scan these regions again
                        regions.Enqueue(RegionType.Row, coords.Row);
                        regions.Enqueue(RegionType.Column, coords.Column);
                        regions.Enqueue(RegionType.Box, coords.Box);
                    }
                }
            }
        }
    }
}
