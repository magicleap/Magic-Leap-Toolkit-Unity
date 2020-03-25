// ---------------------------------------------------------------------
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/terms/developer
//
// ---------------------------------------------------------------------

using UnityEngine;
using System.Collections.Generic;

namespace MagicLeapTools
{
    //Classes:
    public class Curve
    {
        //Public Properties:
        public Vector3 Start
        {
            get
            {
                return _start;
            }

            set
            {
                if (_start != value)
                {
                    _start = value;
                    Normalize();
                }
            }
        }

        public Vector3 Control
        {
            get
            {
                return _control;
            }

            set
            {
                if (_control != value)
                {
                    _control = value;
                    Normalize();
                }
            }
        }

        public Vector3 End
        {
            get
            {
                return _end;
            }

            set
            {
                if (_end != value)
                {
                    _end = value;
                    Normalize();
                }
            }
        }

        public int Resolution
        {
            get
            {
                return _resolution;
            }

            set
            {
                _resolution = Mathf.Max(2, value);
            }
        }

        public float Length
        {
            get
            {
                return _lengthLookup[_lengthLookup.Count - 1].y;
            }
        }

        //Private Variables:
        private List<Vector2> _lengthLookup = new List<Vector2>(); //x is percentage, y is distance
        private Vector3 _start;
        private Vector3 _control;
        private Vector3 _end;
        private int _resolution;

        //Constructors:
        public Curve(int resolution = 10)
        {
            //sets:
            Resolution = resolution;
        }

        public Curve(Vector3 start, Vector3 control, Vector3 end, int resolution = 10)
        {
            //sets:
            Resolution = resolution;
            _start = start;
            _control = control;
            _end = end;

            Normalize();
        }

        public Curve(Transform start, Transform control, Transform end, int resolution = 10)
        {
            //sets:
            Resolution = resolution;
            _start = start.position;
            _control = control.position;
            _end = end.position;

            Normalize();
        }

        //Private Methods:
        private Vector3 Evaluate(float percentage)
        {
            float oneMinusT = 1 - Mathf.Clamp01(percentage);
            return oneMinusT * oneMinusT * _start + 2f * oneMinusT * percentage * _control + percentage * percentage * _end;
        }

        private void Normalize()
        {
            //reset lookup table:
            _lengthLookup.Clear();
            _lengthLookup.Add(Vector2.zero);

            //create lookup values:
            Vector3 previousPosition = _start;
            for (int i = 0; i < _resolution; i++)
            {
                //non-normalized values:
                float percentage = i / (float)(_resolution - 1);
                Vector3 rawPosition = Evaluate(percentage);
                //float oneMinusT = 1 - percentage;
                //Vector3 rawPosition = oneMinusT * oneMinusT * _start + 2f * oneMinusT * percentage * _control + percentage * percentage * _end;

                //log lookup values:
                if (i > 0)
                {
                    float distance = Vector3.Distance(rawPosition, previousPosition);
                    _lengthLookup.Add(new Vector2(percentage, distance + _lengthLookup[i - 1].y));
                }

                previousPosition = rawPosition;
            }
        }

        //Public Methods:
        /// <summary>
        /// Updates curve with less overhead than if each point was changed separately.
        /// </summary>
        public void Update(Vector3 start, Vector3 control, Vector3 end)
        {
            _start = start;
            _control = control;
            _end = end;
            Normalize();
        }

        /// <summary>
        /// Updates curve with less overhead than if each point was changed separately.
        /// </summary>
        public void Update(Vector3 start, Vector3 control, Vector3 end, int resolution)
        {
            _start = start;
            _control = control;
            _end = end;
            Resolution = resolution;
            Normalize();
        }

        /// <summary>
        /// Updates curve with less overhead than if each point was changed separately.
        /// </summary>
        public void Update(Transform start, Transform control, Transform end)
        {
            _start = start.position;
            _control = control.position;
            _end = end.position;
            Normalize();
        }

        /// <summary>
        /// Updates curve with less overhead than if each point was changed separately.
        /// </summary>
        public void Update(Transform start, Transform control, Transform end, int resolution)
        {
            _start = start.position;
            _control = control.position;
            _end = end.position;
            Resolution = resolution;
            Normalize();
        }

        /// <summary>
        /// Returns the world space location at a specific percentage along the curve.
        /// </summary>
        public Vector3 GetPosition(float percentage)
        {
            //clamp:
            percentage = Mathf.Clamp01(percentage);

            //no need to look anything up if we are at the start:
            if (percentage == 0)
            {
                return _start;
            }

            //no need to look anything up if we are at the end:
            if (percentage == 1)
            {
                return _end;
            }

            //targets:
            float targetLength = percentage * Length;

            //lookup:
            Vector3 position = Vector3.zero;
            for (int i = 0; i < _lengthLookup.Count; i++)
            {
                if (_lengthLookup[i].y >= targetLength)
                {
                    float knownLengthPercentage = Mathf.InverseLerp(_lengthLookup[i - 1].y, _lengthLookup[i].y, targetLength);
                    float interpolatedPercentage = Mathf.Lerp(_lengthLookup[i - 1].x, _lengthLookup[i].x, knownLengthPercentage);
                    position = Evaluate(interpolatedPercentage);
                    break;
                }
            }

            return position;
        }

        /// <summary>
        /// Returns the direction vector at a specific percentage along the curve.
        /// </summary>
        public Vector3 GetDirection(float percentage)
        {
            //targets:
            float targetLength = Mathf.Clamp01(percentage) * Length;
            float interpolatedPercentage = 0;

            if (percentage == 0) //extent:
            {
                interpolatedPercentage = 0;
            }
            else if (percentage == 1) //extent:
            {
                interpolatedPercentage = 1;
            }
            else
            {
                //lookup:
                for (int i = 0; i < _lengthLookup.Count; i++)
                {
                    if (_lengthLookup[i].y >= targetLength)
                    {
                        float knownLengthPercentage = Mathf.InverseLerp(_lengthLookup[i - 1].y, _lengthLookup[i].y, targetLength);
                        interpolatedPercentage = Mathf.Lerp(_lengthLookup[i - 1].x, _lengthLookup[i].x, knownLengthPercentage);
                        break;
                    }
                }
            }

            //calculate:
            return 2 * (1 - interpolatedPercentage) * (_control - _start) + 2 * interpolatedPercentage * (_end - _control);
        }
    }
}