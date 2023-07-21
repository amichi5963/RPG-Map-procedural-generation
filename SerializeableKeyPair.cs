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
    // �C���f�N�T�̒�`
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
                // �C���f�b�N�X���͈͊O�̏ꍇ�̓f�t�H���g�l��Ԃ��Ȃǂ̓K�؂ȏ������s�����Ƃ��ł��܂��B
                // �Ⴆ�΁A�ȉ��̂悤�ɂ���ƁA�f�t�H���g�l���Ԃ���܂��B
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
                // �C���f�b�N�X���͈͊O�̏ꍇ�͗�O���X���[����Ȃǂ̓K�؂ȏ������s�����Ƃ��ł��܂��B
                throw new System.IndexOutOfRangeException();
            }
        }
    }
}