using System.Linq;
using UnityEngine;

namespace Rusleo.Utils.Editor.Windows.PlayerPrefsVisualizer.Internal
{
    internal sealed class PrefIndex
    {
        public System.Collections.Generic.List<PrefRecord> items = new();
        private const string IndexKey = "__ppv_index"; // stored as JSON

        [System.Serializable]
        private class SerializableList
        {
            public System.Collections.Generic.List<PrefRecord> items;
        }

        public static PrefIndex Load()
        {
            var json = PlayerPrefs.GetString(IndexKey, string.Empty);
            if (string.IsNullOrEmpty(json)) return new PrefIndex();
            try
            {
                var list = JsonUtility.FromJson<SerializableList>(json);
                return new PrefIndex { items = list?.items ?? new System.Collections.Generic.List<PrefRecord>() };
            }
            catch
            {
                return new PrefIndex();
            }
        }

        public void Save()
        {
            var wrapper = new SerializableList { items = items };
            var json = JsonUtility.ToJson(wrapper, false);
            PlayerPrefs.SetString(IndexKey, json);
            PlayerPrefs.Save();
        }

        public PrefRecord Find(string key) => items.FirstOrDefault(i => i.key == key);
        public bool Contains(string key) => items.Any(i => i.key == key);

        public void AddOrUpdate(PrefRecord rec)
        {
            var existing = Find(rec.key);
            if (existing == null) items.Add(rec);
            else
            {
                existing.type = rec.type;
                existing.raw = rec.raw;
            }
        }

        public bool Remove(string key)
        {
            var i = items.FindIndex(x => x.key == key);
            if (i >= 0)
            {
                items.RemoveAt(i);
                return true;
            }

            return false;
        }

        public void SortByKeyAsc() => items = items.OrderBy(i => i.key, System.StringComparer.Ordinal).ToList();
    }
}