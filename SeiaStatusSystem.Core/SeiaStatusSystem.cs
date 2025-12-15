using System;

namespace SeiaStatusSystem.Core
{
    public class StatusSystem<TStatusType, TStatusInfo>
        where TStatusType : Enum
        where TStatusInfo : IStatusInfo<TStatusType>
    {

        public StatusSystem()
        {
        }

        public StatusScope<TStatusType, TStatusInfo> CreateScope()
        {
            return new StatusScope<TStatusType, TStatusInfo>();
        }
    }
}