using System;
using Rusleo.Utils.Editor.TimeTracking.Core;
using Rusleo.Utils.Editor.TimeTracking.Interfaces;

namespace Rusleo.Utils.Editor.TimeTracking.Services.Ids
{
    public sealed class GuidSessionIdProvider : ISessionIdProvider
    {
        public SessionId Create()
        {
            var id = Guid.NewGuid().ToString("N");
            return new SessionId(id);
        }
    }
}