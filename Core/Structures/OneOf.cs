namespace Core.Structures;

public class OneOf<T1, T2, T3>
{
    public Maybe<T1> First { get; }
    public Maybe<T2> Second { get; }
    public Maybe<T3> Third { get; }

    public OneOf(T1 value)
    {
        First = Maybe<T1>.Of(value);
        Second = Maybe<T2>.Empty;
        Third = Maybe<T3>.Empty;
    }

    public OneOf(T2 value)
    {
        First = Maybe<T1>.Empty;
        Second = Maybe<T2>.Of(value);
        Third = Maybe<T3>.Empty;
    }

    public OneOf(T3 value)
    {
        First = Maybe<T1>.Empty;
        Second = Maybe<T2>.Empty;
        Third = Maybe<T3>.Of(value);
    }

    public OneOf((T1? First, T2? Second, T3? Third) value)
    {
        First = value.First != null ? Maybe<T1>.Of(value.First) : Maybe<T1>.Empty;
        Second = value.Second != null ? Maybe<T2>.Of(value.Second) : Maybe<T2>.Empty;
        Third = value.Third != null ? Maybe<T3>.Of(value.Third) : Maybe<T3>.Empty;
    }

    public TMapped Map<TMapped>(
        Func<T1, TMapped> mapT1,
        Func<T2, TMapped> mapT2,
        Func<T3, TMapped> mapT3
    )
    {
        if (First.IsPresent)
            return mapT1(First.GetOrThrow());

        if (Second.IsPresent)
            return mapT2(Second.GetOrThrow());

        if (Third.IsPresent)
            return mapT3(Third.GetOrThrow());

        throw new Exception("That should never happen!");
    }

    public void Switch(
        Action<T1> onT1,
        Action<T2> onT2,
        Action<T3> onT3
    )
    {
        if (First.IsPresent)
        {
            onT1(First.GetOrThrow());
            return;
        }

        if (Second.IsPresent)
        {
            onT2(Second.GetOrThrow());
            return;
        }

        if (Third.IsPresent)
        {
            onT3(Third.GetOrThrow());
            return;
        }

        throw new Exception("That should never happen!");
    }
}

public static class OneOfExtensions
{
    public static void Map<T1, T2, T3, TMapped>(
        this (T1? First, T2? Second, T3? Third) value,
        Func<T1, TMapped> mapT1,
        Func<T2, TMapped> mapT2,
        Func<T3, TMapped> mapT3
    ) => new OneOf<T1, T2, T3>(value).Map(mapT1, mapT2, mapT3);

    public static void Switch<T1, T2, T3, TMapped>(
        this (T1? First, T2? Second, T3? Third) value,
        Action<T1> onT1,
        Action<T2> onT2,
        Action<T3> onT3
    ) => new OneOf<T1, T2, T3>(value).Switch(onT1, onT2, onT3);
}
