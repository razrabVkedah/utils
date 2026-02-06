using Rusleo.Utils.Editor.TimeTracking.Interfaces;
using UnityEngine;

namespace Rusleo.Utils.Editor.TimeTracking.Services.Systems
{
    public sealed class UnityContextProvider : IUnityContextProvider
    {
        public string GetUnityVersion()
        {
            return Application.unityVersion;
        }
    }
}
