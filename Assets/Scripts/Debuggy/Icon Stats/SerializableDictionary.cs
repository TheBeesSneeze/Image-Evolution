using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DictionaryEntry<TKey, TValue>
{
    public TKey key;
    public TValue value;

    public DictionaryEntry(TKey key, TValue value)
    {
        this.key = key;
        this.value = value;
    }
}

[Serializable]
public class SerializableDictionary<TKey, TValue>
{
    public List<DictionaryEntry<TKey, TValue>> pairs = new List<DictionaryEntry<TKey, TValue>>();

    public SerializableDictionary(Dictionary<TKey, TValue> sourceDictionary)
    {
        foreach (var pair in sourceDictionary)
        {
            pairs.Add(new DictionaryEntry<TKey, TValue>(pair.Key, pair.Value));
        }
    }

    public Dictionary<TKey, TValue> ToDictionary()
    {
        Dictionary<TKey, TValue> result = new Dictionary<TKey, TValue>();
        foreach (var entry in pairs)
        {
            result.Add(entry.key, entry.value);
        }
        return result;
    }
}