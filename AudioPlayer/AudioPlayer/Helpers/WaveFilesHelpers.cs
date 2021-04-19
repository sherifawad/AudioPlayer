using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AudioPlayer.Helpers
{
    public class WaveFilesHelpers
    {
        public int length;
        public short channels;
        public int samplerate;
        public int DataLength;
        public short BitsPerSample;
        private void WaveHeaderIN(string spath)
        {
            using (var fs = new FileStream(spath, FileMode.Open, FileAccess.Read))
            using (var br = new BinaryReader(fs))
            {
                length = (int)fs.Length - 8;
                fs.Position = 22;
                channels = br.ReadInt16();
                fs.Position = 24;
                samplerate = br.ReadInt32();
                fs.Position = 34;

                BitsPerSample = br.ReadInt16();
                DataLength = (int)fs.Length - 44;
            }
        }

        private void WaveHeaderOUT(string sPath)
        {
            using (FileStream fs = new FileStream(sPath, FileMode.Create, FileAccess.Write))
            using (BinaryWriter bw = new BinaryWriter(fs))
            {
                bw.Write(new char[4] { 'R', 'I', 'F', 'F' });

                bw.Write(length);

                bw.Write(new char[8] { 'W', 'A', 'V', 'E', 'f', 'm', 't', ' ' });

                bw.Write((int)16);

                bw.Write((short)1);
                bw.Write(channels);

                bw.Write(samplerate);

                bw.Write((int)(samplerate * ((BitsPerSample * channels) / 8)));

                bw.Write((short)((BitsPerSample * channels) / 8));

                bw.Write(BitsPerSample);

                bw.Write(new char[4] { 'd', 'a', 't', 'a' });
                bw.Write(DataLength);
            }
        }

        public static string Merge(List<string> files, string outfile)
        {
            var wa_IN = new WaveFilesHelpers();
            var wa_out = new WaveFilesHelpers();

            wa_out.DataLength = 0;
            wa_out.length = 0;


            //Gather header data
            foreach (string path in files)
            {
                if (string.IsNullOrEmpty(path))
                    continue;
                wa_IN.WaveHeaderIN(@path);
                wa_out.DataLength += wa_IN.DataLength;
                wa_out.length += wa_IN.length;

            }

            //Recontruct new header
            wa_out.BitsPerSample = wa_IN.BitsPerSample;
            wa_out.channels = wa_IN.channels;
            wa_out.samplerate = wa_IN.samplerate;
            wa_out.WaveHeaderOUT(@outfile);

            byte[] arrfile;
            foreach (string path in files)
            {
                if (string.IsNullOrEmpty(path))
                    continue;
                using (FileStream fs = new FileStream(@path, FileMode.Open, FileAccess.Read))
                {
                    arrfile = new byte[fs.Length - 44];
                    fs.Position = 44;
                    fs.Read(arrfile, 0, arrfile.Length);
                }
                using (FileStream fo = new FileStream(@outfile, FileMode.Append, FileAccess.Write))
                using (BinaryWriter bw = new BinaryWriter(fo))
                {
                    bw.Write(arrfile);
                }
            }

            return outfile;
        }
    }
}