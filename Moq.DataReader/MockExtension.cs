using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;

namespace Moq.DataReader
{
  public static class MockExtension
  {
    /// <summary>
    /// Mock a DbDataReader
    /// </summary>
    /// <typeparam name="T">Model representing table schema</typeparam>
    /// <param name="mock"></param>
    /// <param name="fieldNames">Optional. Column names. Default is <see cref="T"/>'s properties names</param>
    /// <param name="data">Result set</param>
    public static void SetupDataReader<T>(this Mock<DbDataReader> mock, string[] fieldNames = null, params T[] data)
    {
      SetupDataReader(mock, data, fieldNames);
    }

    /// <summary>
    /// Mock a IDataReader
    /// </summary>
    /// <typeparam name="T">Model representing table schema</typeparam>
    /// <param name="mock"></param>
    /// <param name="fieldNames">Optional. Column names. Default is <see cref="T"/>'s properties names</param>
    /// <param name="data">Result set</param>
    public static void SetupDataReader<T>(this Mock<IDataReader> mock, string[] fieldNames = null, params T[] data)
    {
      SetupDataReader(mock, data, fieldNames);
    }
    
    /// <summary>
    /// Mock a DbDataReader
    /// </summary>
    /// <typeparam name="T">Model representing table schema</typeparam>
    /// <param name="mock"></param>
    /// <param name="data">Result set</param>
    /// <param name="fieldNames">Optional. Column names. Default is <see cref="T"/>'s properties names</param>
    public static void SetupDataReader<T>(this Mock<DbDataReader> mock, IEnumerable<T> data, string[] fieldNames = null)
    {
      var list = data as IReadOnlyList<T> ?? data.ToList();
      SetupDataReader(mock, list, fieldNames);
    }

    /// <summary>
    /// Mock a IDataReader
    /// </summary>
    /// <typeparam name="T">Model representing table schema</typeparam>
    /// <param name="mock"></param>
    /// <param name="data">Result set</param>
    /// <param name="fieldNames">Optional. Column names. Default is <see cref="T"/>'s properties names</param>
    public static void SetupDataReader<T>(this Mock<IDataReader> mock, IEnumerable<T> data, string[] fieldNames = null)
    {
      var list = data as IReadOnlyList<T> ?? data.ToList();
      SetupDataReader(mock, list, fieldNames);
    }
    
    /// <summary>
    /// Mock a DbDataReader using a list
    /// </summary>
    /// <typeparam name="T">Model representing table schema</typeparam>
    /// <param name="mock"></param>
    /// <param name="data">List respresenting a result set</param>
    /// <param name="fieldNames">Optional. Column names. Default is <see cref="T"/>'s properties names</param>
    public static void SetupDataReader<T>(this Mock<DbDataReader> mock, IReadOnlyList<T> data, string[] fieldNames = null)
    {
      var dataInfo = new DataInfo<T>(data, fieldNames);
      SetupDataReader(mock, dataInfo, row => mock.Setup(r => r.GetStream(It.IsAny<int>())).Returns<int>(col => dataInfo.GetStream(row, col)));

      mock.Setup(r => r.VisibleFieldCount).Returns(dataInfo.FieldCount);
      Expression<Func<int, bool>> outOfRange = i => i < 0 || i >= dataInfo.FieldCount;
      mock.Setup(r => r.HasRows).Returns(dataInfo.Data.Count > 0);
            mock.Setup(r => r.GetStream(It.Is(outOfRange))).Throws<IndexOutOfRangeException>();
      //mock.Setup(r => r.GetFieldValue(It.Is(outOfRange))).Throws<IndexOutOfRangeException>();
    }

    /// <summary>
    /// Mock a IDataReader using a list
    /// </summary>
    /// <typeparam name="T">Model representing table schema</typeparam>
    /// <param name="mock"></param>
    /// <param name="data">List respresenting a result set</param>
    /// <param name="fieldNames">Optional. Column names. Default is <see cref="T"/>'s properties names</param>
    public static void SetupDataReader<T>(this Mock<IDataReader> mock, IReadOnlyList<T> data, string[] fieldNames = null)
    {
      var dataInfo = new DataInfo<T>(data, fieldNames);
      SetupDataReader(mock, dataInfo);
    }

    private static void SetupDataReader<TReader, T>(this Mock<TReader> mock, DataInfo<T> dataInfo, Action<int> onRead = null)
        where TReader : class, IDataReader
    {
      int row = -1;
      mock.Setup(r => r.FieldCount).Returns(dataInfo.FieldCount);

      mock.Setup(r => r[It.IsAny<string>()]).Returns((string name) => dataInfo.GetValue<object>(row, dataInfo.GetOrdinal(name)));
      mock.Setup(r => r[It.IsAny<int>()]).Returns((int ordinal) => dataInfo.GetValue<object>(row, ordinal));

      mock.Setup(r => r.NextResult()).Returns(false);
      mock.Setup(r => r.Read())
        .Returns(() => row < dataInfo.Data.Count - 1)
        .Callback(() =>
        {
          row++;
          onRead?.Invoke(row);
        });

      mock.Setup(r => r.Close()).Callback(() => dataInfo.Closed = true);
      mock.Setup(r => r.IsClosed).Returns(() => dataInfo.Closed);

      mock.Setup(r => r.GetOrdinal(It.IsAny<string>())).Returns<string>(s => dataInfo.GetOrdinal(s));
      mock.Setup(r => r.GetValues(It.IsAny<object[]>())).Returns<object[]>(values => dataInfo.GetValues(row, values));

      // Wow! Much duplicate! Very engineer
      mock.Setup(r => r.GetName(It.IsAny<int>())).Returns<int>(col => dataInfo.GetName(col));
      mock.Setup(r => r.GetFieldType(It.IsAny<int>())).Returns<int>(col => dataInfo.GetFieldType(col));
      mock.Setup(r => r.GetDataTypeName(It.IsAny<int>())).Returns<int>(col => dataInfo.GetDataTypeName(col));
      mock.Setup(r => r.IsDBNull(It.IsAny<int>())).Returns<int>(col => dataInfo.GetValue<object>(row, col) == null);
      mock.Setup(r => r.GetValue(It.IsAny<int>())).Returns<int>(col => dataInfo.GetValue<object>(row, col));
      mock.Setup(r => r.GetBoolean(It.IsAny<int>())).Returns<int>(col => dataInfo.GetValue<bool>(row, col));
      mock.Setup(r => r.GetByte(It.IsAny<int>())).Returns<int>(col => dataInfo.GetValue<byte>(row, col));
      mock.Setup(r => r.GetChar(It.IsAny<int>())).Returns<int>(col => dataInfo.GetValue<char>(row, col));
      mock.Setup(r => r.GetDateTime(It.IsAny<int>())).Returns<int>(col => dataInfo.GetValue<DateTime>(row, col));
      mock.Setup(r => r.GetDecimal(It.IsAny<int>())).Returns<int>(col => dataInfo.GetValue<Decimal>(row, col));
      mock.Setup(r => r.GetDouble(It.IsAny<int>())).Returns<int>(col => dataInfo.GetValue<double>(row, col));
      mock.Setup(r => r.GetFloat(It.IsAny<int>())).Returns<int>(col => dataInfo.GetValue<float>(row, col));
      mock.Setup(r => r.GetGuid(It.IsAny<int>())).Returns<int>(col => dataInfo.GetValue<Guid>(row, col));
      mock.Setup(r => r.GetInt16(It.IsAny<int>())).Returns<int>(col => dataInfo.GetValue<short>(row, col));
      mock.Setup(r => r.GetInt32(It.IsAny<int>())).Returns<int>(col => dataInfo.GetValue<int>(row, col));
      mock.Setup(r => r.GetInt64(It.IsAny<int>())).Returns<int>(col => dataInfo.GetValue<long>(row, col));
      mock.Setup(r => r.GetString(It.IsAny<int>())).Returns<int>(col => dataInfo.GetValue<string>(row, col));

      Expression<Func<int, bool>> outOfRange = i => i < 0 || i >= dataInfo.FieldCount;
      mock.Setup(r => r.GetName(It.Is(outOfRange))).Throws<IndexOutOfRangeException>();
      mock.Setup(r => r.GetFieldType(It.Is(outOfRange))).Throws<IndexOutOfRangeException>();
      mock.Setup(r => r.GetValue(It.Is(outOfRange))).Throws<IndexOutOfRangeException>();
      mock.Setup(r => r.GetBoolean(It.Is(outOfRange))).Throws<IndexOutOfRangeException>();
      mock.Setup(r => r.GetByte(It.Is(outOfRange))).Throws<IndexOutOfRangeException>();
      mock.Setup(r => r.GetChar(It.Is(outOfRange))).Throws<IndexOutOfRangeException>();
      mock.Setup(r => r.GetDataTypeName(It.Is(outOfRange))).Throws<IndexOutOfRangeException>();
      mock.Setup(r => r.GetDateTime(It.Is(outOfRange))).Throws<IndexOutOfRangeException>();
      mock.Setup(r => r.GetDecimal(It.Is(outOfRange))).Throws<IndexOutOfRangeException>();
      mock.Setup(r => r.GetDouble(It.Is(outOfRange))).Throws<IndexOutOfRangeException>();
      mock.Setup(r => r.GetFieldType(It.Is(outOfRange))).Throws<IndexOutOfRangeException>();
      mock.Setup(r => r.GetFloat(It.Is(outOfRange))).Throws<IndexOutOfRangeException>();
      mock.Setup(r => r.GetGuid(It.Is(outOfRange))).Throws<IndexOutOfRangeException>();
      mock.Setup(r => r.GetInt16(It.Is(outOfRange))).Throws<IndexOutOfRangeException>();
      mock.Setup(r => r.GetInt32(It.Is(outOfRange))).Throws<IndexOutOfRangeException>();
      mock.Setup(r => r.GetInt64(It.Is(outOfRange))).Throws<IndexOutOfRangeException>();
      mock.Setup(r => r.GetString(It.Is(outOfRange))).Throws<IndexOutOfRangeException>();
    }
  }
}
