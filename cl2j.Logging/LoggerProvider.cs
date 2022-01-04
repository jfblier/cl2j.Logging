﻿using cl2j.FileStorage.Core;
using cl2j.FileStorage.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace cl2j.Logging
{
    public class LoggerProvider : ILoggerProvider
    {
#pragma warning disable CA2211 // Non-constant fields should not be visible
        public static LoggerProvider Instance = null!;
#pragma warning restore CA2211 // Non-constant fields should not be visible
        public readonly MemoryLogger MemoryLogger = null!;
        private readonly LoggerOptions options;

        public LoggerProvider(IFileStorageFactory fileStorageFactory, IOptions<LoggerOptions> options, string fileStorageName = "Logger")
        {
            this.options = options.Value;

            if (this.options.MemoryLogs)
                MemoryLogger = new MemoryLogger();

            var fileStorage = fileStorageFactory.Get(fileStorageName);
            if (fileStorage != null)
                BufferedFile = new BufferedFileStorage(fileStorage, this.options.MainFileNamePattern, this.options.MaxSize, this.options.FlushInterval, this.options.ClearOnStartup);
            else
                Debug.WriteLine($"LoggerProvider: FileStorage '{fileStorageName}' not found");
        }

        public BufferedFileStorage BufferedFile { get; } = null!;

        public bool WriteToConsole => options.IncludeConsole;

        public void ClearMemoryLogs()
        {
            if (MemoryLogger != null)
                MemoryLogger.Clear();
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new Logger(this, categoryName, options.Filters, DateTimeProvider.Create(options.TimeZoneName));
        }

        public void Dispose()
        {
            BufferedFile?.Dispose();
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                BufferedFile?.FlushAsync().Wait();
        }
    }
}