using Xunit;
using Core.Structures;
using static Core.Structures.EitherExtensions;

namespace Decider;

public enum FileOpeningError
{
    FileDoesNotExist
}

public class FileProcessor
{
    public Either<FileStream, FileOpeningError> ReadFile(string fileName)
    {
        if (File.Exists(fileName))
            return new Either<FileStream, FileOpeningError>(FileOpeningError.FileDoesNotExist);

        return new Either<FileStream, FileOpeningError>(File.Open(fileName, FileMode.Open));
    }
}


public class TextFormatter
{
    public abstract record FormattedValue
    {
        private FormattedValue() { }

        public record DateTime(
            DateTimeOffset Value
        ): FormattedValue;

        public record Milliseconds(
            long Value
        ): FormattedValue;
    }

    public static string Format((DateTimeOffset? DateTime, long? Milliseconds) date)
    {
        var (dateTime, milliseconds) = date;

        if (!dateTime.HasValue && !milliseconds.HasValue)
            throw new ArgumentException(nameof(date),
                $"Either {nameof(date.DateTime)} or {nameof(date.Milliseconds)} needs to be set");

        return dateTime.HasValue ?
            dateTime.Value.ToString()
            : DateTimeOffset.FromUnixTimeMilliseconds(milliseconds!.Value).ToString();
    }

    public static string FormatV2((DateTimeOffset? DateTime, long? Milliseconds) dateTime) =>
        dateTime.Map(
            date => date.ToString(),
            milliseconds => DateTimeOffset.FromUnixTimeMilliseconds(milliseconds).ToString()
        )!;

    public static string FormatV3(Either<DateTimeOffset, long> dateTime) =>
        dateTime.Map(
            date => date.ToString(),
            milliseconds => DateTimeOffset.FromUnixTimeMilliseconds(milliseconds).ToString()
        );

    public static string FormatV4(FormattedValue formatted) =>
        formatted switch
        {
            FormattedValue.DateTime(var value) => value.ToString(),
            FormattedValue.Milliseconds(var value) => value.ToString(),
            _ => throw new ArgumentOutOfRangeException(nameof(formatted), "That should never happen!")
        };
}

public class UnionTypesTests
{
    [Fact]
    public void Test1()
    {
        var dateTime = DateTimeOffset.Now;
        var milliseconds = dateTime.ToUnixTimeMilliseconds();

        TextFormatter.Format((dateTime, null));
        TextFormatter.Format((null, milliseconds));

        TextFormatter.FormatV2(Either<DateTimeOffset, long>(DateTimeOffset.Now));
        TextFormatter.FormatV2(Either<DateTimeOffset, long>(milliseconds));

        TextFormatter.FormatV3(new Either<DateTimeOffset, long>(dateTime));
        TextFormatter.FormatV3(new Either<DateTimeOffset, long>(milliseconds));

        TextFormatter.FormatV4(new TextFormatter.FormattedValue.DateTime(dateTime));
        TextFormatter.FormatV4(new TextFormatter.FormattedValue.Milliseconds(milliseconds));

var events = new ShoppingCartEvent[]
{
    // (...)
};

var currentState = events.Aggregate(ShoppingCart.Empty, ShoppingCart.Evolve);
    }
}
