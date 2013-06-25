using S22.Imap;
using System;
using System.Linq;

namespace ImapTools.ImapMarkDrafts
{
    class Program
    {
        static int Main(string[] args)
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
                if (cmdArgs.Verbose)
                {
                    Console.WriteLine("Connecting to server");
                }
                using (ImapClient cli = new ImapClient(cmdArgs.Host, cmdArgs.Port == 0 ? (cmdArgs.Ssl ? 993 : 143) : cmdArgs.Port, cmdArgs.Ssl))
                {
                    if (cmdArgs.Verbose)
                    {
                        Console.WriteLine("Logging in");
                    }
                    cli.Login(cmdArgs.UserName, password, AuthMethod.Auto);
                    if (cmdArgs.Verbose)
                    {
                        Console.WriteLine();
                    }

                    string mb = cmdArgs.DraftsFolder;

                    var ids = cli.Search(SearchCondition.All(), mb);
                    foreach (var id in ids)
                    {
                        var fs = cli.GetMessageFlags(id, mb);
                        if (fs.Contains(MessageFlag.Draft))
                        {
                            continue;
                        }

                        if (cmdArgs.Verbose)
                        {
                            Console.WriteLine(id);
                            Console.WriteLine("\t{0}: {1}", "Flags", string.Join(" ", fs.Select(f => f.ToString())));
                            var m = cli.GetMessage(id, FetchOptions.HeadersOnly, false, mb);
                            Console.WriteLine("\tHeaders:");
                            foreach (string h in m.Headers)
                            {
                                Console.WriteLine("\t\t{0}: {1}", h, m.Headers[h]);
                            }
                        }

                        Console.WriteLine("Setting Draft flag on message {0}", id);
                        cli.SetMessageFlags(id, mb, MessageFlag.Draft, MessageFlag.Seen);
                        var fs2 = cli.GetMessageFlags(id, mb);
                        if (cmdArgs.Verbose)
                        {
                            Console.WriteLine("Message flags: {0}", string.Join(" ", fs2.Select(f => f.ToString())));
                            Console.WriteLine();
                        }
                    }

                    if (cmdArgs.Verbose)
                    {
                        Console.WriteLine("Logging out");
                    }
                    cli.Logout();
                }

                return 0;
            }
            catch (Exception x)
            {
                Console.WriteLine("{0}: {1}", x.GetType().FullName, x.Message);
                return 1;
            }
        }
    }
}
