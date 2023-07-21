using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SerializableKeyPair<TKey, TValue>
{
    [SerializeField] private TKey key;
    [SerializeField] private TValue value;

    public TKey Key => key;
    public TValue Value => value;
}
public class SerializableList<T>
{
    public List<T> list;

    public int Count => list.Count;
    // インデクサの定義
    public T this[int index]
    {
        get
        {
            if (list != null && index >= 0 && index < list.Count)
            {
                return list[index];
            }
            else
            {
                // インデックスが範囲外の場合はデフォルト値を返すなどの適切な処理を行うことができます。
                // 例えば、以下のようにすると、デフォルト値が返されます。
                // return default(T);
                throw new System.IndexOutOfRangeException();
            }
        }
        set
        {
            if (list == null)
            {
                list = new List<T>();
            }

            if (index >= 0 && index < list.Count)
            {
                list[index] = value;
            }
            else if (index == list.Count)
            {
                list.Add(value);
            }
            else
            {
                // インデックスが範囲外の場合は例外をスローするなどの適切な処理を行うことができます。
                throw new System.IndexOutOfRangeException();
            }
        }
    }
}