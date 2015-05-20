using Symblr.Symbols.Pdb20;
using System;
using System.IO;
using System.Text;

namespace Symblr
{
    class Program
    {
        static void Main(string[] args)
        {

            using (var fs = File.Open(@"E:\Watermelons\NoSrcSrv.pdb", FileMode.Open))
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
                }

                rf.SaveAsync().Wait();
            }
        }
    }
}

