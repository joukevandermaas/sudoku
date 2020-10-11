using System.Collections;
using System.Collections.Generic;

namespace Sudoku
{
    // allows each item to be enqueued only once,
    // dequeue is in expected order
    public class UniqueQueue<T> : IEnumerable<T>
    {
        private HashSet<T> _set;
        private Queue<T> _queue;

        public UniqueQueue()
        {
            _set = new HashSet<T>();
            _queue = new Queue<T>();
        }

        public UniqueQueue(int expectedSize)
        {
            _set = new HashSet<T>(expectedSize);
            _queue = new Queue<T>(expectedSize);
        }

        public bool Enqueue(T item)
        {
            if (_set.Add(item))
            {
                _queue.Enqueue(item);

                return true;
            }

            return false;
        }

        public void EnqueueAll(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                Enqueue(item);
            }
        }

        // so initializers work
        public void Add(T item) => Enqueue(item);

        public int Count => _queue.Count;

        public bool TryDequeue(out T item)
        {
            if (_queue.Count == 0)
            {
                item = default!;
                return false;
            }

            item = _queue.Dequeue();
            _set.Remove(item);
            return true;
        }

        public IEnumerator<T> GetEnumerator() => _queue.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _queue.GetEnumerator();
    }
}
