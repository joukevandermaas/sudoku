using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace Sudoku
{
    static class Helpers
    {
        private static Dictionary<(int, int), int[][]> _combinationCache = new Dictionary<(int, int), int[][]>();

        public static bool AnyDigitPlaced(this Region region, SudokuValues digits)
        {
            for (int i = 0; i < Puzzle.LineLength; i++)
            {
                var cell = region[i];
                if (cell.IsResolved && cell.Value.HasAnyOptions(digits))
                {
                    return true;
                }
            }

            return false;
        }

        public static SudokuValues GetPositions(this Region region, SudokuValues digits)
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

        public static int[][] GetCombinationIndices(int count, int tupleSize)
        {
            if (_combinationCache.ContainsKey((count, tupleSize)))
            {
                return _combinationCache[(count, tupleSize)];
            }

            var combinations = GetCombinationsHelper(count, 0, new int[0], tupleSize).ToArray();

            _combinationCache.Add((count, tupleSize), combinations);

            return combinations;
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
