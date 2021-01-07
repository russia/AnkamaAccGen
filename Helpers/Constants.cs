using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnkamaAccGen.Helpers
{
    public class Constants
    {
        public static int AccountsCount = 1000; // max acc to create
        public const string AntiCaptchaKey = "3b305a339e905b2be5644650e375dfde";
        public const string OutputPath = "accounts.txt";
        public const string EmailDomain = "dofue.com";
        public const int MaxThreads = 5;
    }
}
