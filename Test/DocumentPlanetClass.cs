using System.Collections.Generic;
using Pdoxcl2Sharp.Naming;

namespace Test
{
    public class DocumentPlanetClass
    {
        [ParadoxAlias("pc_planet")]
        public Dictionary<string, PlanetClass> PlanetClasses { get; set; }
    }
}