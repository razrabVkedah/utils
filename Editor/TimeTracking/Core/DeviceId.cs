using System;

namespace Rusleo.Utils.Editor.TimeTracking.Core
{
    public readonly struct DeviceId
    {
        public DeviceId(string value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public string Value { get; }
        public override string ToString() => Value;
    }
}