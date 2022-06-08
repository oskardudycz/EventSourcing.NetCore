namespace Helpdesk.Api.Core.Http;

public static class ETagExtensions
{
    public static int ToExpectedVersion(string? eTag)
    {
        if (eTag is null)
            throw new ArgumentNullException(nameof(eTag));

        return int.Parse(eTag.Substring(1, eTag.Length - 2));
    }
}
