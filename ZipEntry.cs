using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace PartialZip
{
    class ZipEntry
    {   
        public ZipEntry(byte[] zipData, int offset = 0)
        {
            var header = BitConverter.ToInt32(zipData.Skip(offset).Take(4).ToArray());
            if (header != 0x04034b50)
            {
                IsValid = false;
                return; // Zip header invalid
            }
            GeneralPurposeBitFlag = BitConverter.ToInt16(zipData.Skip(offset + 6).Take(2).ToArray());
            var compressionMethod = BitConverter.ToInt16(zipData.Skip(offset + 8).Take(2).ToArray());
            CompressionMethod = (CompressionMethodEnum) compressionMethod;
            CompressedDataSize = BitConverter.ToInt32(zipData.Skip(offset + 18).Take(4).ToArray());
            UncompressedDataSize = BitConverter.ToInt32(zipData.Skip(offset + 22).Take(4).ToArray());
            CRC = BitConverter.ToInt32(zipData.Skip(offset + 14).Take(4).ToArray());
            var fileNameLength = BitConverter.ToInt16(zipData.Skip(offset + 26).Take(2).ToArray());
            FileName = Encoding.UTF8.GetString(zipData.Skip(offset + 30).Take(fileNameLength).ToArray());
            var extraFieldLength = BitConverter.ToInt16(zipData.Skip(offset + 28).Take(2).ToArray());
            ExtraField = zipData.Skip(offset + 30 + fileNameLength).Take(extraFieldLength).ToArray();
            var dataStartIndex = offset + 30 + fileNameLength + extraFieldLength;
            var bCompressed = zipData.Skip(dataStartIndex).Take(CompressedDataSize).ToArray();
            Decompressed = CompressionMethod == CompressionMethodEnum.None ? bCompressed : Deflate(bCompressed);
            NextOffset = dataStartIndex + CompressedDataSize;
        }

        public bool IsValid = true;

        public int NextOffset;
        public byte[] Decompressed { get; set; }

        public byte[] ExtraField { get; set; }

        public string FileName { get; set; }

        public int CRC { get; set; }

        public int CompressedDataSize { get; set; }

        public int UncompressedDataSize { get; set; }

        public CompressionMethodEnum CompressionMethod { get; set; }

        public int GeneralPurposeBitFlag { get; set; }

        public enum CompressionMethodEnum
        {
            None = 0,
            Deflate = 8
        }

        private static byte[] Deflate(byte[] rawData)
        { 
            var memCompress = new MemoryStream(rawData);
            Stream csStream = new DeflateStream(memCompress, CompressionMode.Decompress);
            var msDecompress = new MemoryStream();
            csStream.CopyTo(msDecompress);
            var bDecompressed = msDecompress.ToArray();
            return bDecompressed;
        }
    }
}
