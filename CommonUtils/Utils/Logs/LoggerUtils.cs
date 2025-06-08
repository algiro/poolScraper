using log4net.Core;
using log4net.Repository.Hierarchy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace CommonUtils.Utils.Logs
{
    public static class LoggerUtils
    {
        private static readonly Microsoft.Extensions.Logging.ILoggerFactory LoggerFactory = InitLoggerFactory();

        private static Microsoft.Extensions.Logging.ILoggerFactory InitLoggerFactory()
        {
            return Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
            {
                builder.AddSimpleConsole();
            });
        }
        public static void AddConsoleLogger(this IServiceCollection services) => services.AddLogging(AddSimpleConsole);
        private static void AddSimpleConsole(ILoggingBuilder logginBuilder)
        {
            logginBuilder
                    .AddSimpleConsole(options =>
                    {
                        options.IncludeScopes = true;
                        options.SingleLine = true;
                        options.TimestampFormat = "HH:mm:ss.FFFFFF ";
                    })
                    .AddDebug(); // Add debug logging
        }
        public static Microsoft.Extensions.Logging.ILogger<T> CreateLogger<T>()
        {
            return LoggerFactory.CreateLogger<T>();
        }

        public static Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName)
        {
            return LoggerFactory.CreateLogger(categoryName);
        }
    }
    public static class LoggerExtensions
    {
        private static readonly HashSet<string> LoggedMessages = new HashSet<string>();
        public static void LogOnce(this Microsoft.Extensions.Logging.ILogger logger, LogLevel logLevel, string message)
        {
            if (logger.IsEnabled(logLevel) && !LoggedMessages.Contains(message))
            {
                LoggedMessages.Add(message);
                logger.Log(logLevel, message);
            }
        }
    }
}
