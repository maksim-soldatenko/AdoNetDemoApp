using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DapperDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            var dpr = new DapperOperations();

            var run = true;

            while (run)
            {
                Console.WriteLine("What operation (CRUD) do you want to perform?");
                var op = Console.ReadLine();

                switch (op.ToUpper())
                {
                    case "C":
                        dpr.Create();
                        break;
                    case "R":
                        dpr.Read();
                        break;
                    case "U":
                        dpr.Update("vasyl");
                        break;
                    case "D":
                        dpr.Delete("vasyl");
                        break;
                    case "E":
                        run = false;
                        break;
                    default:
                        Console.WriteLine("Undefined operation");
                        break;
                }
            }
        }
    }
}
