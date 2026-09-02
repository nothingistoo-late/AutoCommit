

### 💡 Leverage ValueTask<T> to Avoid Async Allocations
* **Category**: `async` | **Timestamp**: `2026-08-28 10:31:25`

```csharp
public ValueTask<int> GetCachedCountAsync() =>
    _cached.HasValue ? new ValueTask<int>(_cached.Value) : new ValueTask<int>(FetchAsync());
```
