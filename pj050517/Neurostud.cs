using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pj050517
{
    class Neurostud
    {
        public void teach(int name, Double[] input)
        {
            StreamWriter sw = new StreamWriter(name.ToString()+".dat");
            for (int i = 0; i < input.Count(); i++)
            {
                sw.WriteLine(Convert.ToInt32(input[i]).ToString());
            }
            sw.Close();
        }
    }
}
