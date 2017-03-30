using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp2
{
    class Program
    {
        public enum Andeby
        {
            Rip=0,
            Rap=1,
            Rup=2
        }

        private static IEnumerable<string> BuildEnu(int x)
        {
            for (int i = 0; i < x; i++)
            {
                Console.WriteLine($"return {i} ({(DateTime.Now)})");
                yield return i.ToString();
            }
        }

        static void Main(string[] args)
        {
            //var lst1 = BuildEnu(5);

            //foreach (var s in lst1)
            //{
            //    Console.WriteLine($"Read1 {s}");
            //}
            //System.Threading.Thread.Sleep(2000);
            //foreach (var s in lst1)
            //{
            //    Console.WriteLine($"Read2 {s}");
            //}

            var p = new Program();
            var typeName = p.GetType().ToString();
            Console.WriteLine(typeName);

            var t2 = Type.GetType(typeName);
            Console.WriteLine(t2.ToString());

            Console.ReadKey(true);

        }
    }
}
