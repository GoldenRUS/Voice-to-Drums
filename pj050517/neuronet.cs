using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pj050517
{
    class Neuronet
    {
        private Neuron[] neurs;
       // public int[] masses;
        public Neuronet(int size)
        {
            neurs = new Neuron[size];
            //masses = new int[size];
            for(int i = 0; i < size; i++)
            {
                Neuron tmp = new Neuron(i);
                neurs[i] = tmp;
                tmp = null;
            }
        }

        public int compute(Double[] input)
        {
            int res;
            int name;
            int[] _in = new int[input.Count()];
            for(int i = 0; i<input.Count(); i++)
            {
                _in[i] = Convert.ToInt32(input[i]);
            }
            res = neurs[0].compute(_in);
            //masses[0] = res;
            name = 0;
            for(int i = 1; i < neurs.Count(); i++)
            {
                if(neurs[i].compute(_in)>res)
                {
                    name = i;
                    res = neurs[i].compute(_in);
                }
            }
            return name;
        }
    }
}
