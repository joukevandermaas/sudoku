using System.Collections;
using System.Collections.Generic;

namespace Sudoku
{
    // allows each item to be enqueued only once,
    // dequeue is in expected order
    internal class RegionQueue
    {
        // 3 pairs of 9 bits
        // 0-8   represent rows 1-9
        // 9-17  represent columns 1-9
        // 18-26 represent boxes 1-9
        // when a bit is set that means the queue already
        // contains that region.
        private int _currentValues;
        private Queue<Region> _queue;

        public RegionQueue()
        {
            _queue = new Queue<Region>(3 * Puzzle.LineLength);
        }

        public bool Enqueue(Region item)
        {
            var mask = GetMask(item);

            if ((_currentValues & mask) == 0)
            {
                _queue.Enqueue(item);
                _currentValues |= mask;

                return true;
            }

            return false;
        }

        public int Count => _queue.Count;

        public int GetMask(Region region)
        {
            return 1 << (region.Index + (Puzzle.LineLength * (int)region.Type));
        }

        public bool TryDequeue(out Region item)
        {
            if (_queue.Count == 0)
            {
                item = default!;
                return false;
            }

            item = _queue.Dequeue();
            var mask = GetMask(item);

            _currentValues &= ~mask;
            
            return true;
        }
    }
}
