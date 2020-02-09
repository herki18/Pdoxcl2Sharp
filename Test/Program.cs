using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Pdoxcl2Sharp;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = new ParadoxSerializerOptions();
            var input = @"pc_planet = { entity=""Test"" }";
            var mem = new MemoryStream(Encoding.ASCII.GetBytes(input));
            var res = ParadoxSerializer.DeserializeAsync<DocumentPlanetClass>(mem, options);
            
            Console.WriteLine("Data:");
            Console.WriteLine(res.Result.PlanetClasses.Count);
        }
    }
}