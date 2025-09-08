using Rusleo.Utils.Runtime.Logging.Filters;
using Rusleo.Utils.Runtime.Logging.Formatters;
using Rusleo.Utils.Runtime.Logging.Sinks;
using UnityEngine;

namespace Rusleo.Utils.Runtime.Logging
{
    public static class LoggingBootstrap
    {
        private const string RES_PATH = "RusleoLoggingSettings";
        private static bool _initialized;

        private static ILogFormatter MakeFormatter(FormatterKind kind, DefaultLogFormatterOptions opt)
        {
            return kind switch
            {
                FormatterKind.Minimal => new MinimalLineFormatter(),
                FormatterKind.Json => new JsonLogFormatter(),
                _ => new DefaultLogFormatter(opt),
            };
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            if (_initialized) return;
            _initialized = true;

            var cfg = Resources.Load<LogConfig>("RusleoLoggingSettings") ??
                      ScriptableObject.CreateInstance<LogConfig>();
            var disp = LogDispatcher.Instance;
            disp.Dispose();
            disp.AddFilter(new LevelFilter(cfg.minimumLevel));

            // Unity Console
            if (cfg.unityConsole.enabled)
            {
                var f = MakeFormatter(cfg.unityConsole.formatter, cfg.defaultFormatterOptions);
                disp.AddSink(new UnityConsoleSink(f));
            }

            // File
            if (cfg.fileSink.enabled)
            {
                var f = MakeFormatter(cfg.fileSink.formatter, cfg.defaultFormatterOptions);
                disp.AddSink(new FileSink(
                    f,
                    directory: System.IO.Path.Combine(UnityEngine.Application.persistentDataPath,
                        cfg.relativeDirectory),
                    fileName: cfg.fileName,
                    maxBytes: cfg.maxFileBytes));
            }

            Log.Info("Rusleo.Utils.Logging initialized");
        }
    }
}