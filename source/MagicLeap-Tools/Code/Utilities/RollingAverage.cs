// ---------------------------------------------------------------------
//
// Copyright (c) 2018 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/creator-terms
//
// ---------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MagicLeapTools
{

    ///<summary>
    /// This class can give you an average of data, without having to store the data itself!
    /// For example, instead of using a float[] and averaging all the values, we just track a "sum",
    /// and "numValues (_n)". Then avg = sum/numValues.
    ///</summary>
    public abstract class RollingAverage<T>
    {

        public T Average => _average;

        protected T _sum;
        protected int _n = 0;
        protected int _nLimit = 1000000;
        protected T _average;

        //add another value to the rolling average
        public void AddData(T value)
        {
            _sum = Add(_sum,value);
            _n++;

            _average = Divide(_sum,_n);

            if(_n > _nLimit)
            {
                Reset();
            }
        }

        public abstract T Add(T a, T b);

        public abstract T Divide(T a, int i);

        //clear the data
        public abstract void Reset();

    }

    public class RollingAverageFloat : RollingAverage<float>
    {

        public RollingAverageFloat()
        {
           Reset();
        }

        public override float Add(float a, float b)
        {
            return a + b;
        }

        public override float Divide(float a, int i)
        {
            return a/i;
        }

        public override void Reset()
        {
            _sum = 0.0f;
            _average = 0.0f;
        }
    }

    public class RollingAverageVector3 : RollingAverage<Vector3>
    {

        public RollingAverageVector3()
        {
            Reset();
        }

        public override Vector3 Add(Vector3 a, Vector3 b)
        {
            return a + b;
        }

        public override Vector3 Divide(Vector3 a, int i)
        {
            return a / i;
        }

        public override void Reset()
        {
            _sum = Vector3.zero;
            _average = Vector3.zero;
        }
    }

}
