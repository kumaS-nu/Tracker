using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace kumaS.Extention
{
    /// <summary>
    /// AverageのVecter3拡張       Average Vecter 3
    /// </summary>
    static class CalcAverage
    {
        static public Vector3 Average(this LinkedList<Vector3> vector3s)
        {
            var vector = new Vector3();
            foreach (var vec in vector3s)
            {
                vector += vec;
            }
            vector /= vector3s.Count;

            return vector;
        }

        static public Vector3 Average(this IEnumerable<Vector3> vector3s)
        {
            var vector = new Vector3();
            foreach (var vec in vector3s)
            {
                vector += vec;
            }
            vector /= vector3s.Count();

            return vector;
        }

        static public Vector3[] Average(this LinkedList<Vector3[]> vector3s)
        {
            var vector = new Vector3[vector3s.First.Value.Length];
            foreach (var vec in vector3s)
            {
                for (int i = 0; i < vector3s.First.Value.Length; i++)
                {
                    vector[i] += vec[i];
                }
            }

            for (int i = 0; i < vector3s.First.Value.Length; i++)
            {
                vector[i] /= vector3s.Count;
            }

            return vector;
        }

        static public Vector3[] Average(this IEnumerable<Vector3[]> vector3s)
        {
            int length = vector3s.First().Length;
            int count = vector3s.Count();
            var vector = new Vector3[length];
            foreach (var vec in vector3s)
            {
                for (int i = 0; i < length; i++)
                {
                    vector[i] += vec[i];
                }
            }
            for (int i = 0; i < length; i++)
            {
                vector[i] /= count;
            }

            return vector;
        }

        static public Vector2 Average(this LinkedList<Vector2> vector2s)
        {
            var vector = new Vector2();
            foreach (var vec in vector2s)
            {
                vector += vec;
            }
            vector /= vector2s.Count;

            return vector;
        }

        static public Vector2 Average(this IEnumerable<Vector2> vector2s)
        {
            var vector = new Vector2();
            foreach (var vec in vector2s)
            {
                vector += vec;
            }
            vector /= vector2s.Count();

            return vector;
        }

        static public Vector2[] Average(this LinkedList<Vector2[]> vector2s)
        {
            var vector = new Vector2[vector2s.First.Value.Length];
            foreach (var vec in vector2s)
            {
                for (int i = 0; i < vector2s.First.Value.Length; i++)
                {
                    vector[i] += vec[i];
                }
            }

            for (int i = 0; i < vector2s.First.Value.Length; i++)
            {
                vector[i] /= vector2s.Count;
            }

            return vector;
        }

        static public Vector2[] Average(this IEnumerable<Vector2[]> vector2s)
        {
            int length = vector2s.First().Length;
            int count = vector2s.Count();
            var vector = new Vector2[length];
            foreach (var vec in vector2s)
            {
                for (int i = 0; i < length; i++)
                {
                    vector[i] += vec[i];
                }
            }
            for (int i = 0; i < length; i++)
            {
                vector[i] /= count;
            }

            return vector;
        }
    }
}