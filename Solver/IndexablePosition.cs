using System;

namespace Sudoku
{
    public readonly struct IndexablePosition : IEquatable<IndexablePosition>
    {
        private const int _rowOffset = (int)(RegionType.Row - 1) * Puzzle.LineLength;
        private const int _colOffset = (int)(RegionType.Column - 1) * Puzzle.LineLength;
        private const int _boxOffset = (int)(RegionType.Box - 1) * Puzzle.LineLength;
        private const int _rowMask = SudokuValues.Mask << _rowOffset;
        private const int _colMask = SudokuValues.Mask << _colOffset;
        private const int _boxMask = SudokuValues.Mask << _boxOffset;
        private const int _allMask = _rowMask | _colMask | _boxMask;

        public static IndexablePosition All { get; } = new IndexablePosition(_allMask);
        public static IndexablePosition None { get; } = new IndexablePosition(0);

        private readonly int _values;

        private IndexablePosition(int values)
        {
            _values = values & _allMask;
        }

        public IndexablePosition(SudokuValues rowValue, SudokuValues columnValue, SudokuValues boxValue)
            : this((rowValue.Values << _rowOffset) | (columnValue.Values << _colOffset) | (boxValue.Values << _boxOffset))
        {
        }

        public SudokuValues RowValue => new SudokuValues((_values & _rowMask) >> _rowOffset);
        public SudokuValues ColumnValue => new SudokuValues((_values & _colMask) >> _colOffset);
        public SudokuValues BoxValue => new SudokuValues((_values & _boxMask) >> _boxOffset);

        public SudokuValues GetValue(RegionType type)
        {
            var offset = GetOffset(type);
            var mask = GetMask(offset);

            return new SudokuValues((_values & mask) >> offset);
        }

        public IndexablePosition SetRowValue(SudokuValues newValue)
            => new IndexablePosition((_values & ~_rowMask) | (newValue.Values << _rowOffset));
        public IndexablePosition SetColumnValue(SudokuValues newValue)
            => new IndexablePosition((_values & ~_colMask) | (newValue.Values << _colOffset));
        public IndexablePosition SetBoxValue(SudokuValues newValue)
            => new IndexablePosition((_values & ~_boxMask) | (newValue.Values << _boxOffset));

        public IndexablePosition SetValue(RegionType type, SudokuValues newValue)
        {
            var offset = GetOffset(type);
            var mask = GetMask(offset);

            return new IndexablePosition((_values & ~mask) | (newValue.Values << offset));
        }

        private int GetOffset(RegionType type) => (int)(type - 1) * Puzzle.LineLength;

        private int GetMask(int offset) => SudokuValues.Mask << offset;

        public bool Equals(IndexablePosition other) => _values == other._values;

        public override bool Equals(object? obj) => obj is IndexablePosition other && Equals(other);

        public override int GetHashCode() => _values.GetHashCode();

    }
}
