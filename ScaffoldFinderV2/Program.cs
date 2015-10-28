using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CommandLine.Utility;

namespace ScaffoldFinderV2
{
    class Program
    {
        static void Main(string[] args)
        {
            #region Argument Parameters

            string OutputFilename;
            string PdbDirectory = @"C:\PDB\Monomer";
            string PdbMultimerDirectory = @"C:\Archives\Biounits";
            string PdbFile = @"C:\PDB\Monomer\pdb1rcv.ent.gz";
            bool useMultimeric = false;

            #endregion

            ILoopCriteria LoopCriteria = new ILoopCriteria();

            Arguments CommandLine = new Arguments(args);
            if (CommandLine["directory"] != null) {
                PdbDirectory = CommandLine["directory"];
            }

            if (CommandLine["multimeric"] != null) {
                useMultimeric = true;
            }

            if (CommandLine["searchfortrimer"] != null)
            {
                LoopCriteria.SearchForTrimer = true;
                LoopCriteria.SearchForLoopPairs = false;
            }

            if (CommandLine["file"] != null) {
                PdbFile = CommandLine["file"];
            }

            if (CommandLine["match"] != null) {
                LoopCriteria.SearchForLoopPairs = true;
                if (CommandLine["lower"] != null) {
                    LoopCriteria.LoopPairDistanceLowerLimit = Convert.ToDouble(CommandLine["lower"]);
                }
                if (CommandLine["upper"] != null) {
                    LoopCriteria.LoopPairDistanceUpperLimit = Convert.ToDouble(CommandLine["upper"]);
                }
            }
            else {
                LoopCriteria.SearchForLoopPairs = false;
            }

            if (CommandLine["out"] != null) {
                OutputFilename = CommandLine["out"];
            }
            else {
                OutputFilename = "ScaffoldFinderLog_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
            }



            string LogFilename = @"c:\Archives\Output\" + OutputFilename;
            FileStream LogStream = File.Open(LogFilename,
                                    FileMode.CreateNew,
                                    FileAccess.Write);
            StreamWriter LogFile = new StreamWriter(LogStream, Encoding.UTF8);


            //Write the lof file header
            if (LoopCriteria.SearchForLoopPairs) {
                LogFile.WriteLine("PDB,ChainA,NTermLoopA,CTermLoopA,Length,ChainB,NTermLoopB,CTermLoopB,LengthB,Size");
            }
            else if (LoopCriteria.SearchForTrimer)
            {
                LogFile.WriteLine("PDB,ChainA,NTermLoopA,CTermLoopA,Length,ChainB,NTermLoopB,CTermLoopB,LengthB,ChainC,NTermLoopC,CTermLoopC,LengthC");
            }
            else
            {
                LogFile.WriteLine("PDB,Chain,NTerm,CTerm,Length");
            }


            //Loop through the directory to get the PDBs
            DirectoryInfo Directory;
            if (useMultimeric) {
                Directory = new DirectoryInfo(PdbMultimerDirectory);

            }
            else {
                Directory = new DirectoryInfo(PdbDirectory);
            }
            
            int FileCounter = 0;
            int FileTotals = Directory.GetFiles().Count();
            foreach (FileInfo file in Directory.GetFiles())
            {
                FileCounter++;
                if (FileCounter % 500 == 0) {
                    Console.WriteLine("[" + FileCounter + "/" + FileTotals + "]: " + file.FullName);
                }
                //Console.SetCursorPosition(0, Console.CursorTop);

                //Console.WriteLine("Filename: " + file.Name + "  Size: " + file.Length);
                if (file.Length > 500000)
                {
                    continue;
                }
                IPdb Pdb = new IPdb(file.FullName, useMultimeric);
                ILoopsCollection LoopsCollection = new ILoopsCollection();
                if (Pdb.Atoms.Count() != 0) { // Implies Nucleic Acid 
                    FindLoops findLoops = new FindLoops();
                    LoopsCollection = findLoops.Search(Pdb);
                    //LoopsCollection = FindLoops.Search(Pdb);
                    findLoops = null;

                }
                else {
                    continue;
                }

                //Search for three loops that match the trimer constraints
                if(LoopCriteria.SearchForTrimer) {
                    FindLoops findLoops = new FindLoops();
                    List<ILoopTrimer> LoopTrimer = findLoops.FindTrimerLoops(LoopsCollection, LoopCriteria);

                    foreach (ILoopTrimer Loops in LoopTrimer)
                    {
                        LogFile.WriteLine(Loops.LoopA.PdbCode + "," +
                                          Loops.LoopA.Chain + "," +
                                          Loops.LoopA.NTermAtom.ResidueNumber + Loops.LoopA.NTermAtom.ResidueExtension + "," +
                                          Loops.LoopA.CTermAtom.ResidueNumber + Loops.LoopA.CTermAtom.ResidueExtension + "," +
                                          Loops.LoopA.LoopLength + "," +
                                          Loops.LoopB.Chain + "," +
                                          Loops.LoopB.NTermAtom.ResidueNumber + Loops.LoopB.NTermAtom.ResidueExtension + "," +
                                          Loops.LoopB.CTermAtom.ResidueNumber + Loops.LoopB.CTermAtom.ResidueExtension + "," +
                                          Loops.LoopB.LoopLength + "," +
                                          Loops.LoopC.Chain + "," +
                                          Loops.LoopC.NTermAtom.ResidueNumber + Loops.LoopC.NTermAtom.ResidueExtension + "," +
                                          Loops.LoopC.CTermAtom.ResidueNumber + Loops.LoopC.CTermAtom.ResidueExtension + "," +
                                          Loops.LoopC.LoopLength);
                    }

                    LoopTrimer = null;
                    findLoops = null;
                }

                else if (LoopCriteria.SearchForLoopPairs) {
                    FindLoops findLoops = new FindLoops();
                    List<ILoopPair> LoopPairs = findLoops.FindMatchedLoops(LoopsCollection, LoopCriteria);
                    foreach (ILoopPair LoopPair in LoopPairs)
                    {
                        Console.WriteLine("Found: " + LoopPair.LoopA.PdbCode);
                        LogFile.WriteLine(LoopPair.LoopA.PdbCode + "," +
                                          LoopPair.LoopA.Chain + "," +
                                          LoopPair.LoopA.NTermAtom.ResidueNumber + LoopPair.LoopA.NTermAtom.ResidueExtension + "," +
                                          LoopPair.LoopA.CTermAtom.ResidueNumber + LoopPair.LoopA.CTermAtom.ResidueExtension + "," +
                                          LoopPair.LoopA.LoopLength + "," +
                                          LoopPair.LoopB.Chain + "," +
                                          LoopPair.LoopB.NTermAtom.ResidueNumber + LoopPair.LoopB.NTermAtom.ResidueExtension + "," +
                                          LoopPair.LoopB.CTermAtom.ResidueNumber + LoopPair.LoopB.CTermAtom.ResidueExtension + "," +
                                          LoopPair.LoopB.LoopLength + "," +
                                          Pdb.Atoms.Count()) ;
                    }
                    LoopPairs = null;
                    findLoops = null;
               
                }

                else
                {

                    foreach (ILoop Loop in LoopsCollection.Loops)
                    {

                        LogFile.WriteLine(Loop.PdbCode + "," +
                                          Loop.Chain + "," +
                                          Loop.NTermAtom.ResidueNumber + Loop.NTermAtom.ResidueExtension + "," +
                                          Loop.CTermAtom.ResidueNumber + Loop.CTermAtom.ResidueExtension + "," +
                                          Loop.LoopLength);

                    }
                    //Cleanup
                    
                }
            }

            LogFile.Close();
            LogStream.Close();


           // Console.ReadLine();

        }


    }
}
