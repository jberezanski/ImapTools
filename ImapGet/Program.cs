using S22.Imap;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImapTools.ImapGet
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var cmdArgs = new Args();
                CommandLine.Parser.Default.ParseArgumentsStrict(args, cmdArgs);
                string password = cmdArgs.Password;
                if (password == "*")
                {
                    Console.Write("Password: ");
                    password = Console.ReadLine();
                    if (!Console.IsOutputRedirected)
                    {
                        Console.CursorTop = Console.CursorTop - 1;
                        Console.WriteLine("Password: " + string.Concat(Enumerable.Repeat("*", Math.Max(20, password.Length))));
                    }
                }
                Console.WriteLine("Connecting to server");
                using (ImapClient cli = new ImapClient(cmdArgs.Host, cmdArgs.Port == 0 ? (cmdArgs.Ssl ? 993 : 143) : cmdArgs.Port, cmdArgs.Ssl))
                {
                    Console.WriteLine("Logging in");
                    cli.Login(cmdArgs.UserName, password, AuthMethod.Auto);
                    Console.WriteLine();

                    Dump(() => cli.ListMailboxes(), "Mailboxes");

                    //Dump(() => cli.Capabilities(), "Capabilities");

                    //Dump(cli.GetMailboxInfo("INBOX"));
                    //Dump(cli.GetMailboxInfo("Elementy wysłane"));

                    string mb = cmdArgs.DraftsFolder;
                    //Dump(cli.GetMailboxInfo(mb));

                    var ids = cli.Search(SearchCondition.All(), mb);
                    Console.WriteLine("{0} message(s)");
                    foreach (var id in ids)
                    {
                        Console.WriteLine(id);
                        var fs = cli.GetMessageFlags(id, mb);
                        Console.WriteLine("\t{0}: {1}", "Flags", string.Join(" ", fs.Select(f => f.ToString())));
                        var m = cli.GetMessage(id, FetchOptions.Normal, false, mb);
                        Console.WriteLine("\tHeaders:");
                        foreach (string h in m.Headers)
                        {
                            Console.WriteLine("\t\t{0}: {1}", h, m.Headers[h]);
                        }
                        if (m.Subject.StartsWith("[PATCH"))
                        {
                            var fileName = string.Format("{0}.patch", id);
                            Console.WriteLine("Saving message to {0}", fileName);
                            Func<string, string> adjustHeaderValue = k =>
                                {
                                    var v = m.Headers[k];
                                    if (k == "Content-Type")
                                    {
                                        if (v != null)
                                        {
                                            var i = v.IndexOf("charset=");
                                            if (0 <= i)
                                            {
                                                return v.Substring(0, i) + "charset=\"UTF-8\"";
                                            }
                                        }
                                    }
                                    else if (k == "Content-Transfer-Encoding")
                                    {
                                        return "8bit";
                                    }
                                    return v;
                                };
                            File.WriteAllBytes(
                                fileName,
                                Encoding.UTF8.GetBytes(
                                    string.Join(
                                        Environment.NewLine,
                                        m.Headers.Cast<string>().Select(k => k + ": " + adjustHeaderValue(k))
                                        .Concat(
                                            new[]
                                    {
                                        string.Empty,
                                        m.Body
                                    }))));
                        }
                        Console.WriteLine();
                    }

                    Console.WriteLine("Logging out");
                    cli.Logout();
                }
            }
            catch (Exception x)
            {
                Console.WriteLine(x);
            }
        }

        private static void Dump(MailboxInfo mbInfo)
        {
            Console.WriteLine("Mailbox: {0}", mbInfo.Name);
            Console.WriteLine("\t{0}: {1}", "Messages", mbInfo.Messages);
            Console.WriteLine("\t{0}: {1}", "NextUID", mbInfo.NextUID);
            Console.WriteLine("\t{0}: {1}", "Unread", mbInfo.Unread);
            Console.WriteLine("\t{0}: {1}", "UsedStorage", mbInfo.UsedStorage);
            Console.WriteLine("\t{0}: {1}", "FreeStorage", mbInfo.FreeStorage);
            Console.WriteLine("\t{0}: {1}", "Flags", string.Join(" ", mbInfo.Flags.Select(f => f.ToString())));
            Console.WriteLine();
        }

        private static void Dump(Func<IEnumerable<string>> items, string name)
        {
            Console.WriteLine("{0}:", name);
            foreach (var mb in items())
            {
                Console.WriteLine("\t{0}", mb);
            }
            Console.WriteLine();
        }
    }
}
