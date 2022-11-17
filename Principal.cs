using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biblioteca
{
    public class Principal
    {
        public static void Main(string[] args)
        {
            Library library = new Library();
            library.RunSimulation();

            Console.ReadKey();
        }
    }
}
