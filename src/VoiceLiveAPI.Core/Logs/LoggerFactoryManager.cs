// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Logs
{
    /// <summary>
    ///     Represents a class that provides a logger output.
    /// </summary>
    public interface ILogOutputClass
    {
        /// <summary>
        ///     Gets the <see cref="ILogger" /> instance for logging.
        /// </summary>
        ILogger Logger { get; }
    }

    /// <summary>
    ///     Provides a singleton manager for application-wide <see cref="ILoggerFactory" /> and logger creation.
    /// </summary>
    public class LoggerFactoryManager
    {
        private static LoggerFactoryManager appLogger;
        private ILoggerFactory factory = NullLoggerFactory.Instance;

        /// <summary>
        ///     Gets the singleton instance of <see cref="LoggerFactoryManager" />.
        /// </summary>
        public static LoggerFactoryManager Instance => appLogger ?? (appLogger = new LoggerFactoryManager());

        /// <summary>
        ///     Gets the current <see cref="ILoggerFactory" /> instance used for logging.
        /// </summary>
        public static ILoggerFactory Current => Instance.factory;

        private LoggerFactoryManager()
        {
        }

        /// <summary>
        ///     Creates a logger for the specified category name.
        /// </summary>
        /// <param name="categoryName">The category name for messages produced by the logger.</param>
        /// <returns>An <see cref="ILogger" /> instance for the specified category.</returns>
        public static ILogger CreateLogger(string categoryName)
        {
            return Instance.factory.CreateLogger(categoryName);
        }

        /// <summary>
        ///     Creates a logger for the specified generic type.
        /// </summary>
        /// <typeparam name="T">The type whose name is used for the logger category.</typeparam>
        /// <returns>An <see cref="ILogger{T}" /> instance for the specified type.</returns>
        public static ILogger<T> CreateLogger<T>()
        {
            return Instance.factory.CreateLogger<T>();
        }

        /// <summary>
        ///     Sets the <see cref="ILoggerFactory" /> instance to be used for logging.
        /// </summary>
        /// <param name="factory">The <see cref="ILoggerFactory" /> instance to set.</param>
        public static void Set(ILoggerFactory factory)
        {
            Instance.factory = factory;
        }
    }
}