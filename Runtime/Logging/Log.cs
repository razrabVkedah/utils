using System;
using UnityEngine;

namespace Rusleo.Utils.Runtime.Logging
{
    public static class Log
    {
        // Быстрые вызовы без контекста
        public static void Trace(object msg) => Write(LogLevel.Trace, msg?.ToString());
        public static void Debug(object msg) => Write(LogLevel.Debug, msg?.ToString());
        public static void Info(object msg) => Write(LogLevel.Info, msg?.ToString());
        public static void Warn(object msg) => Write(LogLevel.Warn, msg?.ToString());
        public static void Error(object msg, Exception ex = null) => Write(LogLevel.Error, msg?.ToString(), ex);
        public static void Fatal(object msg, Exception ex = null) => Write(LogLevel.Fatal, msg?.ToString(), ex);

        // Перегрузки с контекстом Unity
        public static void Trace(object msg, UnityEngine.Object ctx) =>
            Write(LogLevel.Trace, msg?.ToString(), null, ctx);

        public static void Debug(object msg, UnityEngine.Object ctx) =>
            Write(LogLevel.Debug, msg?.ToString(), null, ctx);

        public static void Info(object msg, UnityEngine.Object ctx) => Write(LogLevel.Info, msg?.ToString(), null, ctx);
        public static void Warn(object msg, UnityEngine.Object ctx) => Write(LogLevel.Warn, msg?.ToString(), null, ctx);

        public static void Error(object msg, UnityEngine.Object ctx, Exception ex = null) =>
            Write(LogLevel.Error, msg?.ToString(), ex, ctx);

        public static void Fatal(object msg, UnityEngine.Object ctx, Exception ex = null) =>
            Write(LogLevel.Fatal, msg?.ToString(), ex, ctx);

        private static void Write(LogLevel level, string text, Exception ex = null, UnityEngine.Object ctx = null)
        {
            var e = new LogEvent(level, text, ex, ctx);
            LogDispatcher.Instance.Emit(in e);
        }
    }
}