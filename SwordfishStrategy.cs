using System;
using System.Collections.Generic;

namespace Sudoku
{
    internal class SwordfishStrategy : ISolveStrategy
    {
        public (bool, Puzzle) Apply(Puzzle puzzle)
        {
            for (var fishSize = 2; fishSize < 4; fishSize++)
            {
                for (var digit = 1; digit <= Puzzle.LineLength; digit++)
                {
                    var (success, newPuzzle) = FindSwordfish(puzzle, RegionType.Row, digit, fishSize);

                    if (success)
                    {
                        return (true, newPuzzle);
                    }

                    (success, newPuzzle) = FindSwordfish(puzzle, RegionType.Column, digit, fishSize);

                    if (success)
                    {
                        return (true, newPuzzle);
                    }
                }
            }

            return (false, puzzle);
        }

        private (bool, Puzzle) FindSwordfish(Puzzle puzzle, RegionType type, int digit, int fishSize)
        {
            var perpendicularType = type == RegionType.Row ? RegionType.Column : RegionType.Row;
            var value = SudokuValues.FromHumanValue(digit);

            var potentialRegions = new List<(Region region, SudokuValues positions)>();

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
                var combination = combinations[i].ToIndices();

                combinedPositions = SudokuValues.None;
                regions.Clear();

                foreach (var index in combination)
                {
                    var regionInfo = potentialRegions[index];
                    combinedPositions = combinedPositions.AddOptions(regionInfo.positions);
                    regions.Add(regionInfo.region);
                }

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
            var options = combinedPositions.ToHumanOptions();

            var updatedCells = new List<Cell>();
            foreach (var option in options)
            {
                var perpendicularRegion = puzzle.GetRegion(perpendicularType, option - 1);

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
                Program.HighlightDigit = digit;
                Program.DebugText = $"Fish of size {fishSize} in {string.Join(", ", regions)}.";
                return (true, puzzle.UpdateCells(updatedCells));
            }

            return (false, puzzle);
        }
    }
}
