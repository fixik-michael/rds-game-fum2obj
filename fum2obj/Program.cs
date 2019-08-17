using System;
using System.Globalization;
using System.IO;
using System.Threading;

namespace fum2obj
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            switch (args.Length)
            {
                case 1:
                    ConvertGeometry(args[0], Path.ChangeExtension(args[0], "obj"));
                    break;
                case 2:
                    ConvertGeometry(args[0], args[1]);
                    break;
                default:
                    Console.WriteLine("Usage: fum2obj car.fum [car.obj]");
                    break;
            }
        }

        private static void ConvertGeometry(string inputFile, string outputFile)
        {
            Console.WriteLine("Converting " + Path.GetFileName(inputFile) + "...");

            try
            {
                new FormatConverter().ExtractGeometry(inputFile, outputFile);
            }
            catch (Exception)
            {
                Console.WriteLine("Whoops, something went wrong...");
            }
        }
    }
}
