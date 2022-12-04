using System;

namespace Botcchi
{
    class Program
    {
        private static void Main(string[] args)
        {
            Bot bot = new Bot();
            bot.Run().GetAwaiter().GetResult();
        }
    }
}