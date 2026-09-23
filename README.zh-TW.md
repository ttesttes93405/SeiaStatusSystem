
# Seia Status System

一款輕量、跨平台且高度解耦的狀態效果系統。

```
^   ^
0 x 0
```

## 🚩 特色

- **確定性時序**：所有狀態更新由開發者明確觸發，讓伺服器端可精確同步、重播。

- **明確的 Scope**：由 `StatusSystem` 建構的 `StatusScope`，可以透過 `using` 語法明確限制作用範圍，避免記憶體洩漏。

- **訂閱狀態**：輕鬆訂閱狀態數值變化，讓副作用可在外部處理，提升可測試性與維護度。

- **TargetToken**：透過 token 來表示狀態附加的目標，任何物件都可以透過 token 成為狀態目標，使系統完全獨立於物件層級。


## 📦 安裝

### 方法1 
在 `manifest.json` 加入下列內容
```json
  "dependencies": {
    "com.owensun.seia-status-system": "https://github.com/ttesttes93405/SeiaStatusSystem.git?path=/SeiaStatusSystem.Unity/Assets/SeiaStatusSystem",
    ...
  }
```

### 方法2 
1. 打開 `Window` → `Package Manager`
2. 點擊 `+` → `Add package from git URL…`
3. 輸入 GitHub repo URL： `https://github.com/ttesttes93405/SeiaStatusSystem.git?path=/SeiaStatusSystem.Unity/Assets/SeiaStatusSystem`


## 📚 使用
以下是使用範例，可以依照需求自由調整

### 階層結構
```
StatusSystem
{
    StatusScope
    {
        StatusEntity[]
    }    
}
```


### 定義 `StatusType`、`StatusInfo`
```csharp
enum MyStatusType
{
    Volume = 1,
    // ...
}
```

```csharp
struct MyStatusInfo : IStatusInfo<MyStatusType>
{
    public MyStatusType Type { get; init; }
    public TimeSpan? Duration { get; init; }
    public float Value { get; init; }
    public Tag Tag { get; init; }
    // Add other fields if needed.
}
```

### 建立 `StatusSystem`

```csharp
public class Game
{
    public void Init()
    {
        // Create a status system from the status type and info type.
        var statusSystem = new StatusSystem<MyStatusType, MyStatusInfo>();
    }
}
```

### `StatusScope`
建立 StatusScope，使用它來附加、移除狀態。所有狀態都只存活在 CreateScope 後、Dispose 前。
此處使用 UniTask 舉例，你可以使用任何你喜歡的方式。
```csharp
async UniTask GamePlay(StatusSystem statusSystem)
{
    var isPlaying = true;
    var startTime = Time.time;

    var student = new Student(name: "Seia");
    var targetToken = new TargetToken(student.GetHashCode());
    
    using (var statusScope = statusSystem.CreateScope())
    {
        using var sub = statusScope.Subscribe(
            targetToken,
            MyStatusType.Volume,
            value =>
            {
                if(value >= 100)
                {
                    Debug.Log("🔈🔈🔈🔈🔈🔈🔈🔈").
                    isPlaying = false;
                }
                else
                {
                    Debug.Log("🔇").
                }
            }
        );

        // game loop
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

            var time = TimeSpan.FromSeconds(Time.time - startTime);
            statusScope.Update(time);  // Update time manually.

            await UniTask.Yield();
        }


    }
    // all status entities disposed after exit scope

}

```
