using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace ScaffoldFinderV2
{
    public class ILoop
    {
        public IResidue CTermAtom;
        public IResidue NTermAtom;

        public IResidue CTermAtomP1;
        public IResidue NTermAtomP1;

        public IResidue CTermAtomP2;
        public IResidue NTermAtomP2;

        public Vector3D UnitVector;

        public IResidue ApexAtom;
        public int LoopLength;
        public string PdbCode;
        public string Chain;


    }
}
