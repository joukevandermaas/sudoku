using System;
using System.Collections.Generic;
using System.Linq;

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
                var positions = GetPositions(region, value);
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

            var combinations = GetCombinationIndices(potentialRegions.Count, fishSize).ToArray();
            var combinedPositions = SudokuValues.None;
            List<Region> regions = null;

            for (int i = 0; i < combinations.Length; i++)
            {
                var comb = combinations[i];

                combinedPositions = comb
                    .Select(i => potentialRegions[i].positions)
                    .Aggregate(SudokuValues.None, (allPositions, positions) => allPositions.AddOptions(positions));

                if (combinedPositions.GetOptionCount() == fishSize)
                {
                    regions = comb.Select(i => potentialRegions[i].region).ToList();
                    break;
                }
            }

            // no 3 rows/columns had options in exactly 3 spots
            if (regions == null)
            {
                return (false, puzzle);
            }

            // we found a swordfish!
            var options = combinedPositions.ToHumanOptions();

            var updatedCells = new List<Cell>();
            foreach (var option in options)
            {
                var perpendicularRegion = puzzle.GetRegion(perpendicularType, option - 1);

                foreach (var cell in perpendicularRegion)
                {
                    if (!regions.Any(r => r.Contains(cell)) && cell.HasOptions(value))
                    {
                        updatedCells.Add(cell.RemoveOptions(value));
                    }
                }
            }

            if (updatedCells.Any())
            {
                Program.HighlightDigit = digit;
                Program.DebugText = $"Fish size: {fishSize}. Regions: {string.Join(", ", regions)}.";
                return (true, puzzle.UpdateCells(updatedCells));
            }

            return (false, puzzle);
        }

        private SudokuValues GetPositions(Region region, SudokuValues digits)
        {
            var positions = SudokuValues.None;

            for (var i = 0; i < Puzzle.LineLength; i++)
            {
                var cell = region[i];
                if (cell.HasOptions(digits))
                {
                    positions = positions.AddOptions(SudokuValues.FromHumanValue(i + 1));
                }
            }

            return positions;
        }

        private static IEnumerable<int[]> GetCombinationIndices(int count, int tupleSize)
        {
            return GetCombinationsHelper(count, 0, new int[0], tupleSize);
        }

        private static IEnumerable<int[]> GetCombinationsHelper(int count, int startDigit, int[] previousDigits, int tupleSize)
        {
            for (int i = startDigit; i < count; i++)
            {
                var result = previousDigits.Concat(new[] { i }).ToArray();

                if (result.Length == tupleSize)
                {
                    yield return result;
                }
                else
                {
                    foreach (var tuple in GetCombinationsHelper(count, i + 1, result, tupleSize))
                    {
                        yield return tuple;
                    }
                }
            }
        }
    }
}
