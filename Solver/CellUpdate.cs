using System;

namespace Sudoku
{
    public readonly struct CellUpdate : IEquatable<CellUpdate>
    {
        public SudokuValues RemovedOptions { get; }
        public Coordinate Coordinate { get; }

        public CellUpdate(SudokuValues removedOptions, Coordinate coordinate)
        {
            RemovedOptions = removedOptions;
            Coordinate = coordinate;
        }

        public static bool operator ==(CellUpdate left, CellUpdate right) => left.Equals(right);
        public static bool operator !=(CellUpdate left, CellUpdate right) => !left.Equals(right);

        public override bool Equals(object? obj) => obj is CellUpdate other && Equals(other);

        public bool Equals(CellUpdate other) => other.RemovedOptions == RemovedOptions && other.Coordinate == Coordinate;
        public override int GetHashCode() => HashCode.Combine(RemovedOptions, Coordinate);

        public override string ToString() => RemovedOptions.IsSingle ? RemovedOptions.ToString() : ".";
    }
}
