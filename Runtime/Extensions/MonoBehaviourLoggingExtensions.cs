using UnityEngine;

namespace Rusleo.Utils.Runtime.Logging
{
    public static class MonoBehaviourLoggingExtensions
    {
        /// Быстрый способ получить контекстный логгер для компонента.
        public static Logger GetLogger(this MonoBehaviour mb, string owner = null)
        {
            owner ??= mb.GetType().Name;
            return new Logger(owner, mb);
        }
    }
}