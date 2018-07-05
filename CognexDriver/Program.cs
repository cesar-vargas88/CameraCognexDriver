using System;
using System.Threading;

namespace CognexDriver
{
    class Program
    {
        static void Main(string[] args)
        {
            Camera camera = new Camera("169.254.248.227", 2001, "169.254.6.45", 23, 250);

            while (true)
            {
                Console.WriteLine("Trigger......");
                Console.WriteLine(DateTime.Now.ToString("dd-MM-yyyy  HH.mm.ss.fff  "));
                Console.WriteLine("Response..... " + camera.Trigger("admin\r\n", "\r\n", "se8"));
                Console.WriteLine(DateTime.Now.ToString("dd-MM-yyyy  HH.mm.ss.fff  \n"));
                Thread.Sleep(1000);
            }
        }
    }
}
