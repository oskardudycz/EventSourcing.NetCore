using System;
using Core.Aggregates;

namespace Core.Ids
{
    public interface IIdGenerator
    {
        Guid New();
    }
}
