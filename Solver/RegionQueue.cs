using System.Collections.Generic;
using System.Linq;

namespace Sudoku
{
    // allows each item to be enqueued only once,
    // dequeue is in expected order
    public class RegionQueue
    {
        private const int _rowMask = 0x00001FF;
        private const int _colMask = 0x003FE00;
        private const int _boxMask = 0x7FC0000;

        public static RegionQueue GetEmpty() => new RegionQueue(0);
        public static RegionQueue GetFull() => new RegionQueue(_rowMask | _colMask | _boxMask);

        // 3 pairs of 9 bits
        // 0-8   represent rows 1-9
        // 9-17  represent columns 1-9
        // 18-26 represent boxes 1-9
        // when a bit is set that means the queue already
        // contains that region.
        private int _currentValues;
        private int _lastDequeueIndex = 0;

        private RegionQueue(int values)
        {
            _currentValues = values;
        }

        public void EnqueueAll(RegionQueue other)
        {
            _currentValues |= other._currentValues;
            _lastDequeueIndex = 0;
        }

        public void Enqueue(RegionType type, int index)
        {
            var mask = GetMask(type, index);
            _currentValues |= mask;
            _lastDequeueIndex = 0;
        }

        public void IgnoreAllOfType(RegionType type)
        {
            switch (type)
            {
                case RegionType.Row:
                    _currentValues &= ~_rowMask;
                    break;
                case RegionType.Column:
                    _currentValues &= ~_colMask;
                    break;
                case RegionType.Box:
                    _currentValues &= ~_boxMask;
                    break;
            }
            _lastDequeueIndex = 0;
        }

        public bool TryDequeueOfType(RegionType type, out int index)
        {
            var start = ((int)type - 1) * Puzzle.LineLength;
            var end = start + Puzzle.LineLength;

            for (int i = start; i < end; i++)
            {
                var mask = 1 << i;
                if ((_currentValues & mask) != 0)
                {
                    index = GetIndex(i);
                    _currentValues &= ~mask;

                    return true;
                }
            }

            index = default;
            return false;
        }

        public bool TryDequeue(out RegionType type, out int index)
        {
            if (_currentValues != 0)
            {
                for (int i = _lastDequeueIndex; i < (3 * Puzzle.LineLength); i++)
                {
                    _lastDequeueIndex = i;

                    var mask = 1 << i;
                    if ((_currentValues & mask) != 0)
                    {

                        type = GetRegionType(i);
                        index = GetIndex(i);

                        _currentValues &= ~mask;

                        return true;
                    }
                }
            }

            type = default;
            index = default;
            return false;
        }

        public bool TryDequeue(MutablePuzzle puzzle, out Region item)
        {
            if (TryDequeue(out var type, out var index))
            {
                item = puzzle.Puzzle.GetRegion(type, index);
                return true;
            }

            item = default;
            return false;
        }

        public bool TryDequeue(Puzzle puzzle, out Region item)
        {
            if (TryDequeue(out var type, out var index))
            {
                item = puzzle.GetRegion(type, index);
                return true;
            }

            item = default;
            return false;
        }

        private int GetMask(RegionType type, int index)
        {
            return 1 << (index + (Puzzle.LineLength * (int)(type - 1)));
        }

        private RegionType GetRegionType(int bitIndex)
        {
            return (RegionType)((bitIndex / Puzzle.LineLength) + 1);
        }

        private int GetIndex(int bitIndex)
        {
            return (bitIndex % Puzzle.LineLength);
        }

        public bool Contains(RegionType type, int index)
        {
            var mask = GetMask(type, index);

            return (_currentValues & mask) != 0;
        }

        public void Clear()
        {
            _currentValues = 0;
            _lastDequeueIndex = 0;
        }
        public void Clear(RegionType regionType)
        {
            _currentValues &= ~(regionType switch
            {
                RegionType.Row => _rowMask,
                RegionType.Column => _colMask,
                RegionType.Box => _boxMask,
                _ => 0
            });
            _lastDequeueIndex = 0;
        }

        public bool HasAnyRows => (_currentValues & _rowMask) != 0;
        public bool HasAnyColumns => (_currentValues & _colMask) != 0;
        public bool HasAnyBoxes => (_currentValues & _boxMask) != 0;

        public bool IsEmpty => _currentValues == 0;

        public RegionQueue Clone()
        {
            return new RegionQueue(_currentValues);
        }

        public SudokuValues Rows => new SudokuValues(_currentValues & _rowMask);
        public SudokuValues Columns => new SudokuValues((_currentValues & _colMask) >> Puzzle.LineLength);
        public SudokuValues Boxes => new SudokuValues((_currentValues & _boxMask) >> (Puzzle.LineLength * 2));
        public SudokuValues Regions(RegionType type) => type switch
        {
            RegionType.Row => Rows,
            RegionType.Column => Columns,
            RegionType.Box => Boxes,
            _ => SudokuValues.None,
        };

        public override string ToString()
        {
            List<(RegionType type, int index)> items = new List<(RegionType, int)>();
            for (int i = 0; i < (Puzzle.LineLength * 3); i++)
            {
                var mask = 1 << i;

                if ((_currentValues & mask) != 0)
                {
                    var type = GetRegionType(i);
                    var index = GetIndex(i);
                    items.Add((type, index));
                }
            }

            return string.Join(", ", items.Select(r => $"{r.type} {r.index + 1}"));
        }
    }
}
