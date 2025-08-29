using UnityEngine;

namespace USave.Internal
{
    public class Logger : ILogger
    {
        [HideInCallstack]
        public void Log(object message) => Debug.Log($"[USAVE] {message}");

        [HideInCallstack]
        public void LogError(object message) => Debug.LogError($"[USAVE] {message}");

        [HideInCallstack]
        public void LogWarning(object message) => Debug.LogWarning($"[USAVE] {message}");
    }
}