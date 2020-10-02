using System;
using System.Collections.Generic;
using System.Linq;

namespace Sudoku
{
    struct SudokuValues : IEquatable<SudokuValues>
    {
        public const int Mask = 0x1FF; // only first 9 bits, we don't care about the rest

        public static SudokuValues All { get; } = new SudokuValues(~0);
        public static SudokuValues None { get; } = new SudokuValues(0);

        public static SudokuValues FromHumanValue(int value) => new SudokuValues(1 << (value - 1));
        public static SudokuValues FromCharacter(char value)
        {
            var digit = (int)(value - 48); // 48 is 0 in ascii
            var values = digit < 1 || digit > 9
                ? SudokuValues.All
                : SudokuValues.FromHumanValue(digit);

            return values;
        }

        public SudokuValues(int value)
        {
            Values = value & Mask; // discards anything besides the first 9 bits
        }

        // bits 1-9 represent possible values for the cell, other bits are ignored
        public int Values { get; }

        public bool IsSingle => (Values & (Values - 1)) == 0 && Values != 0;

        public SudokuValues RemoveOptions(SudokuValues options) => new SudokuValues(Values & (~options.Values));
        public SudokuValues AddOptions(SudokuValues options) => new SudokuValues(Values | options.Values);
        public bool HasAnyOptions(SudokuValues options) => (Values & options.Values) != 0;

        public int ToHumanValue()
        {
            return ((int)Math.Log2(Values)) + 1;
        }

        public IEnumerable<int> ToHumanOptions() => GetOptions().Select(o => o.ToHumanValue());

        public IEnumerable<SudokuValues> GetOptions()
        {
            for (int i = 1; i <= Puzzle.LineLength; i++)
            {
                var option = SudokuValues.FromHumanValue(i);
                if (HasAnyOptions(option))
                {
                    yield return option;
                }
            }
        }

        public int GetOptionCount()
        {
            var count = 0;
            for (int i = 1; i <= Puzzle.LineLength; i++)
            {
                var option = SudokuValues.FromHumanValue(i);
                if (HasAnyOptions(option))
                {
                    count += 1;
                }
            }
            return count;
        }

        public static bool operator ==(SudokuValues left, SudokuValues right) => left.Equals(right);
        public static bool operator !=(SudokuValues left, SudokuValues right) => !left.Equals(right);

        public override bool Equals(object? obj) => obj is SudokuValues other && Equals(other);

        public bool Equals(SudokuValues other) => other.Values == Values;

        public override string ToString() => ToHumanValue().ToString();

        public override int GetHashCode() => HashCode.Combine(Values);
    }
}
