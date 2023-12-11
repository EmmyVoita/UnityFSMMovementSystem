using System;
using static UnityEditor.Progress;

public class CircularBuffer<T>
{
    private T[] buffer;
    private int head;
    private int tail;

    public CircularBuffer(int size)
    {
        buffer = new T[size];
        head = 0;
        tail = 0;

       
    }

    public void FillValue(int size, T item)
    {
        for (int i = 0; i < size; i++)
        {
            Add(item);
        }
    }   

    public void Add(T item)
    {
        buffer[head] = item;
        head = (head + 1) % buffer.Length;

        if (head == tail)
        {
            tail = (tail + 1) % buffer.Length;
        }
    }

  

    public T GetPastElementFromIndex(int framesAgo)
    {
        if (framesAgo >= buffer.Length || framesAgo >= Count)
        {
            throw new ArgumentOutOfRangeException("framesAgo", "Requested frames ago is beyond buffer size.");
        }

        int index = (head - framesAgo - 1 + buffer.Length) % buffer.Length;
        return buffer[index];
    }

    public int Count
    {
        get
        {
            int count = head - tail;
            return count < 0 ? count + buffer.Length : count;
        }
    }
}
