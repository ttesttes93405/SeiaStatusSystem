using UnityEngine;
using SeiaStatusSystem.Core;
using System;

public class Sample : MonoBehaviour
{

    enum StatusType
    {
        None = 0,
        Atk = 1,
        Hp = 2,
        Def = 3,
    }

    readonly struct StatusInfo : IStatusInfo<StatusType>
    {
        public StatusType Type { get; init; }
        public TimeSpan? Duration { get; init; }
        public float Value { get; init; }
        public Tag Tag { get; init; }

        public StatusInfo(StatusType type, float value)
        {
            Type = type;
            Value = value;
            Duration = null;
            Tag = Tag.None;
        }
    }


    class Entity
    {
        readonly GameObject entity;
        readonly GameObject hp;
        readonly GameObject atk;

        public TargetToken TargetToken => new(entity.GetHashCode());

        public bool IsDead { get; private set; } = false;

        public Entity(string name)
        {
            entity = new GameObject(name);
            hp = new GameObject("Hp");
            atk = new GameObject("Atk");
            hp.transform.parent = entity.transform;
            atk.transform.parent = entity.transform;
        }
        public void SetHp(float value)
        {
            if (IsDead == false)
                hp.name = $"Hp: {value switch { <= 0 => 0, _ => value }}";
        }
        public void SetAtk(float value)
        {
            if (IsDead == false)
                atk.name = $"Atk: {value}";
        }

        public void Died()
        {
            if (IsDead)
                return;
            entity.name += " (Dead)";
            Destroy(hp);
            Destroy(atk);
            IsDead = true;
        }

        public void Add(GameObject obj)
        {
            obj.transform.parent = entity.transform;
        }

        public void Remove(GameObject obj)
        {
            Destroy(obj);
        }
    }



    StatusSystem<StatusType, StatusInfo> seiaStatusSystem;
    StatusScope<StatusType, StatusInfo> statusScope;

    float startTime;
    Action<TimeSpan> onTick;


    Entity player;
    Entity monster;

    void Start()
    {
        startTime = Time.time;
        onTick = CreateTimer(TimeSpan.Zero);

        seiaStatusSystem = new StatusSystem<StatusType, StatusInfo>();
        statusScope = seiaStatusSystem.CreateScope();

        player = SpawnEntity("Player", initialHp: 100, initialAtk: 20);
        monster = SpawnEntity("Monster", initialHp: 150, initialAtk: 5);


        Entity SpawnEntity(string name, float initialHp, float initialAtk)
        {
            var entity = new Entity(name);

            statusScope.Apply(
                entity.TargetToken,
                new StatusInfo(StatusType.Hp, initialHp)
            );

            statusScope.Apply(
                entity.TargetToken,
                new StatusInfo(StatusType.Atk, initialAtk)
            );

            statusScope.Subscribe(
                entity.TargetToken,
                StatusType.Hp,
                value =>
                {
                    if (value <= 0)
                    {
                        entity.Died();
                        statusScope.CleanTarget(entity.TargetToken);
                    }
                    else
                    {
                        entity.SetHp(value);
                    }
                },
                executeAfterSubscribe: false
            );

            statusScope.Subscribe(
                entity.TargetToken,
                StatusType.Atk,
                value => entity.SetAtk(value),
                executeAfterSubscribe: false
            );

            return entity;
        }


    }

    StatusEntityToken defToken = StatusEntityToken.None;

    void Update()
    {
        var currentTime = Time.time;
        var duration = TimeSpan.FromSeconds(currentTime - startTime);

        statusScope.Update(duration);

        onTick(duration);




        if (Input.GetKeyDown(KeyCode.Space) && monster.IsDead == false)
        {
            var atk = statusScope.GetStatusValue(
                player.TargetToken,
                StatusType.Atk
            );
            statusScope.Apply(
                monster.TargetToken,
                new StatusInfo(StatusType.Hp, -atk)
            );
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            defToken = statusScope.Apply(
                player.TargetToken,
                new StatusInfo(StatusType.Def, 10)
            );
            statusScope.SuscribeEffect(defToken, () =>
            {
                var d = statusScope.GetStatusValue(
                    player.TargetToken,
                    StatusType.Def
                );

                var g = new GameObject($"Def: {d}");
                player.Add(g);

                return () =>
                {
                    player.Remove(g);
                };
            });
        }
        else if (Input.GetKeyUp(KeyCode.D))
        {
            statusScope.RemoveByEntityToken(defToken);
        }

    }

    void OnDestroy()
    {
        statusScope.Dispose();
    }


    Action<TimeSpan> CreateTimer(TimeSpan initialTime)
    {
        var timer = new GameObject("Timer");
        OnTick(initialTime);

        return OnTick;

        void OnTick(TimeSpan currentTime)
        {
            timer.name = $"Timer ({currentTime.TotalSeconds:F1}s)";
        }

    }

}
