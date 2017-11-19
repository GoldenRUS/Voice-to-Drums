using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Numerics;
using System.Collections;

namespace pj050517
{
    class Wav
    {
        public Complex[] output;
        //public List<Complex[]> fft = new List<Complex[]>();
        public Delimed fft = new Delimed();
        public List<Double[]> range = new List<Double[]>();
        //public List<Complex[]> delimed = new List<Complex[]>();
        public Delimed delimed = new Delimed();
        //public  Complex[] fft;
        //public  Complex[] delimed;
        public  int N;
        private  int calcLenth(int length)//округление длинны массива до ближайшей степени 2ки.
        {
            int bit = 2;
            for (int i = 0; i < 30; i++)
            {
                bit *= 2;
                if ((bit == length) || (bit > length)) return bit;
            }
            return 0;
        }

        private  double midleOfFrame(int pos, int framesize)
        {
            double res = 0;
            for (int i = pos; i <= pos + framesize; i++)
            {
                res += Math.Abs(Math.Sqrt(output[i].Real * output[i].Real + output[i].Imaginary * output[i].Imaginary));
            }
            res /= framesize;
            return res;
        }
        public  double midleOfFrame(Complex[] input, int pos, int framesize)
        {
            double res = 0;
            for (int i = pos; i <= pos + framesize; i++)
            {
                res += Math.Sqrt(input[i].Real * input[i].Real + input[i].Imaginary * input[i].Imaginary);
            }
            res /= framesize;
            return res;
        }

        public  double maxOfFrame(Complex[] input, int pos, int framesize)
        {
            double res = 0;
            for (int i = pos; i <= pos + framesize; i++)
            {
                res = Math.Sqrt(input[i].Real * input[i].Real + input[i].Imaginary * input[i].Imaginary) > res ? Math.Sqrt(input[i].Real * input[i].Real + input[i].Imaginary * input[i].Imaginary) : res;
            }
            return res;
        }

        private double maxOfArray(Double[] input)
        {
            double max = input[0];
            for (int i = 1; i < input.Count(); i++)
                max = input[i] > max ? input[i] : max;
            return max;
        }

        public double[] normalize(double[] input)
        {
            Double[] tmp = new Double[input.Count()];
            double k = 100 / maxOfArray(input);
            for (int i = 0; i < input.Count(); i++)
                tmp[i] = input[i] * k;
            return tmp;
        }

        private  void AddDelimed(int start, int end)
        {
            int size = end - start;
            Complex[] tmp = new Complex[calcLenth(size)];
            for(int i = 0; i < size; i++)
            {
                tmp[i] = output[i + start];
            }
            delimed.Add(tmp, start);
        }

        private  bool isSil(int pos)
        {
            const int framesize = 100;
            const double level = 0.01;
            return midleOfFrame(pos, framesize) < level;
        }

        private void delimBySil()
        {
            const int framesize = 500;
            int last = 0;
            bool mark = false;

            for (int i = 0; i < output.Length - framesize; i += framesize)
            {
                if(isSil(i))
                {
                    if(mark)
                    {
                        AddDelimed(last, i);
                        mark = false;
                    }
                }
                else
                {
                    if(!mark)
                    {
                        last = i;
                    }
                    mark = true;
                }
            }
            if(mark)
            {
                AddDelimed(last, output.Length - framesize);
            }
        }


        /*private  Complex[] prepare(String wavePath)
        {
            Complex[] data;
            byte[] wave;
            System.IO.FileStream WaveFile = System.IO.File.OpenRead(wavePath);
            wave = new byte[WaveFile.Length];
            int defleng = (wave.Length - 44) / 4;
            int leng = calcLenth(defleng);
            N = leng;
            data = new Complex[leng];//shifting the headers out of the PCM data;
            WaveFile.Read(wave, 0, Convert.ToInt32(WaveFile.Length));//read the wave file into the wave variable
            for (int i = 0; i < defleng; i++)
            {
                data[i] = (BitConverter.ToInt32(wave, 44 + i * 4)) / 4294967296.0;
            }

            return data;
        } */

        private  Complex[] fstprepare(String wavePath)
        {
            Complex[] data;
            byte[] wave;
            System.IO.FileStream WaveFile = System.IO.File.OpenRead(wavePath);
            wave = new byte[WaveFile.Length];
            N = (wave.Length - 44) / 4;
            data = new Complex[N];//shifting the headers out of the PCM data;
            WaveFile.Read(wave, 0, Convert.ToInt32(WaveFile.Length));//read the wave file into the wave variable
                                                                     /***********Converting and PCM accounting***************/
            for (int i = 0; i < N; i++)
            {
                data[i] = (Complex)(BitConverter.ToInt32(wave, 44 + i * 4)) / 4294967296.0;
            }

            return data;
        }

        public  void parce(String wavePath)
        {
            output = fstprepare(wavePath);
            delimBySil();
            for(int i = 0; i<delimed.Count(); i++)
            {
                fft.Add(FFT.fft(delimed.delimed[i]), delimed.sampletime[i]*2);
            }
            //fft = FFT.fft(output);
            //fft = FFT.nfft(fft);
         }

    }
}