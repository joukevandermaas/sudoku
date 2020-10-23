using System.Collections.Concurrent;

namespace Sudoku
{
    static class Helpers
    {
        private static ConcurrentDictionary<(int, int), SudokuValues[]> _combinationCache = new ConcurrentDictionary<(int, int), SudokuValues[]>();

        public static SudokuValues GetPlacedDigits(this Region region)
        {
            var placedDigits = SudokuValues.None;

            for (int i = 0; i < Puzzle.LineLength; i++)
            {
                var cell = region[i];
                if (cell.IsSingle)
                {
                    placedDigits = placedDigits.AddOptions(cell);
                }
            }

            return placedDigits;
        }

        public static SudokuValues GetPositions(this Region region, SudokuValues digits)
        {
            var positions = SudokuValues.None;

            for (var i = 0; i < Puzzle.LineLength; i++)
            {
                var cell = region[i];
                if (!cell.IsSingle && cell.HasAnyOptions(digits))
                {
                    positions = positions.AddOptions(SudokuValues.FromIndex(i));
                }
            }

            return positions;
        }

        /// <summary>
        /// Returns a flat array containing combinations of size tupleSize in order
        /// </summary>
        public static SudokuValues[] GetCombinationIndices(int count, int tupleSize)
        {
            if (_combinationCache.ContainsKey((count, tupleSize)))
            {
                return _combinationCache[(count, tupleSize)];
            }

            var combinationCount = count == tupleSize ? 1 : Factorial(count) / (Factorial(tupleSize) * Factorial(count - tupleSize));
            var results = new SudokuValues[combinationCount];

            GetCombinationsHelper(
                results: results,
                index: 0,
                count: count,
                startDigit: 0,
                previousDigits: SudokuValues.None,
                tupleSize: tupleSize,
                currentSize: 0);

            _combinationCache.TryAdd((count, tupleSize), results);

            return results;
        }

        private static int Factorial(int n)
        {
            int result = n;

            for (int i = 1; i < n; i++)
            {
                result *= i;
            }

            return result;
        }

        private static int GetCombinationsHelper(SudokuValues[] results, int index, SudokuValues previousDigits, int startDigit, int count, int tupleSize, int currentSize)
        {
            currentSize += 1;

            for (int digit = startDigit; digit < count; digit++)
            {
                var newValues = previousDigits.AddOptions(SudokuValues.FromIndex(digit));

                if (currentSize == tupleSize)
                {
                    // we have a complete tuple now
                    results[index] = newValues;
                    index += 1;
                }
                else
                {
                    index = GetCombinationsHelper(results, index, newValues, digit + 1, count, tupleSize, currentSize);
                }
            }

            return index;
        }
    }
}
