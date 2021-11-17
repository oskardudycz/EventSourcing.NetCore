namespace Core.Projections;

public interface IProjection
{
    void When(object @event);
}