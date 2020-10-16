using System.Buffers;
using System.Collections.Generic;

namespace Sudoku
{
    public class FishStrategy : ISolveStrategy
    {
        private readonly int _size;
        private List<Cell> _updatedCells; 

        public FishStrategy(int size)
        {
            _size = size;
            _updatedCells = new List<Cell>(Puzzle.LineLength * _size);
        }

        public ChangeSet Apply(in Puzzle puzzle, RegionQueue unprocessedRegions)
        {
            _updatedCells.Clear();

            var potentialRegions = new List<(Region region, SudokuValues positions)>();

            var changedRows = unprocessedRegions.Rows;
            var changedCols = unprocessedRegions.Columns;

            for (var digit = 1; digit <= Puzzle.LineLength; digit++)
            {
                if (changedRows != SudokuValues.None)
                {
                    FindSwordfish(potentialRegions, puzzle, RegionType.Row, digit);

                    if (_updatedCells.Count > 0)
                    {
                        return new ChangeSet(_updatedCells);
                    }
                }

                if (changedCols != SudokuValues.None)
                {
                    FindSwordfish(potentialRegions, puzzle, RegionType.Column, digit);

                    if (_updatedCells.Count > 0)
                    {
                        return new ChangeSet(_updatedCells);
                    }
                }
            }

            unprocessedRegions.Clear();

            return ChangeSet.Empty;
        }

        private void FindSwordfish(List<(Region region, SudokuValues positions)> potentialRegions, in Puzzle puzzle, RegionType type, int digit)
        {
            potentialRegions.Clear();

            var perpendicularType = type == RegionType.Row ? RegionType.Column : RegionType.Row;
            var value = SudokuValues.FromHumanValue(digit);

            foreach (var region in puzzle.GetRegions(type))
            {
                var positions = region.GetPositions(value);
                var count = positions.GetOptionCount();

                if (count > 1 && count <= _size)
                {
                    potentialRegions.Add((region, positions));
                }
            }

            // cannot find a swordfish, because we don't have enough rows/columns
            if (potentialRegions.Count < _size)
            {
                return;
            }

            var combinations = Helpers.GetCombinationIndices(potentialRegions.Count, _size);
            var combinedPositions = SudokuValues.None;
            var regions = new List<Region>();
            var foundFish = false;

            for (int i = 0; i < combinations.Length; i++)
            {
                var combination = ArrayPool<int>.Shared.Rent(_size);

                combinations[i].CopyIndices(combination);

                combinedPositions = SudokuValues.None;
                regions.Clear();

                for (var index = 0; index < _size; index++)
                {
                    var regionInfo = potentialRegions[index];
                    combinedPositions = combinedPositions.AddOptions(regionInfo.positions);
                    regions.Add(regionInfo.region);
                }

                ArrayPool<int>.Shared.Return(combination);

                if (combinedPositions.GetOptionCount() == _size)
                {
                    foundFish = true;
                    break;
                }
            }

            // no 3 rows/columns had options in exactly 3 spots
            if (!foundFish)
            {
                return;
            }

            // we found a swordfish!
            var options = combinedPositions.ToIndices();

            foreach (var option in options)
            {
                var perpendicularRegion = puzzle.GetRegion(perpendicularType, option);

                for (int i = 0; i < Puzzle.LineLength; i++)
                {
                    var cell = perpendicularRegion[i];

                    if (!cell.HasOptions(value))
                    {
                        continue;
                    }

                    var originalRegionsContains = false;
                    foreach (var region in regions)
                    {
                        if (region.Contains(cell))
                        {
                            originalRegionsContains = true;
                            break;
                        }
                    }

                    if (originalRegionsContains)
                    {
                        continue;
                    }

                    _updatedCells.Add(cell.RemoveOptions(value));
                }
            }

            if (_updatedCells.Count > 0)
            {
#if DEBUG
                Program.HighlightDigit = digit;
                Program.AddDebugText($"Fish of size {_size} in {string.Join(", ", regions)}.");
#endif
            }
        }
    }
}
