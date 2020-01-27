using System.IO;
using System.IO.Compression;

namespace MapLoader
{
    public static class ZipExtensions
    {
        public static Stream ExtractToStream(this ZipArchiveEntry entry)
        {
            MemoryStream output = new MemoryStream();

            entry.Open().CopyTo(output);

            return output;
        }
    }
}
