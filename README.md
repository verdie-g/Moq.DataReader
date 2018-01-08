# DbDataReader extension for Moq

## Usage
```csharp
var data = new List<T>() { ... };
var mock = new Mock<DbDataReader>();
mock.SetupDataReader(data);
DbDataReader r = mock.Object;
```

## Not implemented

These methods are not implemented and will return default value:
- Depth
- RecordAffected
- GetFieldValue
- NextResult
- CanGetColumnSchema
- GetColumnSchema
- GetLifeTimeService
- GetTextReader
- IsDbNullAsync
- ReadAsync