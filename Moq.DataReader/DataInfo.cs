using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;

namespace Moq.DataReader
{
  internal class DataInfo<T>
  {
    public List<T> Data { get; }
    public int FieldCount => _properties.Length;
    public bool Closed { get; set; }

    private PropertyInfo[] _properties;
    private string[] _fieldNames;

    public DataInfo(List<T> data, string[] fieldNames)
    {
      Data = data;
      Closed = false;
      _properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
      _fieldNames = fieldNames;

      if (_fieldNames != null && _fieldNames.Length != _properties.Length)
        throw new ArgumentException($"{nameof(fieldNames)}.Length != number of properties");
    }

    public Type GetFieldType(int i)
    {
      ThrowIfOutOfRange(i);

      return _properties[i].PropertyType;
    }

    public string GetName(int i)
    {
      ThrowIfOutOfRange(i);

      return _fieldNames != null ? _fieldNames[i] : _properties[i].Name;
    }

    public U GetValue<U>(int r, int c)
    {
      ThrowIfOutOfRange(c);

      PropertyInfo prop = _properties[c];
      object obj = Data[r];
      return (U)Convert.ChangeType(prop.GetValue(obj), typeof(U));
    }

    public string GetDataTypeName(int i)
    {
      Type t = GetFieldType(i);
      return t.Name;
    }

    public Stream GetStream(int r, int c)
    {
      ThrowIfOutOfRange(c);

      object obj = GetValue<object>(r, c);
      BinaryFormatter bf = new BinaryFormatter();
      MemoryStream ms = new MemoryStream();
      bf.Serialize(ms, obj);
      return ms;
    }

    public int GetOrdinal(string name)
    {
      for (int i = 0; i < _properties.Length; ++i)
      {
        if (_properties[i].Name == name)
          return i;
      }
      throw new IndexOutOfRangeException();
    }

    public int GetValues(int r, object[] values)
    {
      int length = Math.Min(FieldCount, values.Length);
      for (int c = 0; c < length; c++)
        values[c] = GetValue<object>(r, c);
      return length;
    }

    private void ThrowIfOutOfRange(int c)
    {
      if (Closed)
        throw new InvalidOperationException();

      if (c < 0 || c >= _properties.Length)
        throw new IndexOutOfRangeException();
    }
  }
}
