using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ndvoronoisharp
{
    public class Facet
    {
        Simplice ParentA { get; set; }
        Simplice ParentB { get; set; }
        Nuclei[] Nucles { get { return ParentA.Nucleis.Intersect(ParentB.Nucleis).ToArray(); } }
    }
}
