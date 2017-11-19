using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace pj050517
{
    class Neuron
    {
        private const int line = 8;
        private int[] memory;
        private int[] input;
        public int name;
        private int output = -1;

        public int[] readMemFromFile()
        {
            int[] tmp = new int[line];
            String[] str = File.ReadAllLines(name + ".dat");
            for(int i = 0; i < line; i++)
            {
                tmp[i] = Convert.ToInt32(str[i]);
            }
            return tmp;
        }

        public Neuron(int _name)
        {
            name = _name;
            memory = readMemFromFile();
        }
        public int compute(int[] _in)
        {
            input = _in;
            output = 1000;
            for(int i = 0; i < line; i++)
            {
                output -= Math.Abs(memory[i] - input[i]) > 10 ? Convert.ToInt32(Math.Sqrt(Math.Abs(memory[i] - input[i]))) : 0;
        }
            return output;
        }
    }
}
