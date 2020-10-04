using System.Collections.Generic;
using System.Linq;

namespace Sudoku
{
    static class Helpers
    {
        public static bool AnyDigitPlaced(this Region region, SudokuValues digits)
        {
            foreach (var cell in region)
            {
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

        public static IEnumerable<int[]> GetCombinationIndices(int count, int tupleSize)
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
