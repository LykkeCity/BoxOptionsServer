using BoxOptions.Common.Models;
using System;
using System.Collections.Generic;

namespace BoxOptions.CoefficientCalculator.Algo
{
    public class Runner
    {
        public double prevDC;
        public double extreme;
        public double deltaUp;  // 1% == 0.01
        public double deltaDown;
        public int type; // means the latest observed direction
        public bool initalized;
        public int numberDC;

        //TODO: Check Java Compat: 
        //public LinkedList<Long> timesDC;
        //public LinkedList<Double> osLengths;
        public LinkedList<long> timesDC;
        public LinkedList<double> osLengths;

        public double avSqrtVar;
        public double osL;

        public Runner(double deltaUp, double deltaDown, int type)
        {
            this.type = type; this.deltaUp = deltaUp; this.deltaDown = deltaDown; initalized = false; numberDC = 0;
            timesDC = new LinkedList<long>();
            osLengths = new LinkedList<double>();
        }

        public int Run(Price aPrice)
        {

            if (!initalized)
            {
                initalized = true;
                timesDC.AddLast(aPrice.Time);
                osL = 0.0;
                osLengths.AddLast(0.0);
                numberDC += 1;
                if (type == -1)
                {
                    extreme = aPrice.Ask;
                    prevDC = aPrice.Ask;
                }
                else if (type == 1)
                {
                    extreme = aPrice.Bid;
                    prevDC = aPrice.Bid;
                }
                return type;
            }
            else
            {

                if (type == -1)
                {
                    if (aPrice.Ask < extreme)
                    {
                        extreme = aPrice.Ask;
                        return 0;

                    }
                    else if (Math.Log(aPrice.Bid / extreme) >= deltaUp)
                    {
                        osL = Math.Abs(Math.Log(extreme / prevDC));
                        osLengths.AddLast(osL);
                        timesDC.AddLast(aPrice.Time);
                        prevDC = aPrice.Bid;
                        extreme = aPrice.Bid;
                        type = 1;
                        numberDC += 1;
                        return 1;
                    }

                }
                else if (type == 1)
                {
                    if (aPrice.Bid > extreme)
                    {
                        extreme = aPrice.Bid;
                        return 0;

                    }
                    else if (-Math.Log(aPrice.Ask / extreme) >= deltaDown)
                    {
                        osL = Math.Abs(Math.Log(extreme / prevDC));
                        osLengths.AddLast(osL);
                        timesDC.AddLast(aPrice.Time);
                        prevDC = aPrice.Ask;
                        extreme = aPrice.Ask;
                        type = -1;
                        numberDC += 1;
                        return -1;
                    }
                }
            }

            return 0;
        }
        public double ComputeSqrtVar()
        {
            avSqrtVar = 0.0;
            if (numberDC == 0)
            {
                return avSqrtVar;
            }
            else
            {
                //for (double osL : osLengths)
                //{
                //    avSqrtVar += Math.pow(osL - deltaUp, 2);
                //}
                // Local var name might conflict
                foreach (double osLen in osLengths)
                {
                    avSqrtVar += Math.Pow(osLen - deltaUp, 2);
                }
                return avSqrtVar;
            }
        }
    }
}
