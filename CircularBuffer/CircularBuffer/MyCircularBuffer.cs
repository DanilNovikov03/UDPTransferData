using System.Collections;


namespace CircularBuffer.CircularBuffer
{
    public class MyCircularBuffer<T> : IEnumerable<T>, IEnumerable
    {
        protected readonly T[] _buffer;
        protected int _start;
        protected int _end;

        public int Capacity => _buffer.Length;
        public bool IsFull => Count == Capacity;
        public bool IsEmpty => Count == 0;
        public int Count { get; private set; }

        public MyCircularBuffer(int capacity)
            : this(capacity, Array.Empty<T>()) { }

        public MyCircularBuffer(int capacity, T[] items)
        {
            if (capacity < 1)
                throw new ArgumentException("Circular buffer must have a capacity greater than 0.", "capacity");

            if (items == null)
                throw new ArgumentNullException("items");

            if (items.Length > capacity)
                throw new ArgumentException("Too many items to fit circular buffer", "items");

            _buffer = new T[capacity];
            Array.Copy(items, _buffer, items.Length);
            Count = items.Length;
            _start = 0;
            _end = ((Count != capacity) ? Count : 0);
        }

        public virtual T this[int index]
        {
            get => _buffer[InternalIndex(index)];
            set => _buffer[InternalIndex(index)] = value;
        }

        public virtual int IndexOf(T item)
        {
            for (int i = 0; i < Count; i++)
                if (object.Equals(this[i], item))
                    return i;

            return -1;
        }

        public virtual void Add(T item)
        {
            if (IsFull)
                AddFullBuffer(item);
            else
                AddNoFullBuffer(item);
        }

        protected void AddFullBuffer(T item)
        {
            _buffer[_end] = item;
            Increment(ref _end);
            _start = _end;
        }

        protected void AddNoFullBuffer(T item)
        {
            _buffer[_end] = item;
            Increment(ref _end);
            Count++;
        }

        public bool Contains(T item) =>
            IndexOf(item) != -1;

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array.Length - arrayIndex < Count)
                throw new ArgumentException("Array does not contain enough space for items");

            for (int i = 0; i < Count; i++)
                array[i + arrayIndex] = this[i];
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
                yield return this[i];
        }

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();

        protected int InternalIndex(int index) =>
            (_start + index) % Capacity;

        protected void Increment(ref int index)
        {
            if (++index >= Capacity)
                index = 0;
        }

        protected void IncrementCount()
        {
            if (!IsFull)
                Count++;
        }
    }
}
