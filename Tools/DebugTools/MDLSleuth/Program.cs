namespace MDLSleuth
{
    internal class Program
    {
        /// <summary>
        /// Really rough tool. Just checks MDL files for information inside the header...
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            // Make sure exactly 1 argument is present
            if (args.Length != 1)
                return;

            // Is this a directory (scan all MDLs) or a file ?
            if (File.Exists(args[0]))
                ScanMDLInfo(args[0]);
            {
                if (!Directory.Exists(args[0]))
                    return;

                DirectoryInfo dirInfo = new DirectoryInfo(args[0]);

                foreach (FileInfo file in dirInfo.GetFiles("*.mdl"))
                    ScanMDLInfo(file.FullName);
            }
        }

        static void ScanMDLInfo(string mdlFilePath)
        {
            Console.Write($"'{Path.GetFileName(mdlFilePath)}' INFO: ");

            if (!Path.GetExtension(mdlFilePath).EndsWith("mdl", StringComparison.InvariantCultureIgnoreCase))
                Console.WriteLine("\tNOT MDL.");
            else
            {
                Console.WriteLine();
                // Open and Read Header...
                using BinaryReader binr = new BinaryReader(File.OpenRead(mdlFilePath));

                byte mdlFlags = binr.ReadByte();
                byte mdlNumSkelAnim     = binr.ReadByte();
                byte mdlNumVertAnim     = binr.ReadByte();
                byte mdlNumTexture      = binr.ReadByte();
                byte mdlNumTmdObject    = binr.ReadByte();
                byte mdlNumUvBlocks     = binr.ReadByte();
                ushort mdlMeshBlockSize = binr.ReadUInt16();
                ushort mdlPad0x08       = binr.ReadUInt16();
                ushort mdlPad0x0A       = binr.ReadUInt16();
                ushort mdlSkelBlockSize = binr.ReadUInt16();
                ushort mdlVertBlockSize = binr.ReadUInt16();

                Console.WriteLine($"Raw Flags = {{ 1 = {(mdlFlags & 1) == 1}, 2 = {(mdlFlags & 2) == 2}, 3 = {(mdlFlags & 4) == 4}, 4 = {(mdlFlags & 8) == 8}, 5 = {(mdlFlags & 16) == 8}, 6 = {(mdlFlags & 32) == 32}, 7 = {(mdlFlags & 64) == 64}, 8 = {(mdlFlags & 128) == 128} }}");
                Console.WriteLine($"Joints Anim Info = {{ Has = {(mdlFlags & 0x1) == 1}, Count = {mdlNumSkelAnim}, Block Size = {mdlSkelBlockSize * 4} }}");
                Console.WriteLine($"Vertex Anim Info = {{ Has = {(mdlFlags & 0x4) == 4}, Count = {mdlNumVertAnim}, Block Size = {mdlVertBlockSize * 4} }}");
                Console.WriteLine($"Mesh Info = {{ Num Object = {mdlNumTmdObject}, Block Size = {mdlMeshBlockSize * 4} }}");
                Console.WriteLine($"Texture Info = {{ Num Texture = {mdlNumTexture}, Num UV Block = {mdlNumUvBlocks} }}");
                Console.WriteLine();
            }
        }
    }
}