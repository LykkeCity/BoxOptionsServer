using System;
using System.Collections.Generic;
using System.Text;
using BoxOptions.Common.Models;

namespace BoxOptions.CoefficientCalculator.Algo
{
    internal class Runner
    {
        public double _prevDC;
        public double _extreme;
        public double _deltaUp;  // 1% == 0.01
        public double _deltaDown;
        public int _type; // means the latest observed direction
        public bool _initalized;
        public int _numberDC;
        public LinkedList<long> _timesDC;
        public LinkedList<double> _osLengths;
        //public double _avSqrtVar;
        public double _osL;
        private LinkedList<IE> _ieLinkedList;
        private long _movingWindow;

        public Runner(double deltaUp, double deltaDown, int type, long movingWindow)
        {
            _type = type;
            _deltaUp = deltaUp;
            _deltaDown = deltaDown;
            _initalized = false;
            _numberDC = 0;
            _timesDC = new LinkedList<long>();
            _osLengths = new LinkedList<double>();
            _ieLinkedList = new LinkedList<IE>();
            _movingWindow = movingWindow;
        }

        internal int Run(Price aPrice)
        {
            if (!_initalized)
            {
                _initalized = true;
                _timesDC.AddLast(aPrice.Time);
                _osL = 0.0;
                _osLengths.AddLast(0.0);
                _numberDC += 1;
                if (_type == -1)
                {
                    _extreme = aPrice.Ask;
                    _prevDC = aPrice.Ask;
                }
                else if (_type == 1)
                {
                    _extreme = aPrice.Bid;
                    _prevDC = aPrice.Bid;
                }
                double sqrtVar = ComputeSqrtVar(_osL, _deltaUp);
                IE ie = new IE(_type, aPrice.Time, _prevDC, _osL, sqrtVar);
                _ieLinkedList.AddLast(ie);
                return _type;
            }
            else
            {

                if (_type == -1)
                {
                    if (aPrice.Ask < _extreme)
                    {
                        _extreme = aPrice.Ask;
                        return 0;

                    }
                    else if (Math.Log(aPrice.Bid / _extreme) >= _deltaUp)
                    {
                        _osL = Math.Abs(Math.Log(_extreme / _prevDC));
                        _osLengths.AddLast(_osL);
                        _timesDC.AddLast(aPrice.Time);
                        _prevDC = aPrice.Bid;
                        _extreme = aPrice.Bid;
                        _type = 1;
                        _numberDC += 1;
                        double sqrtVar = ComputeSqrtVar(_osL, _deltaDown);
                        IE ie = new IE(_type, aPrice.Time, aPrice.Bid, _osL, sqrtVar);
                        _ieLinkedList.AddLast(ie);
                        RemoveOldIEsIfAny(aPrice.Time);
                        return 1;
                    }

                }
                else if (_type == 1)
                {
                    if (aPrice.Bid > _extreme)
                    {
                        _extreme = aPrice.Bid;
                        return 0;

                    }
                    else if (-Math.Log(aPrice.Ask / _extreme) >= _deltaDown)
                    {
                        _osL = Math.Abs(Math.Log(_extreme / _prevDC));
                        _osLengths.AddLast(_osL);
                        _timesDC.AddLast(aPrice.Time);
                        _prevDC = aPrice.Ask;
                        _extreme = aPrice.Ask;
                        _type = -1;
                        _numberDC += 1;
                        double sqrtVar = ComputeSqrtVar(_osL, _deltaUp);
                        IE ie = new IE(_type, aPrice.Time, aPrice.Ask, _osL, sqrtVar);
                        _ieLinkedList.AddLast(ie);
                        RemoveOldIEsIfAny(aPrice.Time);
                        return -1;
                    }
                }
            }

            return 0;
        }

        /**
        * Computes squared variability of one overshoot
        * @param osL is the size of an overshoot
        * @param delta is the size of the relevant threshold
        * @return squared variability of the overshoot
        */
        private double ComputeSqrtVar(double osL, double delta)
        {
            return Math.Pow(osL - delta, 2);
        }

        /**
         * This part is needed to have IEs with correct prices after weekends.
         * @param time
         */
        internal void AddTimeToIEs(long time)
        {
            foreach(var ie in _ieLinkedList)
            {
                ie.Time = (ie.Time + time);
            }
        }

        /**
        * The methods compute summ of all squared variabilities of overshoots in the ieLinkedList
        * @return
        */
        internal double ComputeTotalSqrtVar()
        {
            double totalSqrtVar = 0;
            foreach (var ie in _ieLinkedList)
            {
                totalSqrtVar += ie.SqrtOsDeviation;
            }
            return totalSqrtVar;
        }

        /**
        * The method should remove old IEs from the list
        * @param currentTime is really the current time
        */
        private void RemoveOldIEsIfAny(long currentTime)
        {
            while (_ieLinkedList.Count != 0 
                && currentTime - _ieLinkedList.First.Value.Time > _movingWindow)
            {
                _ieLinkedList.RemoveFirst();
            }
        }
    }
}
