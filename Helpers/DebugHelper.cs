using System;

namespace AnkamaAccGen.Helpers
{
    public class DebugHelper
    {
        public enum Type
        {
            Error,
            Info,
            Success
        }

        public static void JsonFieldParseError(string task, string field, dynamic submitResult)
        {
            string error = field + " could not be parsed. Raw response: " + JsonHelper.AsString(task, submitResult);
            Out(task, error, Type.Error);
        }

        //public static void JsonFieldParseError(string field, dynamic submitResult)
        //{
        //    string error = field + " could not be parsed. Raw response: " + JsonHelper.AsString(submitResult);
        //    Out(error, Type.Error);
        //}

        public static void Out(string task, string message, Type? type = null)
        {
            Console.ForegroundColor = type switch
            {
                Type.Error => ConsoleColor.Red,
                Type.Info => ConsoleColor.Yellow,
                Type.Success => ConsoleColor.Green,
                _ => throw new NotImplementedException(),
            };
            Console.WriteLine($"{task}{message}");
            Console.ResetColor();
        }

        public static void Out(string message, Type? type = null)
        {
            Console.ForegroundColor = type switch
            {
                Type.Error => ConsoleColor.Red,
                Type.Info => ConsoleColor.Yellow,
                Type.Success => ConsoleColor.Green,
                _ => throw new NotImplementedException(),
            };
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}