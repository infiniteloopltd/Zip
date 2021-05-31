using System;
using System.IO;
using System.Text;

namespace PartialZip
{
    class Program
    {
        static void Main(string[] args)
        {
            var file = "hello3.zip";
            var bFile = File.ReadAllBytes(file);
            var nextOffset = 0;
            do
            {
                var entry = new ZipEntry(bFile, nextOffset);
                if (!entry.IsValid) break;
                var content = Encoding.UTF8.GetString(entry.Decompressed);
                Console.WriteLine(entry.FileName);
                Console.WriteLine(content);
                nextOffset = entry.NextOffset;
            } while (true);
        }

        

    }
}
