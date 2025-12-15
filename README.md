# Seia Status System

A lightweight, cross‑platform, highly decoupled status‑effect system.

```
^   ^
0 x 0
```


## 🚩 Features
- **Deterministic Timing** : All status updates are explicitly triggered by the developer, enabling precise synchronization, replay, or back‑fill on the server side.
- **Explicit Scope** : The `StatusSystem` constructs a `StatusScope` that can be scoped with a using block, guaranteeing that all statuses are automatically disposed and preventing memory leaks.
- **Event‑Based Subscriptions** : Easily subscribe to status value changes, keeping side effects separate for better testability and maintainability.
- **TargetToken** : Any object can become a status target via a token, making the system fully agnostic to your object hierarchy




## 📦 Installation

Add via `manifest.json`
```json
"dependencies": {
  "com.owensun.seia-status-system": "https://github.com/ttesttes93405/SeiaStatusSystem.git?path=/SeiaStatusSystem.Unity/Assets/SeiaStatusSystem",
  ...
}
```

Or add via Package Manager

1. `Window` → `Package Manager`
2. Click + → `Add package from git URL…`
3. Paste `https://github.com/ttesttes93405/SeiaStatusSystem.git?path=/SeiaStatusSystem.Unity/Assets/SeiaStatusSystem`




## 📚 Quick Start
Below is a minimal example that demonstrates the core concepts.

### Structure
```
StatusSystem
{
    StatusScope
    {
        StatusEnity[]
    }    
}
```

### Define your `StatusType` & `StatusInfo`
```csharp
public enum MyStatusType
{
    Volume = 1,
    // …add more
}

public struct MyStatusInfo : IStatusInfo<MyStatusType>
{
    public MyStatusType Type { get; init; }
    public TimeSpan? Duration { get; init; }
    public float Value { get; init; }
    public Tag Tag { get; init; }
    // add other fields if needed
}
```

### Create a `StatusSystem`
```csharp
var statusSystem = new StatusSystem<MyStatusType, MyStatusInfo>();
```

### Use a `StatusScope` (best inside a using block)
```csharp
async UniTask GamePlay(StatusSystem<MyStatusType, MyStatusInfo> statusSystem)
{
    var isPlaying = true;
    var startTime = Time.time;

    var student = new Student(name: "Seia");
    var targetToken = new TargetToken(student.GetHashCode());

    // Create a scope – all status entities are disposed when it ends
    using (var statusScope = statusSystem.CreateScope())
    {
        // Subscribe to a value change
        using var sub = statusScope.Subscribe(
            targetToken,
            MyStatusType.Volume,
            (value) =>
            {
                if (value >= 100)
                {
                    Debug.Log("🔈🔈🔈🔈🔈🔈🔈🔈");
                    isPlaying = false;
                }
                else
                {
                    Debug.Log("🔇");
                }
            });

        // Game loop
        while (isPlaying)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                statusScope.Apply(
                    targetToken,
                    new MyStatusInfo()
                    {
                        Type = MyStatusType.Volume,
                        Value = 10,
                        Duration = TimeSpan.FromSeconds(3),
                    }
                );
            }

            var elapsed = TimeSpan.FromSeconds(Time.time - startTime);
            statusScope.Update(elapsed);         // Manual time tick

            await UniTask.Yield();               // Wait until next frame
        }
    }    // All statuses cleaned up automatically here


}
```

## ⚙️ Note
- StatusScope.Update can be called from any tick loop (Unity Update, server loop, coroutine, etc.).

