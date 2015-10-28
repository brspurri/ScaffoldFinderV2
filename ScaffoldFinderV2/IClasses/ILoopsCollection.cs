using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScaffoldFinderV2
{
    public class ILoopsCollection
    {
        public List<ILoop> Loops = new List<ILoop>();

        public void Add(ILoop Loop)
        {

            Loops.Add(Loop);

        }
    }
}
