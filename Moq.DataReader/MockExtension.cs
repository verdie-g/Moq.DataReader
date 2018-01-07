using System;
using System.Collections.Generic;
using System.Data.Common;

namespace Moq.DataReader
{
  public static class MockExtension
  {
    /// <summary>
    /// Mock a DbDataReader using a list
    /// </summary>
    /// <typeparam name="T">Model representing table schema</typeparam>
    /// <param name="mock"></param>
    /// <param name="data">List respresenting a result set</param>
    /// <param name="fieldNames">Optional. Column names. Default is <see cref="T"/>'s properties names</param>
    public static void SetupDataReader<T>(this Mock<DbDataReader> mock, List<T> data, string[] fieldNames = null)
    {
      int row = -1;

      var dataInfo = new DataInfo<T>(data, fieldNames);

      mock.Setup(r => r.FieldCount).Returns(dataInfo.FieldCount);
      mock.Setup(r => r.VisibleFieldCount).Returns(dataInfo.FieldCount);
      mock.Setup(r => r.HasRows).Returns(dataInfo.Data.Count > 0);

      mock.Setup(r => r.NextResult()).Returns(false);
      mock.Setup(r => r.Read())
        .Returns(() => row < dataInfo.Data.Count - 1)
        .Callback(() => row++);

      mock.Setup(r => r.Close()).Callback(() => dataInfo.Closed = true);
      mock.Setup(r => r.IsClosed).Returns(() => dataInfo.Closed);

      mock.Setup(r => r.GetOrdinal(It.IsAny<string>())).Returns<string>(s => dataInfo.GetOrdinal(s));
      mock.Setup(r => r.GetValues(It.IsAny<object[]>())).Returns<object[]>(values => dataInfo.GetValues(row, values));

      for (int i = 0; i < dataInfo.FieldCount; ++i)
      {
        // Wow! Much duplicate! Very engineer
        mock.Setup(r => r.GetName(i)).Returns<int>(col => dataInfo.GetName(col));
        mock.Setup(r => r.GetFieldType(i)).Returns<int>(col => dataInfo.GetFieldType(col));
        mock.Setup(r => r.GetDataTypeName(i)).Returns<int>(col => dataInfo.GetDataTypeName(col));
        mock.Setup(r => r.GetValue(i)).Returns<int>(col => dataInfo.GetValue<object>(row, col));
        mock.Setup(r => r.GetBoolean(i)).Returns<int>(col => dataInfo.GetValue<bool>(row, col));
        mock.Setup(r => r.GetByte(i)).Returns<int>(col => dataInfo.GetValue<byte>(row, col));
        mock.Setup(r => r.GetChar(i)).Returns<int>(col => dataInfo.GetValue<char>(row, col));
        mock.Setup(r => r.GetDateTime(i)).Returns<int>(col => dataInfo.GetValue<DateTime>(row, col));
        mock.Setup(r => r.GetDecimal(i)).Returns<int>(col => dataInfo.GetValue<Decimal>(row, col));
        mock.Setup(r => r.GetDouble(i)).Returns<int>(col => dataInfo.GetValue<double>(row, col));
        mock.Setup(r => r.GetFloat(i)).Returns<int>(col => dataInfo.GetValue<float>(row, col));
        mock.Setup(r => r.GetGuid(i)).Returns<int>(col => dataInfo.GetValue<Guid>(row, col));
        mock.Setup(r => r.GetInt16(i)).Returns<int>(col => dataInfo.GetValue<short>(row, col));
        mock.Setup(r => r.GetInt32(i)).Returns<int>(col => dataInfo.GetValue<int>(row, col));
        mock.Setup(r => r.GetInt64(i)).Returns<int>(col => dataInfo.GetValue<long>(row, col));
        mock.Setup(r => r.GetString(i)).Returns<int>(col => dataInfo.GetValue<string>(row, col));
        mock.Setup(r => r.GetStream(i)).Returns<int>(col => dataInfo.GetStream(row, col));
      }

      /*
      int outOfRangeInt = It.Is<int>(i => i < 0 || i >= dataInfo.FieldCount);

      mock.Setup(r => r.GetName(outOfRangeInt)).Throws<IndexOutOfRangeException>();
      mock.Setup(r => r.GetFieldType(outOfRangeInt)).Throws<IndexOutOfRangeException>();
      mock.Setup(r => r.GetValue(outOfRangeInt)).Throws<IndexOutOfRangeException>();
      mock.Setup(r => r.GetBoolean(outOfRangeInt)).Throws<IndexOutOfRangeException>();
      mock.Setup(r => r.GetByte(outOfRangeInt)).Throws<IndexOutOfRangeException>();
      mock.Setup(r => r.GetChar(outOfRangeInt)).Throws<IndexOutOfRangeException>();
      mock.Setup(r => r.GetDataTypeName(outOfRangeInt)).Throws<IndexOutOfRangeException>();
      mock.Setup(r => r.GetDateTime(outOfRangeInt)).Throws<IndexOutOfRangeException>();
      mock.Setup(r => r.GetDecimal(outOfRangeInt)).Throws<IndexOutOfRangeException>();
      mock.Setup(r => r.GetDouble(outOfRangeInt)).Throws<IndexOutOfRangeException>();
      mock.Setup(r => r.GetFieldType(outOfRangeInt)).Throws<IndexOutOfRangeException>();
      // mock.Setup(r => r.GetFieldValue(outOfRangeInt)).Throws<IndexOutOfRangeException>();
      mock.Setup(r => r.GetFloat(outOfRangeInt)).Throws<IndexOutOfRangeException>();
      mock.Setup(r => r.GetGuid(outOfRangeInt)).Throws<IndexOutOfRangeException>();
      mock.Setup(r => r.GetInt16(outOfRangeInt)).Throws<IndexOutOfRangeException>();
      mock.Setup(r => r.GetInt32(outOfRangeInt)).Throws<IndexOutOfRangeException>();
      mock.Setup(r => r.GetInt64(outOfRangeInt)).Throws<IndexOutOfRangeException>();
      mock.Setup(r => r.GetString(outOfRangeInt)).Throws<IndexOutOfRangeException>();
      mock.Setup(r => r.GetStream(outOfRangeInt)).Throws<IndexOutOfRangeException>();
      */
    }
  }
}
