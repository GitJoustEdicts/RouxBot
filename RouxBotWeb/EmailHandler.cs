using MailKit.Net.Imap;
using MailKit.Search;
using MailKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace RouxBotWeb
{
    public class MailRepository
    {
        private readonly string mailServer, login, password;
        private readonly int port;
        private readonly bool ssl;

        public MailRepository(string mailServer, int port, bool ssl, string login, string password)
        {
            this.mailServer = mailServer;
            this.port = port;
            this.ssl = ssl;
            this.login = login;
            this.password = password;
        }

        public IEnumerable<ValidEmailFormat> GetUnreadMails()
        {
            var messages = new List<ValidEmailFormat>();

            using (var client = new ImapClient())
            {
                client.Connect(mailServer, port, ssl);

                // Note: since we don't have an OAuth2 token, disable
                // the XOAUTH2 authentication mechanism.
                client.AuthenticationMechanisms.Remove("XOAUTH2");

                client.Authenticate(login, password);

                // The Inbox folder is always available on all IMAP servers...
                var inbox = client.Inbox;
                inbox.Open(FolderAccess.ReadWrite);
                var results = inbox.Search(SearchOptions.All, SearchQuery.Not(SearchQuery.Seen));
                foreach (var uniqueId in results.UniqueIds)
                {
                    var message = inbox.GetMessage(uniqueId);
                    string body = message.GetTextBody(MimeKit.Text.TextFormat.Text);
                    string[] arr = body.Split("\r\n");
                    try
                    {
                        ValidEmailFormat? temp = new();
                        temp.notification = arr[0];
                        temp.emoji = arr[1];
                        temp.readableContent = arr[2];
                        temp.voice = arr[3];

                        if (temp != null)
                        {
                            // Emojify
                            int value = Convert.ToInt32(temp.emoji, 16);
                            temp.emoji = Char.ConvertFromUtf32(value);

                            messages.Add(temp);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Caught exception {0}", ex.Message);
                    }

                    // Delete email (one time use)
                    inbox.AddFlags(uniqueId, MessageFlags.Deleted, true);
                }

                client.Disconnect(true);
            }

            return messages;
        }

        public IEnumerable<string> GetAllMails()
        {
            var messages = new List<string>();

            using (var client = new ImapClient())
            {
                client.Connect(mailServer, port, ssl);

                // Note: since we don't have an OAuth2 token, disable
                // the XOAUTH2 authentication mechanism.
                client.AuthenticationMechanisms.Remove("XOAUTH2");

                client.Authenticate(login, password);

                // The Inbox folder is always available on all IMAP servers...
                var inbox = client.Inbox;
                inbox.Open(FolderAccess.ReadOnly);
                var results = inbox.Search(SearchOptions.All, SearchQuery.NotSeen);
                foreach (var uniqueId in results.UniqueIds)
                {
                    var message = inbox.GetMessage(uniqueId);

                    messages.Add(message.HtmlBody);

                    //Mark message as read
                    //inbox.AddFlags(uniqueId, MessageFlags.Seen, true);
                }

                client.Disconnect(true);
            }

            return messages;
        }
    }
}