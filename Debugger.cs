using System;

namespace DonationIntegration
{
    class Debugger
    {
        public static void successOutput(string message)
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(Config.plugin_tag + message);
            makeColorsBack();
        }

        public static void messageOutput(string message)
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(Config.plugin_tag + message);
            makeColorsBack();
        }

        public static void errorOutput(string message)
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(Config.plugin_tag + message);
            makeColorsBack();
        }

        public static void makeColorsBack()
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
