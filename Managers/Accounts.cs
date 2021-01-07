using AccountCreatorV2.Helper;
using AnkamaAccGen.Helper;
using AnkamaAccGen.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AnkamaAccGen.Managers
{
    public static class Accounts
    {
        public static List<Task> AccountTasks = new List<Task>();
        public static List<Account> CreatedAccounts = new List<Account>();

        // public static List<Account> UnverifiedAccounts = new List<Account>();
        private static object m_lock = new object();

        public static void Start()
        {
            Thread CapchaThread = new Thread(() => Captchas.Run());
            CapchaThread.Start();
            Run();
        }

        public static void Run()
        {
            while (true)
            {
                Thread.Sleep(50);
                if (!Captchas.Tokens.ToArray().Any(x => x.GotResponse))
                    continue;

                try
                {
                    CaptchaResponse Response = Captchas.Tokens.First(x => x.GotResponse);
                    Captchas.Tokens.Remove(Response);
                    var task = Task.Run(() => { Process p = new Process(Response.Token, AccountTasks.Count()); });
                    AccountTasks.Add(task);
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Caught exception : {ex}");
                }
            }
        }

        public static async void OnEmailReceived(string email, MimeKit.MimeMessage content)
        {
            Console.WriteLine(content.Subject);
            if (content.Subject.Contains("Validation"))
            {
                var account = CreatedAccounts.FirstOrDefault(x => x.email == email && !x.isEmailValidated);
                if (account != null)
                {
                    string verificationLink = StringHelper.ParseVerificationLinkFromBody(content.TextBody);
                    Console.WriteLine(verificationLink);
                    if(await EmailVerification(account, "[EMAIL MANAGER] ", verificationLink))
                        SaveAccount(account);
                    account.isEmailValidated = true;
                    DebugHelper.Out("[EMAIL MANAGER] " + $"Email confirmed", DebugHelper.Type.Info);
                }
            }
        }

        public static async Task<bool> EmailVerification(Account acc, string task, string link)
        {
            try
            {
                var result = await HttpHelper.EmailValidation(acc, link);
                return result.Contains("terminée à 100%");
            }
            catch (Exception e)
            {
                DebugHelper.Out(task + $"Failed. Reason : {e.Message}", DebugHelper.Type.Error);
                return false;
            }
        }

        public static void AddAccount(Account acc)
        {
            lock (m_lock)
                CreatedAccounts.Add(acc);
        }

        public static void SaveAccount(Account acc)
        {
            lock (m_lock)
            {
                //UnverifiedAccounts.Remove(acc);
                string path = Constants.OutputPath;
                if (!File.Exists(path))
                    using (StreamWriter sw = File.CreateText(path))
                        sw.WriteLine($"{acc.username}:{acc.password}");
                else
                    using (StreamWriter sw = File.AppendText(path))
                        sw.WriteLine($"{acc.username}:{acc.password}");

                DebugHelper.Out($"Account {acc.username}:{acc.password} saved succesfully in " + path + "!", DebugHelper.Type.Success);
            }
        }
    }

    public class Account
    {
        public string username { get; set; }
        public string password { get; set; }
        public string email { get; set; }
        public bool isEmailValidated { get; set; } = false;

        public Account(string _username, string _password, string _email)
        {
            username = _username;
            password = _password;
            email = _email;
            Accounts.AddAccount(this);
        }
    }
}