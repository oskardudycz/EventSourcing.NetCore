namespace Core.Projections;

public interface IProjection
{
    void Evolve(object @event);
}

public interface IVersionedProjection: IProjection
{
    public ulong LastProcessedPosition { get; set; }
}
