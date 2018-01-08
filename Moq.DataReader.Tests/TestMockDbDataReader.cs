using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Xunit;

namespace Moq.DataReader.Tests
{
  public class TestMockDbDataReader
  {
    private List<TestModel> _testModelsCollection = new List<TestModel>()
    {
      new TestModel(1, '2', 3, 4, 5, 6, 7, 8, 9, 10.11f, 12.13, true, "15, 16", new DateTime(17), 18.19M, YN.Perhaps),
      new TestModel()
    };

    private string[] _expectedFieldNames = new string[]
    {
      nameof(TestModel.Sb),
      nameof(TestModel.C),
      nameof(TestModel.S),
      nameof(TestModel.I),
      nameof(TestModel.L),
      nameof(TestModel.B),
      nameof(TestModel.Us),
      nameof(TestModel.Ui),
      nameof(TestModel.Ul),
      nameof(TestModel.F),
      nameof(TestModel.D),
      nameof(TestModel.Bo),
      nameof(TestModel.Str),
      nameof(TestModel.Date),
      nameof(TestModel.Dec),
      nameof(TestModel.En),
    };

    [Fact]
    public void TestRead()
    {
      DbDataReader r = CreateDataReaderMock(0).Object;
      Assert.True(r.Read());
      Assert.False(r.Read());
      Assert.False(r.Read());
    }

    [Fact]
    public void TestReadEmpty()
    {
      DbDataReader r = CreateDataReaderMock().Object;
      Assert.False(r.Read());
      Assert.False(r.Read());
    }

    [Fact]
    public void TestFieldCount()
    {
      DbDataReader r = CreateDataReaderMock(0).Object;
      Assert.Equal(r.FieldCount, _expectedFieldNames.Length);
    }

    [Fact]
    public void TestGetName()
    {
      DbDataReader r = CreateDataReaderMock(0).Object;
      for (int i = 0; i < r.FieldCount; ++i)
      {
        Assert.Equal(r.GetName(i), _expectedFieldNames[i]);
      }
    }

    [Fact]
    public void TestGetNameEmpty()
    {
      DbDataReader r = CreateDataReaderMock().Object;
      Assert.Throws<IndexOutOfRangeException>(() => r.GetName(0));
    }

    [Fact]
    public void TestGetNameOutOfRange()
    {
      DbDataReader r = CreateDataReaderMock().Object;
      Assert.Throws<IndexOutOfRangeException>(() => r.GetName(30));
    }

    [Fact]
    public void TestGetNameAfterReading()
    {
      DbDataReader r = CreateDataReaderMock(0).Object;
      r.Read();
      IEnumerable<string> actualFieldNames = Enumerable.Range(0, r.FieldCount - 1).Select(i => r.GetName(i));
      actualFieldNames.SequenceEqual(_expectedFieldNames);
    }

    [Fact]
    public void TestHasRows()
    {
      DbDataReader r = CreateDataReaderMock(0).Object;
      Assert.True(r.HasRows);
    }

    [Fact]
    public void TestHasRowsEmpty()
    {
      DbDataReader r = CreateDataReaderMock().Object;
      Assert.False(r.HasRows);
    }

    [Fact]
    public void TestGetOrdinal()
    {
      DbDataReader r = CreateDataReaderMock().Object;
      for (int i = 0; i < r.FieldCount; ++i)
      {
        Assert.Equal(r.GetOrdinal(_expectedFieldNames[i]), i);
      }
    }

    [Fact]
    public void TestClose()
    {
      DbDataReader r = CreateDataReaderMock().Object;
      Assert.False(r.IsClosed);
      r.Close();
      Assert.True(r.IsClosed);
    }

    [Fact]
    public void TestGetValues()
    {
      DbDataReader r = CreateDataReaderMock(0).Object;
      Assert.True(r.Read());
      object[] values = new object[_expectedFieldNames.Length];
      int len = r.GetValues(values);
      Assert.Equal(len, _expectedFieldNames.Length);

      for (int i = 0; i < _expectedFieldNames.Length; ++i)
        Assert.Equal(values[i], r.GetValue(i));
    }

    [Fact]
    public void TestGets()
    {
      DbDataReader r = CreateDataReaderMock(0, 1).Object;
      for (int i = 0; i < 2; ++i)
      {
        Assert.True(r.Read());
        var tm = new TestModel
        {
          Sb = (sbyte)r.GetValue(0),
          C = r.GetChar(1),
          S = r.GetInt16(2),
          I = r.GetInt32(3),
          L = r.GetInt64(4),
          B = r.GetByte(5),
          Us = (ushort)r.GetValue(6),
          Ui = (uint)r.GetValue(7),
          Ul = (ulong)r.GetValue(8),
          F = r.GetFloat(9),
          D = r.GetDouble(10),
          Bo = r.GetBoolean(11),
          Str = r.GetString(12),
          Date = r.GetDateTime(13),
          Dec = r.GetDecimal(14),
          En = (YN)r.GetInt32(15)
        };
        TestModelEqual(tm, i);
      }
    }

    [Fact]
    public void TestIsDbNull()
    {
      DbDataReader r = CreateDataReaderMock(1).Object;
      Assert.True(r.Read());
      Assert.True(r.IsDBNull(12));
      Assert.False(r.IsDBNull(0));
      Assert.False(r.IsDBNull(1));
      Assert.False(r.IsDBNull(2));
      Assert.False(r.IsDBNull(3));
    }

    private void TestModelEqual(TestModel tm1, int i)
    {
      TestModel tm2 = _testModelsCollection[i];
      Assert.Equal(tm1.Sb, tm2.Sb);
      Assert.Equal(tm1.C, tm2.C);
      Assert.Equal(tm1.S, tm2.S);
      Assert.Equal(tm1.I, tm2.I);
      Assert.Equal(tm1.L, tm2.L);
      Assert.Equal(tm1.B, tm2.B);
      Assert.Equal(tm1.Us, tm2.Us);
      Assert.Equal(tm1.Ui, tm2.Ui);
      Assert.Equal(tm1.Ul, tm2.Ul);
      Assert.Equal(tm1.F, tm2.F);
      Assert.Equal(tm1.D, tm2.D);
      Assert.Equal(tm1.Bo, tm2.Bo);
      Assert.Equal(tm1.Str, tm2.Str);
      Assert.Equal(tm1.Date, tm2.Date);
      Assert.Equal(tm1.Dec, tm2.Dec);
      Assert.Equal(tm1.En, tm2.En);
    }

    /// <summary>
    /// Create an DbDataReader mock by cloning models from <see cref="_testModelsCollection"/>
    /// </summary>
    /// <param name="indexes">Indexes of models in <see cref="_testModelsCollection"/></param>
    /// <returns></returns>
    private Mock<DbDataReader> CreateDataReaderMock(params int[] indexes)
    {
      List<TestModel> data = indexes.Select(i => (TestModel)_testModelsCollection[i].Clone()).ToList();
      var mock = new Mock<DbDataReader>();
      mock.SetupDataReader(data);
      return mock;
    }
  }
}
