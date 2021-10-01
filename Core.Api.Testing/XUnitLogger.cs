using System;
using System.Text;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Core.Api.Testing
{
    // From: https://www.meziantou.net/how-to-get-asp-net-core-logs-in-the-output-of-xunit-tests.htm
    internal class XUnitLogger: ILogger
    {
        private readonly IMessageSink messageSink;
        private readonly string categoryName;
        private readonly LoggerExternalScopeProvider scopeProvider;

        public static ILogger CreateLogger(IMessageSink messageSink) =>
            new XUnitLogger(messageSink, new LoggerExternalScopeProvider(), "");

        public static ILogger<T> CreateLogger<T>(IMessageSink messageSink) =>
            new XUnitLogger<T>(messageSink, new LoggerExternalScopeProvider());

        public XUnitLogger(IMessageSink messageSink, LoggerExternalScopeProvider scopeProvider,
            string categoryName)
        {
            this.messageSink = messageSink;
            this.scopeProvider = scopeProvider;
            this.categoryName = categoryName;
        }

        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

        public IDisposable BeginScope<TState>(TState state) => scopeProvider.Push(state);

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            var sb = new StringBuilder();
            sb.Append(GetLogLevelString(logLevel))
                .Append(" [").Append(categoryName).Append("] ")
                .Append(formatter(state, exception));

            if (exception != null)
            {
                sb.Append('\n').Append(exception);
            }

            // Append scopes
            scopeProvider.ForEachScope((scope, builder) =>
            {
                builder.Append("\n => ");
                builder.Append(scope);
            }, sb);

            messageSink.OnMessage(new DiagnosticMessage(sb.ToString()));
        }

        private static string GetLogLevelString(LogLevel logLevel)
        {
            return logLevel switch
            {
                LogLevel.Trace => "trce",
                LogLevel.Debug => "dbug",
                LogLevel.Information => "info",
                LogLevel.Warning => "warn",
                LogLevel.Error => "fail",
                LogLevel.Critical => "crit",
                _ => throw new ArgumentOutOfRangeException(nameof(logLevel))
            };
        }
    }

    internal sealed class XUnitLogger<T> : XUnitLogger, ILogger<T>
    {
        public XUnitLogger(IMessageSink messageSink, LoggerExternalScopeProvider scopeProvider)
            : base(messageSink, scopeProvider, typeof(T).FullName!)
        {
        }
    }

    internal sealed class XUnitLoggerProvider : ILoggerProvider
    {
        private readonly IMessageSink messageSink;
        private readonly LoggerExternalScopeProvider scopeProvider = new();

        public XUnitLoggerProvider(IMessageSink messageSink)
        {
            this.messageSink = messageSink;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new XUnitLogger(messageSink, scopeProvider, categoryName);
        }

        public void Dispose()
        {
        }
    }
}
