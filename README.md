# IDataReader extension for Moq

## Usage
```csharp
var data = new List<T>() { ... };
var mock = new Mock<IDataReader>();
mock.SetupDataReader(data);
IDataReader r = mock.Object;
```

## Not implemented

These methods are not implemented and will return default value:
- Depth
- RecordAffected