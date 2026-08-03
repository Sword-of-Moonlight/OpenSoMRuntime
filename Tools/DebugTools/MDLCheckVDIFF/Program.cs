using System.Diagnostics;

namespace MDLCheckVDIFF
{
    internal class Program
    {
        /// <summary>
        /// Quick throw away. Just checks MDL VDIFF data (which you have to extract manually)
        /// </summary>
        static void Main(string[] args)
        {
            if (args.Length != 1)
                return;

            using BinaryReader binr = new BinaryReader(File.OpenRead(args[0]));

            List<int> buildVDiff = new List<int>();

            int vdiff = 0;

            unchecked
            {
                byte vdiffLen = 0;
                while (binr.BaseStream.Position < binr.BaseStream.Length)
                {
                    // 1. Read Byte
                    byte vdiffPart = binr.ReadByte();

                    // 2. Merge vdiff...
                    vdiff = (vdiff << 7) | ((vdiffPart >> 1) & 0x7F);

                    vdiffLen++;

                    // 3. Is VDIFF complete...
                    if ((vdiffPart & 0x1) == 1 || vdiffLen == 2)
                    {
                        buildVDiff.Add(vdiff);
                        vdiff = 0;
                        vdiffLen = 0;
                    }
                }
            }

            Console.WriteLine($"VDIFF Count = {buildVDiff.Count}");
        }
    }
}
