using System.Globalization;
using UnityEngine;

namespace Rusleo.Utils.Editor.Windows.PlayerPrefsVisualizer.Internal
{
    internal enum PrefType
    {
        Int,
        Float,
        String
    }

    internal sealed class PrefRecord
    {
        public string key;
        public PrefType type;
        public string raw; // value as text for JSON/GUI round‑trip

        public PrefRecord()
        {
        }

        public PrefRecord(string key, PrefType type, string raw)
        {
            this.key = key;
            this.type = type;
            this.raw = raw;
        }

        public static PrefRecord FromPlayerPrefs(string key, PrefType type)
        {
            switch (type)
            {
                case PrefType.Int: return new PrefRecord(key, type, PlayerPrefs.GetInt(key, 0).ToString());
                case PrefType.Float:
                    return new PrefRecord(key, type,
                        PlayerPrefs.GetFloat(key, 0f).ToString(CultureInfo.InvariantCulture));
                default: return new PrefRecord(key, type, PlayerPrefs.GetString(key, string.Empty));
            }
        }

        public void WriteToPlayerPrefs()
        {
            switch (type)
            {
                case PrefType.Int:
                    if (!int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var iv))
                        throw new System.Exception($"'{raw}' is not a valid int");
                    PlayerPrefs.SetInt(key, iv);
                    break;
                case PrefType.Float:
                    if (!float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var fv))
                        throw new System.Exception($"'{raw}' is not a valid float");
                    PlayerPrefs.SetFloat(key, fv);
                    break;
                case PrefType.String:
                    PlayerPrefs.SetString(key, raw ?? string.Empty);
                    break;
            }
        }
    }
}