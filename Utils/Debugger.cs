using System;
using TShockAPI;

namespace DA_Integration.Utils
{
    public static class Debugger
    {
        private const string Tag = "[DAIntegration] ";

        public static void SuccessOutput(string message)
        {
            TShock.Log.ConsoleInfo($"{Tag}{message}");
        }

        public static void MessageOutput(string message)
        {
            TShock.Log.ConsoleWarn($"{Tag}{message}");
        }

        public static void ErrorOutput(string message)
        {
            TShock.Log.ConsoleError($"{Tag}{message}");
        }

        [Obsolete("Используйте SuccessOutput")]
        public static void successOutput(string message) => SuccessOutput(message);

        [Obsolete("Используйте MessageOutput")]
        public static void messageOutput(string message) => MessageOutput(message);

        [Obsolete("Используйте ErrorOutput")]
        public static void errorOutput(string message) => ErrorOutput(message);
    }
}