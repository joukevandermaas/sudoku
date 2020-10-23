using System;

namespace Sudoku
{
    internal static class CoordinateConverter
    {
        private static int[] _colCoords = new int[Puzzle.LineLength * Puzzle.LineLength]
        {
            00, 09, 18, 27, 36, 45, 54, 63, 72,
            01, 10, 19, 28, 37, 46, 55, 64, 73,
            02, 11, 20, 29, 38, 47, 56, 65, 74,
            03, 12, 21, 30, 39, 48, 57, 66, 75,
            04, 13, 22, 31, 40, 49, 58, 67, 76,
            05, 14, 23, 32, 41, 50, 59, 68, 77,
            06, 15, 24, 33, 42, 51, 60, 69, 78,
            07, 16, 25, 34, 43, 52, 61, 70, 79,
            08, 17, 26, 35, 44, 53, 62, 71, 80,
        };

        private static int[] _boxCoords = new int[Puzzle.LineLength * Puzzle.LineLength]
        {
            00, 01, 02, 09, 10, 11, 18, 19, 20,
            03, 04, 05, 12, 13, 14, 21, 22, 23,
            06, 07, 08, 15, 16, 17, 24, 25, 26,
            27, 28, 29, 36, 37, 38, 45, 46, 47,
            30, 31, 32, 39, 40, 41, 48, 49, 50,
            33, 34, 35, 42, 43, 44, 51, 52, 53,
            54, 55, 56, 63, 64, 65, 72, 73, 74,
            57, 58, 59, 66, 67, 68, 75, 76, 77,
            60, 61, 62, 69, 70, 71, 78, 79, 80,
        };

        private static int[] _inverseColCoords = new int[Puzzle.LineLength * Puzzle.LineLength];
        private static int[] _inverseBoxCoords = new int[Puzzle.LineLength * Puzzle.LineLength];

        static CoordinateConverter()
        {
            for (int i = 0; i < _colCoords.Length; i++)
            {
                var colIndex = _colCoords[i];
                _inverseColCoords[colIndex] = i;

                var boxIndex = _boxCoords[i];
                _inverseBoxCoords[boxIndex] = i;
            }
        }

        private static int[][] _targetMap = new int[][] { _inverseColCoords, _inverseBoxCoords };

        public static int ConvertToRow(int index, RegionType source)
        {
            if (source == RegionType.Row)
            {
                return index;
            }

            var mapIndex = (int)(source - 2);
            var targetMap = _targetMap[mapIndex];

            return targetMap[index];
        }

        public static int ColToRow(int colIndex) => _inverseColCoords[colIndex];

        public static int BoxToRow(int boxIndex) => _inverseBoxCoords[boxIndex];

        public static int RowToCol(int rowIndex) => _colCoords[rowIndex];

        public static int RowToBox(int rowIndex) => _boxCoords[rowIndex];
    }
}
