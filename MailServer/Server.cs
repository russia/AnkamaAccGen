using AnkamaAccGen.Managers;
using MimeKit;
using Rnwood.SmtpServer;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AnkamaAccGen.MailServer
{
    public class Server
    {
        private static DefaultServer m_server;

        public static event Action<string, MimeMessage> EmailReceived;

        public static void Initialize()
        {
            m_server = new DefaultServer(true, 25);
            m_server.MessageReceivedEventHandler += OnMessageReceived;
            m_server.SessionStartedHandler += M_server_SessionStartedHandler;
            m_server.AuthenticationCredentialsValidationRequiredEventHandler += OnAuthenticationCredentialsValidationRequired;
            EmailReceived += Accounts.OnEmailReceived;
            m_server.Start();

            Console.WriteLine($"[STMPMANAGER] SMTP Server is listening on port {m_server.PortNumber}.");
        }

        private static Task M_server_SessionStartedHandler(object sender, SessionEventArgs e)
        {
            Console.WriteLine($"[STMPMANAGER] Session started {e.Session.ClientAddress}");
            return Task.CompletedTask;
        }

        private static Task OnAuthenticationCredentialsValidationRequired(object sender, AuthenticationCredentialsValidationEventArgs e)
        {
            e.AuthenticationResult = AuthenticationResult.Success;
            return Task.CompletedTask;
        }

        private static async Task OnMessageReceived(object sender, MessageEventArgs e)
        {
            using Stream stream = e.Message.GetData().Result;
            string to = string.Join(", ", e.Message.Recipients);
            Console.WriteLine($"[STMPMANAGER] Message received. Client address {e.Message.Session.ClientAddress}. From {e.Message.From}. To {to}.");
            var message = await ConvertAsync(stream, e.Message.From, to);
            if (message != null)
                EmailReceived?.Invoke(to, message);
        }

        private static async Task<MimeMessage> ConvertAsync(Stream messageData, string envelopeFrom, string envelopeTo)
        {
            byte[] data = new byte[messageData.Length];
            await messageData.ReadAsync(data, 0, data.Length);

            bool foundHeaders = false;
            bool foundSeparator = false;
            using (StreamReader dataReader = new StreamReader(new MemoryStream(data)))
            {
                while (!dataReader.EndOfStream)
                {
                    if (dataReader.ReadLine().Length != 0)
                    {
                        foundHeaders = true;
                    }
                    else
                    {
                        foundSeparator = true;
                        break;
                    }
                }
            }

            if (!foundHeaders || !foundSeparator)
            {
                Console.WriteLine("[STMPMANAGER] Malformed MIME message. No headers found");
            }
            else
            {
                messageData.Seek(0, SeekOrigin.Begin);
                try
                {
                    CancellationTokenSource cts = new CancellationTokenSource();
                    cts.CancelAfter(TimeSpan.FromSeconds(10));
                    MimeMessage mime = await MimeMessage.LoadAsync(messageData, true, cts.Token).ConfigureAwait(false);
                    var body = mime.Body as MultipartAlternative;
                    return mime;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"[STMPMANAGER] Error while parsing mime message : {e.Message}");
                }
            }

            return null;
        }
    }
}