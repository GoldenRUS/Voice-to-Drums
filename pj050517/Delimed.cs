using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace pj050517
{
    class Delimed
    {
        public List<Complex[]> delimed = new List<Complex[]>();
        public List<int> sampletime = new List<int>();
        public List<double> time = new List<double>();

        public void Add(Complex[] wav, int samples)
        {
            delimed.Add(wav);
            sampletime.Add(samples);
            time.Add((double) samples / 22050);
        }

        public long Count()
        {
            return delimed.Count();
        }
    }
}
