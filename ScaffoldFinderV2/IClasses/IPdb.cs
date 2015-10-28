using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;


namespace ScaffoldFinderV2
{
    public class IPdb
    {

        public string PdbCode;
        public List<IResidue> Atoms = new List<IResidue>();
        long filesize;

        public IPdb(string filename, bool multimeric) {

            int lineCount = 0;

            //Check to see if the file needs decomrpession (this is the case for full PDB scans)
            if (filename.Substring(filename.Length - 3, 3) == ".gz") {
                IGzip gzip = new IGzip();
                string PdbDecompressed = gzip.Decompress(filename);
                gzip = null;

                //Console.WriteLine("Filename: " + filename);
                string[] lines = PdbDecompressed.Split('\n');
                
                foreach (string line in lines) {
                    ParseLine(line);
                    lineCount++;
                }
            }

            //Standard PDB (already unzipped)
            else {
                StreamReader PdbFile = File.OpenText(filename);
                string line = PdbFile.ReadLine();

                while (line != null) {       
                    ParseLine(line);
                    line = PdbFile.ReadLine();
                    lineCount++;
                }
                PdbFile.Close();
            }

            this.filesize = lineCount;
            this.PdbCode = ParsePdbCode(filename, multimeric);
            
        }

        public IPdb(string filename, bool multimeric, bool Decompress)
        {

   
            StreamReader PdbFile = File.OpenText(filename);

            string line = PdbFile.ReadLine();
            while (line != null)
            {
                ParseLine(line);
                line = PdbFile.ReadLine();
            }
            PdbFile.Close();

            this.PdbCode = ParsePdbCode(filename, multimeric);
        }

        private string ParsePdbCode(string Filename, bool multimeric)
        {
            string Code = @"";
            if (multimeric) {
                string DirectoryBase = @"C:\PDB\ICM-Multimer\";
                string Substring = Filename.Substring(0, Filename.IndexOf("."));
                Code = Substring.Replace(DirectoryBase, "");
                Code = Code.Replace("B1", "");
            }
            else {
                string DirectoryBase = @"C:\PDB\Monomer\pdb";
                string Extension = @".ent.gz";
                Code = Filename.Replace(DirectoryBase, "");
                Code = Code.Replace(Extension, "");
            }
            
            return Code;

        }

        private void ParseLine(string line) {
            if (line.Length < 4) { return; }
            if (line.Substring(0,4)=="ATOM" && line.IndexOf(" CA ")!=-1)
            {
                string Record = line.Substring(0, 6-0).Trim();
                string Id = line.Substring(6, 11 - 6).Trim();
                string name = line.Substring(12, 16 - 12).Trim();
                string residue = line.Substring(17, 20 - 17).Trim();
                string chain = line.Substring(21, 23 - 21).Trim();
                string residuenumber = line.Substring(22, 26 - 22).Trim();
                string residueextension = line.Substring(26, 28 - 26).Trim();
                string xcoord = line.Substring(30, 38 - 30).Trim();
                string ycoord = line.Substring(38, 46 - 38).Trim();
                string zcoord = line.Substring(46, 54 - 46).Trim();

                string[] PdbComponents = Regex.Split(line, @"[ \t]+");
                IResidue Residue = new IResidue(Convert.ToInt32(Id), 
                    name, 
                    residue, 
                    chain, 
                    residuenumber + residueextension, 
                    Convert.ToDouble(xcoord), 
                    Convert.ToDouble(ycoord), 
                    Convert.ToDouble(zcoord));

                //Add the atom to the list
                Atoms.Add(Residue);
            }

        }
    }
}
