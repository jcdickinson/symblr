using Symblr.Symbols.Pdb20;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Symblr
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var fs = File.Open(@"C:\dbg\symbols\MicrosoftPublicSymbols\Microsoft.CSharp.pdb\A909949FDA7E40CD9421932B02C411711\Symblr.Server.pdb", FileMode.Open))
            using (var buff = new BufferedStream(fs, Environment.SystemPageSize))
            using (var rf = Pdb20File.TryOpenAsync(fs).Result)
            {
                using (var srcsrv = rf.GetStream("SRCSRV"))
                //using (var r = new StreamReader(srcsrv))
                //{
                //    Console.WriteLine(r.ReadToEnd());
                //}
                using (var tr = new StreamWriter(srcsrv))
                {
                    srcsrv.SetLength(0);
                    for (var i = 0; i < 20; i++)
                        tr.WriteLine("test {0}", i);
                }

                rf.SaveAsync().Wait();
            }
        }
    }
}

