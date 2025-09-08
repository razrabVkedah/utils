using System.Text;

namespace Rusleo.Utils.Runtime.Logging.Formatters
{
    public sealed class JsonLogFormatter : ILogFormatter
    {
        public string Format(in LogEvent e)
        {
            var sb = new StringBuilder(512);
            sb.Append('{');

            AppendProp(sb, "ts", e.UtcTimestamp.ToString("o"));
            sb.Append(',');
            AppendProp(sb, "level", e.Level.ToString());
            sb.Append(',');
            AppendProp(sb, "owner", e.Owner);
            sb.Append(',');
            AppendProp(sb, "msg", e.Message);
            sb.Append(',');

            // tags (array)
            sb.Append("\"tags\":[");
            if (e.Tags != null && e.Tags.Length > 0)
            {
                for (int i = 0; i < e.Tags.Length; i++)
                {
                    if (i > 0) sb.Append(',');
                    AppendString(sb, e.Tags[i]);
                }
            }

            sb.Append("],");

            // metadata (object)
            sb.Append("\"meta\":{");
            if (e.Metadata != null && e.Metadata.Count > 0)
            {
                int i = 0;
                foreach (var kv in e.Metadata)
                {
                    if (i++ > 0) sb.Append(',');
                    AppendProp(sb, kv.Key, kv.Value?.ToString());
                }
            }

            sb.Append("},");

            AppendProp(sb, "corr", e.CorrelationId);
            sb.Append(',');
            sb.Append("\"threadId\":").Append(e.ThreadId).Append(',');
            AppendProp(sb, "thread", e.ThreadName);
            sb.Append(',');

            if (e.Exception != null)
                AppendProp(sb, "exception", e.Exception.ToString());
            else
                sb.Append("\"exception\":null");

            sb.Append('}');
            return sb.ToString();
        }

        private static void AppendProp(StringBuilder sb, string key, string value)
        {
            AppendString(sb, key);
            sb.Append(':');
            if (value != null) AppendString(sb, value);
            else sb.Append("null");
        }

        private static void AppendString(StringBuilder sb, string s)
        {
            sb.Append('"');
            if (s != null)
            {
                foreach (var c in s)
                {
                    switch (c)
                    {
                        case '\\': sb.Append("\\\\"); break;
                        case '\"': sb.Append("\\\""); break;
                        case '\n': sb.Append("\\n"); break;
                        case '\r': sb.Append("\\r"); break;
                        case '\t': sb.Append("\\t"); break;
                        default:
                            if (char.IsControl(c))
                                sb.Append("\\u").Append(((int)c).ToString("x4"));
                            else
                                sb.Append(c);
                            break;
                    }
                }
            }

            sb.Append('"');
        }
    }
}