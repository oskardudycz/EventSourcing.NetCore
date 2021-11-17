using System;

namespace Core.Ids;

public interface IIdGenerator
{
    Guid New();
}