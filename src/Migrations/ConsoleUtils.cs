using System;

namespace Migrations
{
    public static class ConsoleUtils
    {
        public static void LogException(Exception e)
        {
            Console.Error.WriteLine($"{e.GetType()}; {e.Message}");
            Console.Error.WriteLine(e.StackTrace);

            if (e.InnerException != null)
            {
                Console.Error.WriteLine("Inner exception:");
                LogException(e.InnerException);
            }
        }
    }
}
