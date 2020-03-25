// ---------------------------------------------------------------------
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/terms/developer
//
// ---------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MagicLeapTools
{
    public static class Lines
    {
        //Private Variables:
        private static Dictionary<string, LineRenderer> _lines = new Dictionary<string, LineRenderer>();
        private static float _lineWidth;
        private static Shader _defaultShader;

        //Public Methods:
        public static LineRenderer DrawRay(string name, Color startColor, Color endColor, Vector3 origin, Vector3 direction, float width = .0005f)
        {
            LineRenderer line = VerifyLine(name);
            if (line != null)
            {
                line.startColor = startColor;
                line.endColor = endColor;
                line.startWidth = line.endWidth = width;
                line.SetPositions(new Vector3[] { origin, origin + direction });
            }
            return line;
        }

        public static LineRenderer DrawRay(string name, Color startColor, Color endColor, Ray ray, float width = .0005f)
        {
            LineRenderer line = VerifyLine(name);
            if (line != null)
            {
                line.startColor = startColor;
                line.endColor = endColor;
                line.startWidth = line.endWidth = width;
                line.SetPositions(new Vector3[] { ray.origin, ray.origin + ray.direction });
            }
            return line;
        }

        public static LineRenderer DrawLine(string name, Color startColor, Color endColor, float width = .0005f, params Vector3[] positions)
        {
            LineRenderer line = VerifyLine(name);
            if (line != null)
            {
                line.startColor = startColor;
                line.endColor = endColor;
                line.startWidth = line.endWidth = width;
                line.positionCount = positions.Length;
                line.SetPositions(positions);
            }
            return line;
        }

        public static LineRenderer DrawLine(string name, Color startColor, Color endColor, float width = .0005f, params Transform[] positions)
        {
            LineRenderer line = VerifyLine(name);
            if (line != null)
            {
                line.startColor = startColor;
                line.endColor = endColor;
                line.startWidth = line.endWidth = width;
                line.positionCount = positions.Length;
                List<Vector3> pos = new List<Vector3>();
                foreach (var item in positions)
                {
                    pos.Add(item.position);
                }
                line.SetPositions(pos.ToArray());
            }
            return line;
        }

        public static void SetVisibility(string name, bool visible)
        {
            LineRenderer line = VerifyLine(name);
            if (line != null)
            {
                line.enabled = visible;
            }
        }

        public static void SetColors(string name, Color startColor, Color endColor)
        {
            LineRenderer line = VerifyLine(name);
            if (line != null)
            {
                line.startColor = startColor;
                line.endColor = endColor;
            }
        }

        public static void SetWidth(string name, float width)
        {
            LineRenderer line = VerifyLine(name);
            if (line != null)
            {
                line.startWidth = line.endWidth = width;
            }
        }

        public static void DestroyLine(string name)
        {
            if (_lines.ContainsKey(name))
            {
                DestroyLine(_lines[name]);
                Object.Destroy(_lines[name].gameObject);
                _lines.Remove(name);
            }
        }

        public static void DestroyLine(LineRenderer lineRenderer)
        {
            if (_lines.ContainsKey(lineRenderer.name))
            {
                Object.Destroy(lineRenderer.gameObject);
                _lines.Remove(lineRenderer.name);
            }
        }

        public static void DestroyAllLines()
        {
            foreach (var item in _lines)
            {
                Object.Destroy(item.Value.gameObject);
            }
            _lines.Clear();
        }

        //Private Methods:
        private static LineRenderer VerifyLine(string name)
        {
            if (_lineWidth <= 0)
            {
                _lineWidth = .0005f;
            }

            if (!_lines.ContainsKey(name))
            {
                LineRenderer newLine = new GameObject($"({name})").AddComponent<LineRenderer>();
                if (newLine != null)
                {
                    if (_defaultShader == null)
                    {
                        _defaultShader = Shader.Find("Mobile/Particles/Additive");
                    }
                    newLine.material = new Material(_defaultShader);
                    newLine.startWidth = _lineWidth;
                    _lines.Add(name, newLine);
                }
                return newLine;
            }
            else
            {
                return _lines[name];
            }
        }
    }
}