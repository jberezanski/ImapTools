using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImapTools
{
    class Args
    {
        [Option('h', "host", Required = true)]
        public string Host { get; set; }

        [Option('t', "port", DefaultValue = 0)]
        public int Port { get; set; }

        [Option('s', "ssl", DefaultValue = false)]
        public bool Ssl { get; set; }

        [Option('p', "password", Required = true)]
        public string Password { get; set; }

        [Option('u', "username", Required = true)]
        public string UserName { get; set; }

        [Option('d', "drafts-folder", DefaultValue = "Kopie robocze")]
        public string DraftsFolder { get; set; }

        [Option('v', "verbose", DefaultValue = false)]
        public bool Verbose { get; set; }
    }
}
