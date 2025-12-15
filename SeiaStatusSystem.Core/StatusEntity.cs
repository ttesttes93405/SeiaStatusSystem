using System;

namespace SeiaStatusSystem.Core
{
    public readonly struct StatusEntity<TStatusType>
        where TStatusType : Enum
    {
        int Id { get; init; }
        public StatusEntityToken Token => new(Id);
        public TimeSpan CreateTime { get; init; }
        public IStatusInfo<TStatusType> Info { get; init; }
        public TargetToken TargetToken { get; init; }
        public TargetTypeToken<TStatusType> GetTargetTypeToken() => new(TargetToken, Info.Type);

        public StatusEntity(int id, TimeSpan createTime, IStatusInfo<TStatusType> info, TargetToken targetToken)
        {
            Id = id;
            CreateTime = createTime;
            Info = info;
            TargetToken = targetToken;
        }

        public override string ToString() => $"{nameof(StatusEntity<TStatusType>)}(Id: {Id}, CreateTime: {CreateTime}, Info: {Info}, TargetToken: {TargetToken})";

    }
}