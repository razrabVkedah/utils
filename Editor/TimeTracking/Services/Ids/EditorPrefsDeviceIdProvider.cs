using System;
using Rusleo.Utils.Editor.TimeTracking.Core;
using Rusleo.Utils.Editor.TimeTracking.Interfaces;
using UnityEditor;

namespace Rusleo.Utils.Editor.TimeTracking.Services.Ids
{
    public sealed class EditorPrefsDeviceIdProvider : IDeviceIdProvider
    {
        private const string Key = "Rusleo.TimeTracking.DeviceId";

        public DeviceId GetOrCreate()
        {
            var value = EditorPrefs.GetString(Key, string.Empty);

            if (!string.IsNullOrWhiteSpace(value))
                return new DeviceId(value);

            value = Guid.NewGuid().ToString("N");
            EditorPrefs.SetString(Key, value);

            return new DeviceId(value);
        }
    }
}