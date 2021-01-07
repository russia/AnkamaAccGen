using AnkamaAccGen.MailServer;
using AnkamaAccGen.Managers;
using System;
using System.Threading;

namespace AnkamaAccGen
{
    class Program
    {
        public static bool isRunning = true;
        public static bool isDebug = false;
        static void Main()
        {
            Server.Initialize();
            Accounts.Start();
            while (isRunning)
            {
                Thread.Sleep(500);
            }
        }
    }
}
