using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pj050517
{
    class Output
    {
        public List<int> name = new List<int>();
        public List<int> sampletime = new List<int>();
        public List<double> time = new List<double>();
        public List<int[]> samples = new List<int[]>();
        private short[] output;
        private List<byte> _output = new List<byte>();

        public Boolean chgBpm = false;
        public Boolean staticBpm = false;
        public int newBpm = 0;

        public void LoadSamples(int count)
        {
            for(int i = 0; i < count; i++)
            {
                int[] tmp;
                openWav(i.ToString() + ".sp", out tmp);
                samples.Add(tmp);
            }
        }

        public int getBpm()
        {
            if (time.Count() > 1)
            {
                int bpm =(int)(60/(time[1]-time[0]));

                for (int i = 2; i < time.Count() - 1; i++)
                {
                    bpm = (int)((60 / (time[i + 1] - time[i]))+bpm)/2;
                }

                return bpm;
            }
            else
            {
                return 0;
            }
        }
        public void Make()
        {
            float coef = chgBpm ? newBpm / getBpm() : 1.0f;

                output = new short[(int)(sampletime.Last()*coef) + samples[name.Last()].Count()]; //задает длинну - номер семпла последнего распознаного семпла+длинна этого семпла
                for (int j = 0; j < sampletime.Count(); j++)
                {
                    for (int i = 0; i < samples[name[j]].Length; i++)
                    {
                        if (output.Length - 1 < sampletime[j] + i) Array.Resize(ref output, output.Length + 1);
                        output[(int)(sampletime[j]*coef) + i] = (short)(output[(int)(sampletime[j]*coef) + i] + samples[name[j]][i]);
                    }
                }
                List<byte> op = new List<byte>();
                foreach (short i in output)
                {
                    op.Add((byte)(i / 256));
                    op.Add((byte)((i ^ 255 << 8) / 256));
                }
                WaveFormat waveFormat = new WaveFormat(44100, 16, 1);
                using (WaveFileWriter writer = new WaveFileWriter("out.wav", waveFormat))
                {
                    writer.Write(op.ToArray(), 0, op.ToArray().Length);
                }
        }

        static int bytesToShort(byte firstByte, byte secondByte)
        {
            return  (int)((secondByte << 8) | firstByte);
        }

        // Returns left and right double arrays. 'right' will be null if sound is mono.
        public void openWav(string filename, out int[] left/*, out double[] right*/)
        {
            byte[] wav = File.ReadAllBytes(filename);

            // Determine if mono or stereo
            int channels = wav[22];     // Forget byte 23 as 99.999% of WAVs are 1 or 2 channels

            // Get past all the other sub chunks to get to the data subchunk:
            int pos = 12;   // First Subchunk ID from 12 to 16

            // Keep iterating until we find the data chunk (i.e. 64 61 74 61 ...... (i.e. 100 97 116 97 in decimal))
            while (!(wav[pos] == 100 && wav[pos + 1] == 97 && wav[pos + 2] == 116 && wav[pos + 3] == 97))
            {
                pos += 4;
                int chunkSize = wav[pos] + wav[pos + 1] * 256 + wav[pos + 2] * 65536 + wav[pos + 3] * 16777216;
                pos += 4 + chunkSize;
            }
            pos += 8;

            // Pos is now positioned to start of actual sound data.
            int samples = (wav.Length - pos) / 2;     // 2 bytes per sample (16 bit sound mono)
            //if (channels == 2) samples /= 2;        // 4 bytes per sample (16 bit stereo)

            // Allocate memory (right will be null if only mono sound)
            left = new int[samples];
            /*if (channels == 2) right = new double[samples];
            else right = null;*/

            // Write to double array/s:
            int i = 0;
            while (pos < wav.Length)
            {
                left[i] = bytesToShort(wav[pos], wav[pos + 1]);
                pos += 2;
               /* if (channels == 2)
                {
                    //right[i] = bytesToDouble(wav[pos], wav[pos + 1]);
                    pos += 2;
                }*/
                i++;
            }
        }

        public void Add(int _name, int _samplename, double _time)
        {
            name.Add(_name);
            sampletime.Add(_samplename);
            time.Add(_time);
        }


    }
}
