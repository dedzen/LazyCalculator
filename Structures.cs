
public class ListX<T>
{  // Generic list implementation. Don't need any other methods except these
    private T[] _array = new T[10];

    private int _pointer = 0;

    public void Add(T element)
    {
        _array[_pointer] = element;
        _pointer += 1;

        if (_pointer == _array.Length)
        {
            T[] extendedArray = new T[_array.Length * 2];
            for (var i = 0; i < _array.Length; i++)
            {
                extendedArray[i] = _array[i];
            }

            _array = extendedArray;
        }
    }
    public T[] ToArray()
    {
        T[] res = new T[_pointer];
        for(var i = 0;i<_pointer; i++)
        {
            res[i] = _array[i];
        }
        return res;
    }
}

public class StackX<T>
{
    private const int Capacity = 100;

    private T[] _array = new T[Capacity];

    private int _pointer;

    public void Push(T value)
    {
        if (_pointer == _array.Length)
        {
            // this code is raising an exception about reaching stack limit
            throw new Exception("Stack overflowed");
        }

        _array[_pointer] = value;
        _pointer++;
    }

    public T Pop()
    {
        if (_pointer == 0)
        {
            //you can also raise an exception here, but we're simple returning nothing
            throw new Exception("Stack empty");
        }

        T value = _array[_pointer-1];
        _pointer--;
        return value;
    }
    public T Peek()
    {
        if (_pointer == 0)
        {
            throw new Exception("Stack empty");
        }
        T value = _array[_pointer-1];
        return value;
    }
    public int Count
    {
        get {return _pointer;}
    }

}

public class QueueX<T>
{
    private const int Capacity = 100;
    private T[] _array = new T[Capacity];
    private int _pointer;
    public void Enqueue(T element)
    {
        if (_pointer == Capacity)
        {
            throw new Exception("Queue overflow");
        }
        _array[_pointer] = element;
        _pointer++;
    }
    // I don't even need Dequeue, but if I did, it's O(N) here.
    // If I wanted N(1) I should use a double linked list, but that's just unnecessary here

    public T[] ToArray()
    {
        T[] res = new T[_pointer];
        for(var i = 0;i<_pointer; i++)
        {
            res[i] = _array[i];
        }
        return res;
    }
}
