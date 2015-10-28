using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;

namespace ScaffoldFinderV2
{
    public class IGzip
    {
        public string Decompress(string filename)
        {
            FileStream fsIn = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);

            string DecompressedContents = "";
            using (var decompress = new GZipStream(fsIn, CompressionMode.Decompress))
            using (var sr = new StreamReader(decompress)){
                DecompressedContents = sr.ReadToEnd();
            }
            return DecompressedContents;

        }

        
    }

    

}
