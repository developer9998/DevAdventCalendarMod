using System;
using WebSocketSharp;

namespace DevAdventCalendarMod.Scripts
{
    public class Logger
    {
        public enum LogType
        {
            Default = 0,
            Warning = 1,
            Error = 2
        }

        public static void LogMessage(string message, LogType type)
        {
            switch (type)
            {
                case LogType.Warning:
                    UnityEngine.Debug.LogWarning(string.Format("[{0}, {1}] {2}", PluginInfo.Name, DateTime.Now, message));
                    break;
                case LogType.Error:
                    UnityEngine.Debug.LogError(string.Format("[{0}, {1}] {2}", PluginInfo.Name, DateTime.Now, message));
                    break;
                default:
                    UnityEngine.Debug.Log(string.Format("[{0}, {1}] {2}", PluginInfo.Name, DateTime.Now, message));
                    break;
            }
        }
    }
}
