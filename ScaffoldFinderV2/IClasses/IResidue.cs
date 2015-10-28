using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ScaffoldFinderV2
{
    public class IResidue
    {
        public Guid guid;
        public int Id;

        public string AtomType;
        public string Residue;
        public string Chain;
        public int ResidueNumber;
        public string ResidueExtension;

        public double X;
        public double Y;
        public double Z;

        public IResidue() { }

        public IResidue(int Id, string AtomType, string Residue, string Chain, string ResidueNumber, double X, double Y, double Z)
        {

            this.guid = Guid.NewGuid();
            this.Id = Id;
            this.AtomType = AtomType;
            this.Residue = Residue;
            this.Chain = Chain;
            this.ResidueExtension = Regex.Replace(ResidueNumber, @"\d", "");
            this.ResidueNumber = Convert.ToInt32(Regex.Replace(ResidueNumber, @"[a-z]|[A-Z]", ""));
            this.X = X;
            this.Y = Y;
            this.Z = Z;

        }

        public IResidue(int Id, string AtomType, string Residue, string Chain, int ResidueNumber, double X, double Y, double Z)
        {

            this.guid = Guid.NewGuid();
            this.Id = Id;
            this.AtomType = AtomType;
            this.Residue = Residue;
            this.Chain = Chain;
            this.ResidueNumber = ResidueNumber;
            this.ResidueNumber = 0;
            this.X = X;
            this.Y = Y;
            this.Z = Z;

        }

    }


}
