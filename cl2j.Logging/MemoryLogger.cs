﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace cl2j.Logging
{
    public class MemoryLogger
    {
        private readonly int maxLines;
        private readonly List<LogData> list;

        public MemoryLogger(int maxLines = 1000)
        {
            this.maxLines = maxLines;
            list = new(maxLines);
        }

        public IList<LogData> GetLogs()
        {
            return list;
        }

        internal void Log(DateTimeOffset dateTime, LogLevel logLevel, Exception ex, string text)
        {
            if (!EnsureCapacityDoNotOverflow())
                return;

            list.Add(new LogData
            {
                Stamp = dateTime,
                Level = logLevel,
                Text = text,
                Exception = ex
            });
        }

        internal void Clear()
        {
            list.Clear();
        }

        private bool EnsureCapacityDoNotOverflow()
        {
            if (list.Count >= maxLines)
            {
                try
                {
                    list.RemoveAt(0);
                }
                catch { }
            }

            return list.Count < maxLines;
        }
    }
}