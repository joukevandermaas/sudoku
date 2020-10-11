using System.Buffers;
using System.Collections.Generic;

namespace Sudoku
{
    internal class SwordfishStrategy : ISolveStrategy
    {
        public (bool, Puzzle) Apply(in Puzzle puzzle)
        {
            var potentialRegions = new List<(Region region, SudokuValues positions)>();
            var updatedCells = new List<Cell>();

            for (var fishSize = 2; fishSize < 4; fishSize++)
            {
                for (var digit = 1; digit <= Puzzle.LineLength; digit++)
                {
                    var (success, newPuzzle) = FindSwordfish(potentialRegions, updatedCells, puzzle, RegionType.Row, digit, fishSize);

                    if (success)
                    {
                        return (true, newPuzzle);
                    }

                    (success, newPuzzle) = FindSwordfish(potentialRegions, updatedCells, puzzle, RegionType.Column, digit, fishSize);

                    if (success)
                    {
                        return (true, newPuzzle);
                    }
                }
            }

            return (false, puzzle);
        }

        private (bool, Puzzle) FindSwordfish(List<(Region region, SudokuValues positions)> potentialRegions, List<Cell> updatedCells, in Puzzle puzzle, RegionType type, int digit, int fishSize)
        {
            potentialRegions.Clear();

            var perpendicularType = type == RegionType.Row ? RegionType.Column : RegionType.Row;
            var value = SudokuValues.FromHumanValue(digit);

            foreach (var region in puzzle.GetRegions(type))
            {
                var positions = region.GetPositions(value);
                var count = positions.GetOptionCount();

                if (count > 1 && count <= fishSize)
                {
                    potentialRegions.Add((region, positions));
                }
            }

            // cannot find a swordfish, because we don't have enough rows/columns
            if (potentialRegions.Count < fishSize)
            {
                return (false, puzzle);
            }

            var combinations = Helpers.GetCombinationIndices(potentialRegions.Count, fishSize);
            var combinedPositions = SudokuValues.None;
            var regions = new List<Region>();
            var foundFish = false;

            for (int i = 0; i < combinations.Length; i++)
            {
                var combination = ArrayPool<int>.Shared.Rent(fishSize);

                combinations[i].AddIndices(combination);

                combinedPositions = SudokuValues.None;
                regions.Clear();

                for (var index = 0; index < fishSize; index++)
                {
                    var regionInfo = potentialRegions[index];
                    combinedPositions = combinedPositions.AddOptions(regionInfo.positions);
                    regions.Add(regionInfo.region);
                }

                ArrayPool<int>.Shared.Return(combination);

                if (combinedPositions.GetOptionCount() == fishSize)
                {
                    foundFish = true;
                    break;
                }
            }

            // no 3 rows/columns had options in exactly 3 spots
            if (!foundFish)
            {
                return (false, puzzle);
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

                    updatedCells.Add(cell.RemoveOptions(value));
                }
            }

            if (updatedCells.Count > 0)
            {
#if DEBUG
                Program.HighlightDigit = digit;
                Program.DebugText = $"Fish of size {fishSize} in {string.Join(", ", regions)}.";
#endif

                return (true, puzzle.UpdateCells(updatedCells));
            }

            return (false, puzzle);
        }
    }
}
