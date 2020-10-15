using System.Collections.Generic;
using System.Linq;

namespace Sudoku
{
    public class NakedSingleStrategy : ISolveStrategy
    {
        public ChangeSet Apply(in Puzzle puzzle, RegionQueue unprocessedRegions)
        {
            var cells = puzzle.Cells.ToArray();
            var changedIndices = new HashSet<int>();

            var anySuccess = false;

            // keep scanning regions for naked singles, removing
            // options when digits are placed. when a digit is placed,
            // its box, row and column are added back to 'regions' so
            // they can be scanned again.
            while (unprocessedRegions.TryDequeue(cells, out var region))
            {
                anySuccess = ScanRegion(unprocessedRegions, cells, region, changedIndices) || anySuccess;
            }

            if (anySuccess)
            {
                var changes = new List<Cell>(changedIndices.Count);
                foreach (var index in changedIndices)
                {
                    changes.Add(cells[index]);
                }
                return new ChangeSet(changes);
            }

            return ChangeSet.Empty;
        }

        private bool ScanRegion(RegionQueue regions, Cell[] cells, Region region, HashSet<int> changedIndices)
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
                    changedIndices.Add(cell.Index);

                    if (newCell.IsResolved)
                    {
                        // since we have placed a digit we must
                        // scan these regions again
                        regions.Enqueue(RegionType.Row, newCell.Row);
                        regions.Enqueue(RegionType.Column, newCell.Column);
                        regions.Enqueue(RegionType.Box, newCell.Box);
                    }

                    removedOptions = true;
                }
            }

            return removedOptions;
        }
    }
}
