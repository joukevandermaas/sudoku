using System;

namespace Sudoku
{
    public readonly struct Coordinate : IEquatable<Coordinate>
    {
        public readonly int _globalRowIndex;

        public int GlobalRowIndex => _globalRowIndex;
        public int GlobalColumnIndex => CoordinateConverter.RowToCol(_globalRowIndex);
        public int GlobalBoxIndex => CoordinateConverter.RowToBox(_globalRowIndex);

        public int IndexInRow => GlobalRowIndex % Puzzle.LineLength;
        public int IndexInColumn => GlobalColumnIndex % Puzzle.LineLength;
        public int IndexInBox => GlobalBoxIndex % Puzzle.LineLength;

        public int Row => GlobalRowIndex / Puzzle.LineLength;
        public int Column => GlobalColumnIndex / Puzzle.LineLength;
        public int Box => GlobalBoxIndex / Puzzle.LineLength;

        public Coordinate(RegionType regionType, int globalIndex)
        {
            _globalRowIndex = CoordinateConverter.ConvertToRow(globalIndex, regionType);
        }

        public Coordinate(RegionType regionType, int regionIndex, int indexInRegion)
            : this(regionType, (regionIndex * Puzzle.LineLength) + indexInRegion)
        {
        }

        public static bool operator ==(Coordinate left, Coordinate right) => left.Equals(right);
        public static bool operator !=(Coordinate left, Coordinate right) => !left.Equals(right);

        public override bool Equals(object? obj) => obj is Coordinate other && Equals(other);

        public bool Equals(Coordinate other) => other._globalRowIndex == _globalRowIndex;
        public override int GetHashCode() => _globalRowIndex.GetHashCode();

        public override string ToString() => $"R{Row + 1}C{Column + 1}";
    }
}
