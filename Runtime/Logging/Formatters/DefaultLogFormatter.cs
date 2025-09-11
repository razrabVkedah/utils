using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Rusleo.Utils.Runtime.Logging.Formatters
{
    public sealed class DefaultLogFormatter : ILogFormatter
    {
        private readonly DefaultLogFormatterOptions _opt;

        public DefaultLogFormatter(DefaultLogFormatterOptions opt = null)
            => _opt = opt ?? new DefaultLogFormatterOptions();

        public string Format(in LogEvent e)
        {
            var sb = new StringBuilder(256);

            if (_opt.IncludeTimestamp)
                sb.Append(e.UtcTimestamp.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")).Append(' ');

            sb.Append('[').Append(e.Level).Append(']').Append(' ');

            if (_opt.IncludeOwner && !string.IsNullOrEmpty(e.Owner))
                sb.Append('{').Append(e.Owner).Append("} ");

            sb.Append(e.Message ?? string.Empty);

            if (_opt.IncludeTags && e.Tags is { Length: > 0 })
                sb.Append("  [#").Append(string.Join(" #", e.Tags)).Append(']');

            if (_opt.IncludeMetadata && e.Metadata is { Count: > 0 })
            {
                var ordered = OrderMeta(e.Metadata, _opt.MetadataPriorityOrder, _opt.SortRemainingMetaAlphabetically)
                    .Take(_opt.MetadataMaxCount);

                var list = string.Join(", ", ordered.Select(kv => $"{kv.Key}={kv.Value}"));
                sb.Append("  | ").Append(list);

                var trimmed = System.Math.Max(0, e.Metadata.Count - _opt.MetadataMaxCount);
                if (trimmed > 0) sb.Append($" (+{trimmed} more)");
            }

            if (_opt.IncludeCorrId && !string.IsNullOrEmpty(e.CorrelationId))
                sb.Append("  (corr=").Append(e.CorrelationId).Append(')');

            if (e.Exception != null && _opt.MultilineException)
            {
                sb.AppendLine();
                sb.Append(e.Exception); // полный ToString со стэком
            }

            return sb.ToString();
        }

        private static IEnumerable<KeyValuePair<string, object>> OrderMeta(
            IReadOnlyDictionary<string, object> meta, string[] priority, bool sortRest)
        {
            var set = new HashSet<string>(priority ?? Array.Empty<string>());
            // 1) приоритетные в заданном порядке
            foreach (var key in priority ?? Array.Empty<string>())
                if (meta.TryGetValue(key, out var val))
                    yield return new(key, val);

            // 2) остальные по алфавиту (или без сортировки)
            var rest = meta.Where(kv => !set.Contains(kv.Key));
            if (sortRest) rest = rest.OrderBy(kv => kv.Key, StringComparer.Ordinal);
            foreach (var kv in rest) yield return kv;
        }
    }
}