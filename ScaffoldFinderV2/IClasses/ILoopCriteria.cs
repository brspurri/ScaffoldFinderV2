using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScaffoldFinderV2
{
    public class ILoopCriteria
    {

        public double TerminiDistanceThreshold = 8; //Angstroms
        public double SolventExposedThreshold = 10; //Angstroms
        public int Length = 8; //Residues

        public bool SolventAccessible;
        public int Start = -1;
        public int End = -1;

        //Loop pairs
        public bool SearchForLoopPairs = true;
        public bool RequireLoopsOnSameChain = true;
        public double LoopPairDistanceLowerLimit = 17; //Angstoms
        public double LoopPairDistanceUpperLimit = 13; //Angstoms

        private const double AdditionalDeviation = 1.0;
        //Loop Distances
        //W206/Ca - N295/Ca => 24.4419477742671
        public double NNP0DistanceLower = 23.4 - AdditionalDeviation;
        public double NNP0DistanceUpper = 25.4 + AdditionalDeviation;

        //D207/Ca - C296/Ca => 24.514307679394072
        public double NNP1DistanceLower = 23.5 - AdditionalDeviation;
        public double NNP1DistanceUpper = 25.5 + AdditionalDeviation;

        //Q208/Ca - T297/Ca => 24.331733620932184
        public double NNP2DistanceLower = 23.3 - AdditionalDeviation;
        public double NNP2DistanceUpper = 25.3 + AdditionalDeviation;


        //S209/Ca - H330/Ca => 24.665282990470644
        public double CCP0DistanceLower = 23.6 - AdditionalDeviation;
        public double CCP0DistanceUpper = 25.6 + AdditionalDeviation;

        //F210/Ca - C331/Ca => 23.097814139004559
        public double CCP1DistanceLower = 22.0 - AdditionalDeviation;
        public double CCP1DistanceUpper = 24.0 + AdditionalDeviation;

        //E211/Ca -N332/Ca => 23.438158417418379
        public double CCP2DistanceLower = 22.4 - AdditionalDeviation;
        public double CCP2DistanceUpper = 24.4 + AdditionalDeviation;

        //Loop Orientation Angle Criteria => 19.78604402342993
        public double LoopOrientationAngleLower = 15.0;
        public double LoopOrientationAngleUpper = 25.0;


        /*
        public double NNP0DistanceLower = 24.8;
        public double NNP0DistanceUpper = 25.8;

        public double NNP1DistanceLower = 23.3;
        public double NNP1DistanceUpper = 24.3;

        public double NNP2DistanceLower = 24.5;
        public double NNP2DistanceUpper = 25.5;


        public double CCP0DistanceLower = 24.1;
        public double CCP0DistanceUpper = 25.1;

        public double CCP1DistanceLower = 22.3;
        public double CCP1DistanceUpper = 23.9;

        public double CCP2DistanceLower = 21.3;
        public double CCP2DistanceUpper = 22.9;
         * */






        public double P0DistanceNTermLower = 6.0;
        public double P0DistanceNTermUpper = 8.0;
        public double P1DistanceNTermLower = 6.0;
        public double P1DistanceNTermUpper = 6.0;
        public double P2DistanceNTermLower = 6.0;
        public double P3DistanceNTermUpper = 6.0;

        public double P0DistanceCTermLower = 6.0;
        public double P0DistanceCTermUpper = 6.0;
        public double P1DistanceCTermLower = 6.0;
        public double P1DistanceCTermUpper = 6.0;
        public double P2DistanceCTermLower = 6.0;
        public double P3DistanceCTermUpper = 6.0;

        //Trimer conditions
        public bool SearchForTrimer = false;
        public double LoopTrimerDistanceLowerLimit = 52; //Angstoms
        public double LoopTrimerDistanceUpperLimit = 54; //Angstoms


        public ILoopCriteria() {
            this.SolventAccessible = true;
        }

        public ILoopCriteria(bool RequireSolventAccessible) {
            this.SolventAccessible = RequireSolventAccessible;
        }

        public ILoopCriteria(bool RequireSolventAccessible, int Start, int End) {
            this.SolventAccessible = RequireSolventAccessible;
            this.Start = Start;
            this.End = End;
        }
    }
}
