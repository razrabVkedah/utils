using System;
using Rusleo.Utils.Editor.TimeTracking.Core;
using Rusleo.Utils.Editor.TimeTracking.Interfaces;

namespace Rusleo.Utils.Editor.TimeTracking.Services.Systems
{
    public sealed class ClockUtcSeconds : IClock
    {
        public UnixTime UtcNow()
        {
            var ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return new UnixTime(ts);
        }
    }
}