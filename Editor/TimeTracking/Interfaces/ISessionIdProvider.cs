using Rusleo.Utils.Editor.TimeTracking.Core;

namespace Rusleo.Utils.Editor.TimeTracking.Interfaces
{
    public interface ISessionIdProvider
    {
        SessionId Create();
    }
}