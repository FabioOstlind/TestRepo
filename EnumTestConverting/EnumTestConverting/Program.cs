using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnumTestConverting
{
    internal class Program
    {
        [Flags]
        public enum Days : int
        {
            Invalid = 0x00,

            Monday = 0x02,

            Tuesday = 0x04,

            Wednesday = 0x08,

            Friday = 0x10,

            Saturday = 0x20,

            Sunday = 0x40

        }

        private static void Main(string[] args)
        {
            DayOfWeek day = DayOfWeek.Saturday;
            int nr = (int)day;

            Console.WriteLine("{0}", nr);
        }
    }

}