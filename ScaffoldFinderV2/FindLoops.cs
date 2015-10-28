using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace ScaffoldFinderV2
{
    public class FindLoops
    {

        public ILoopsCollection Search(IPdb Pdb)
        {

            ILoopsCollection LoopsCollection = new ILoopsCollection();
            ILoopCriteria LoopCriteria = new ILoopCriteria(true);

            //Try some LINQ on this beast
            var Chains = (from p in Pdb.Atoms select p.Chain).Distinct();

            foreach (string ChainName in Chains)
            {
                //Get all the atoms in the chain
                var Chain = (from p in Pdb.Atoms where p.Chain == ChainName select p);
                IResidue[] AtomsInChain = Chain.ToArray();

                //Set the chain length if it is not explicitly set (full length here)
                if(LoopCriteria.Start == -1) { LoopCriteria.Start = 0; }
                if(LoopCriteria.End == -1) { LoopCriteria.End = AtomsInChain.Count(); }

                //Search for the loops
                if (LoopCriteria.Length > AtomsInChain.Count()) { continue; }
                for (int i = LoopCriteria.Start; i < LoopCriteria.End - LoopCriteria.Length-1; i++)
                {
                    //Check for the terminal distances
                    double eDistance = EuclideanDistance(AtomsInChain[i], AtomsInChain[i + LoopCriteria.Length]);
                    if (eDistance < LoopCriteria.TerminiDistanceThreshold) {
                        //Passes the Distance Test
                        
                        //Check for the solvent accessible regions
                        if (LoopCriteria.SolventAccessible) {
                            List<IResidue> LoopResidues = new List<IResidue>();
                            for (int j = i; j < i + LoopCriteria.Length; j++) { LoopResidues.Add(AtomsInChain[j]); }
                            IResidue ApexResidue = IsLoopVectorSolventAccessible(Pdb.Atoms, LoopResidues, 4, LoopCriteria);
                            //IResidue ApexResidue = IsLoopSolventAccessible(Pdb.Atoms, LoopResidues, LoopCriteria);
                            if (ApexResidue!=null) {
                                //Passes the solvent test, add it to the loop collection
                                ILoop Loop = new ILoop();
                                Loop.NTermAtom = AtomsInChain[i];
                                Loop.NTermAtomP1 = AtomsInChain[i + 1];
                                Loop.NTermAtomP2 = AtomsInChain[i + 2];
                                Loop.CTermAtom = AtomsInChain[i + LoopCriteria.Length];
                                Loop.CTermAtomP1 = AtomsInChain[i + LoopCriteria.Length - 1];
                                Loop.CTermAtomP2 = AtomsInChain[i + LoopCriteria.Length - 2];
                                Loop.LoopLength = LoopCriteria.Length;
                                Loop.PdbCode = Pdb.PdbCode;
                                Loop.Chain = ChainName;
                                Loop.ApexAtom = ApexResidue;


                                Loop.UnitVector = new Vector3D(((Loop.NTermAtomP2.X + Loop.CTermAtomP2.X) / 2) - ((Loop.NTermAtom.X + Loop.CTermAtom.X) / 2),
                                    ((Loop.NTermAtomP2.Y + Loop.CTermAtomP2.Y) / 2) - ((Loop.NTermAtom.Y + Loop.CTermAtom.Y) / 2),
                                    ((Loop.NTermAtomP2.X + Loop.CTermAtomP2.X) / 2) - ((Loop.NTermAtom.X + Loop.CTermAtom.X) / 2));

                                //Ensure Beta-Loop-Beta
                                if (EuclideanDistance(Loop.NTermAtom, Loop.CTermAtom) < LoopCriteria.TerminiDistanceThreshold &&
                                    EuclideanDistance(Loop.NTermAtomP1, Loop.CTermAtomP1) < LoopCriteria.TerminiDistanceThreshold &&
                                    EuclideanDistance(Loop.NTermAtomP2, Loop.CTermAtomP2) < LoopCriteria.TerminiDistanceThreshold)
                                {
                                    LoopsCollection.Add(Loop);
                                }
                            }
                        }
                    }
                    

                }
                LoopCriteria.End = -1;
                LoopCriteria.Start = -1;

                
            }

            //Cleanup
            LoopCriteria = null;

            //Return
            return CleanupLoops(LoopsCollection);

        }

        private static ILoopsCollection CleanupLoops(ILoopsCollection LoopsCollection)
        {
            ILoopsCollection CleanedLoopsCollection = new ILoopsCollection();
            foreach (ILoop Loop in LoopsCollection.Loops) {
                var Count = (from p in CleanedLoopsCollection.Loops where p.ApexAtom == Loop.ApexAtom select p).Count();
                if (Count == 0) {
                    CleanedLoopsCollection.Add(Loop);
                }
            }
            return CleanedLoopsCollection;
        }

        private IResidue IsLoopVectorSolventAccessible(List<IResidue> PdbResidues, List<IResidue> LoopResidues, int VectorScale, ILoopCriteria LoopCriteria)
        {

            var PdbGuids = from p in PdbResidues
                           select p;

            var LoopGuids = from c in LoopResidues
                            select c;

            var PdbWithoutLoop = PdbGuids.Except(LoopGuids);


            IResidue[] LoopArray = LoopResidues.ToArray();
            IResidue NTerm = LoopArray[0];
            IResidue NTermP1 = LoopArray[1];    //Might not use this...
            IResidue NTermP2 = LoopArray[2];

            IResidue CTerm = LoopArray[LoopArray.Count()- 1]; //Zero base
            IResidue CTermP1 = LoopArray[LoopArray.Count() - 2]; //Might not use this...
            IResidue CTermP2 = LoopArray[LoopArray.Count() - 3];

            //Average the positions of the N aand C sides of the loops
            IResidue P0Term = new IResidue(0, "Virtual", "VP0", "N", 0,
                (NTerm.X + CTerm.X) / 2,
                (NTerm.Y + CTerm.Y) / 2,
                (NTerm.Z + CTerm.Z) / 2);

            //Might not use this...
            IResidue P1Term = new IResidue(0, "Virtual", "VP1", "N", 0,
                (NTermP1.X + CTermP1.X) / 2,
                (NTermP1.Y + CTermP1.Y) / 2,
                (NTermP1.Z + CTermP1.Z) / 2);

            IResidue P2Term = new IResidue(0, "Virtual", "VP2", "N", 0,
                (NTermP2.X + CTermP2.X) / 2,
                (NTermP2.Y + CTermP2.Y) / 2,
                (NTermP2.Z + CTermP2.Z) / 2);

            //Calculate the unit vector (not using P1 for now)
            double unitX = P2Term.X - P0Term.X;
            double unitY = P2Term.Y - P0Term.Y;
            double unitZ = P2Term.Z - P0Term.Z;

            //Scaled PsuedoResidue out in the solvent
            IResidue SolventResidue = new IResidue(0, "Virtual", "Solvent", "N", 0,
                (unitX * VectorScale) + P0Term.X,
                (unitY * VectorScale) + P0Term.Y,
                (unitZ * VectorScale) + P0Term.Z);



            //Now calculate the distances between the Apex residue and all the other atoms in the PDB (minus the loop residues)
            foreach (IResidue Residue in PdbWithoutLoop)
            {
                if (EuclideanDistance(SolventResidue, Residue) < LoopCriteria.SolventExposedThreshold)
                {
                    //Not solvent accessible
                    return null;
                }
            }

            //Determine the Apex Residue
            IResidue ApexResidue = new IResidue();
            double MaxApexDistance = 0.0;
            double ApexDistance = 0.0;
            foreach (IResidue Residue in LoopResidues)
            {
                ApexDistance = EuclideanDistance(P0Term, Residue);
                if (ApexDistance > MaxApexDistance)
                {
                    MaxApexDistance = ApexDistance;
                    ApexResidue = Residue;
                }
            }
            return ApexResidue;

        }

        private IResidue IsLoopSolventAccessible(List<IResidue> PdbResidues, List<IResidue> LoopResidues, ILoopCriteria LoopCriteria)
        {
            var PdbGuids = from p in PdbResidues
                select p;

            var LoopGuids = from c in LoopResidues
                select c;

            var PdbWithoutLoop = PdbGuids.Except(LoopGuids);

            //Grad the Apex atom from the loop segment
            double MaxApexDistance = 0.0;
            double ApexDistance = 0.0;
            IResidue ApexResidue = new IResidue();
            IResidue NTermResidue = LoopResidues.First();
            IResidue CTermResidue = LoopResidues.Last();

            double TerminalDistance = EuclideanDistance(NTermResidue, CTermResidue);
            IResidue PseudoTermResidue = new IResidue(0, "", "Virtual", "V", 0,
                ((NTermResidue.X + CTermResidue.X) / 2),
                ((NTermResidue.Y+CTermResidue.Y) / 2),
                ((NTermResidue.Z+CTermResidue.Z) / 2));

            //Determine the Apex Residue
            foreach (IResidue Residue in LoopResidues) {
                ApexDistance = EuclideanDistance(PseudoTermResidue, Residue);
                if (ApexDistance > MaxApexDistance) { 
                    MaxApexDistance = ApexDistance;
                    ApexResidue = Residue;
                }
            }

            //Now calculate the distances between the Apex residue and all the other atoms in the PDB (minus the loop residues)
            foreach (IResidue Residue in PdbWithoutLoop) {
                if (EuclideanDistance(ApexResidue, Residue) < LoopCriteria.SolventExposedThreshold) {
                    //Not solvent accessible
                    return null;
                }
            }
                return ApexResidue;
        }

        public List<ILoopTrimer> FindTrimerLoops(ILoopsCollection LoopsCollection, ILoopCriteria LoopCriteria)
        {

            List<ILoopTrimer> LoopTrimers = new List<ILoopTrimer>();

            //Search for pairs if we specify
            if (LoopCriteria.SearchForTrimer) {
                foreach (ILoop LoopA in LoopsCollection.Loops) {
                    foreach (ILoop LoopB in LoopsCollection.Loops) {

                        double InterPairDistanceAB = EuclideanDistance(LoopA.ApexAtom, LoopB.ApexAtom);
                        if (InterPairDistanceAB > LoopCriteria.LoopTrimerDistanceLowerLimit &&
                            InterPairDistanceAB < LoopCriteria.LoopTrimerDistanceUpperLimit)
                        {

                            foreach (ILoop LoopC in LoopsCollection.Loops)
                            {
                                double InterPairDistanceAC = EuclideanDistance(LoopA.ApexAtom, LoopC.ApexAtom);
                                double InterPairDistanceBC = EuclideanDistance(LoopB.ApexAtom, LoopC.ApexAtom);

                                if (InterPairDistanceAC > LoopCriteria.LoopTrimerDistanceLowerLimit &&
                                    InterPairDistanceAC < LoopCriteria.LoopTrimerDistanceUpperLimit &&
                                    InterPairDistanceBC > LoopCriteria.LoopTrimerDistanceLowerLimit &&
                                    InterPairDistanceBC < LoopCriteria.LoopTrimerDistanceUpperLimit)
                                {
                                    ILoopTrimer LoopTrimer = new ILoopTrimer();
                                    LoopTrimer.LoopA = LoopA;
                                    LoopTrimer.LoopB = LoopB;
                                    LoopTrimer.LoopC = LoopC;
                                    LoopTrimers.Add(LoopTrimer);
                                    Console.WriteLine("Added Trimer");

                                }
                            }
                        }
                    }
                }
            }

            return (LoopTrimers);
            //return CleanupLoopPairs(LoopPairs); //TODO -> Cleanup trimers

        }

        public List<ILoopPair> FindMatchedLoops(ILoopsCollection LoopsCollection, ILoopCriteria LoopCriteria)
        {

            List<ILoopPair> LoopPairs = new List<ILoopPair>();

            //Search for pairs if we specify
            if (LoopCriteria.SearchForLoopPairs)
            {
                foreach (ILoop LoopA in LoopsCollection.Loops)
                {
                    foreach (ILoop LoopB in LoopsCollection.Loops)
                    {
                        double InterPairDistance = EuclideanDistance(LoopA.ApexAtom, LoopB.ApexAtom);

                        if(InterPairDistance==0) {
                            //Same loop
                            continue;
                        }

                        double N0Distance = EuclideanDistance(LoopA.NTermAtom, LoopB.NTermAtom);
                        double N1Distance = EuclideanDistance(LoopA.NTermAtomP1, LoopB.NTermAtomP1);
                        double N2Distance = EuclideanDistance(LoopA.NTermAtomP2, LoopB.NTermAtomP2);

                        double C0Distance = EuclideanDistance(LoopA.CTermAtom, LoopB.CTermAtom);
                        double C1Distance = EuclideanDistance(LoopA.CTermAtomP1, LoopB.CTermAtomP1);
                        double C2Distance = EuclideanDistance(LoopA.CTermAtomP2, LoopB.CTermAtomP2);

                        //Old
                        //if (InterPairDistance > LoopCriteria.LoopPairDistanceLowerLimit && 
                        //   InterPairDistance < LoopCriteria.LoopPairDistanceUpperLimit) {

                        //New
                        if( N0Distance < LoopCriteria.NNP0DistanceUpper && N0Distance > LoopCriteria.NNP0DistanceLower &&
                            N1Distance < LoopCriteria.NNP1DistanceUpper && N1Distance > LoopCriteria.NNP1DistanceLower &&
                            N2Distance < LoopCriteria.NNP2DistanceUpper && N2Distance > LoopCriteria.NNP2DistanceLower &&
                            C0Distance < LoopCriteria.CCP0DistanceUpper && C0Distance > LoopCriteria.CCP0DistanceLower && 
                            C1Distance < LoopCriteria.CCP1DistanceUpper && C1Distance > LoopCriteria.CCP1DistanceLower && 
                            C2Distance < LoopCriteria.CCP2DistanceUpper && C2Distance > LoopCriteria.CCP2DistanceLower) {

                                Vector3D vector1 = new Vector3D(1, 0, 0);
                                Vector3D vector2 = new Vector3D(0, 0, 1);
                                double crossProduct3D;

                                


                                // crossProduct is equal to 50    
                                crossProduct3D = Vector3D.AngleBetween(LoopA.UnitVector, LoopB.UnitVector);



                               if (LoopCriteria.RequireLoopsOnSameChain)
                               {
                                   if (crossProduct3D > LoopCriteria.LoopOrientationAngleLower &&
                                       crossProduct3D < LoopCriteria.LoopOrientationAngleUpper)
                                   {
                                       if (LoopA.Chain == LoopB.Chain)
                                       {
                                           ILoopPair LoopPair = new ILoopPair();
                                           LoopPair.LoopA = LoopA;
                                           LoopPair.LoopB = LoopB;
                                           LoopPairs.Add(LoopPair);
                                       }
                                   }
                               }
                            
                        }
                    }
                }
            }

            //return (LoopPairs);
            return CleanupLoopPairs(LoopPairs);

        }

        private List<ILoopPair> CleanupLoopPairs(List<ILoopPair> LoopPairs)
        {
            List<ILoopPair> CleanedLoopPairsCollection = new List<ILoopPair>();
            foreach (ILoopPair LoopPair in LoopPairs)
            {

                var Count = (from p in CleanedLoopPairsCollection
                             where ((p.LoopA.NTermAtom == LoopPair.LoopA.NTermAtom) && (p.LoopB.NTermAtom == LoopPair.LoopB.NTermAtom)) ||
                                   ((p.LoopA.NTermAtom == LoopPair.LoopB.NTermAtom) && (p.LoopB.NTermAtom == LoopPair.LoopA.NTermAtom))
                             select p).Count();

                if (Count == 0)
                {
                    CleanedLoopPairsCollection.Add(LoopPair);
                }

            }
            return CleanedLoopPairsCollection;
        }

        double EuclideanDistance(IResidue a, IResidue b) {
            return Math.Sqrt(((a.X - b.X) * (a.X - b.X)) + ((a.Y - b.Y) * (a.Y - b.Y)) + ((a.Z - b.Z) * (a.Z - b.Z)));
        }

    }
}
