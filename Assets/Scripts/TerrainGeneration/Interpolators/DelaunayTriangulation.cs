using System.Collections.Generic;
using UnityEngine;
using TerrainGeneration.ScriptableObjects;

namespace TerrainGeneration
{
    public class DelaunayTriangulation
    {
        public struct Vertex
        {
            public Vector2 position;
            public Biome biome;
        }

        public struct Triangle
        {
            public Vertex a;
            public Vertex b;
            public Vertex c;

            public bool ContainsPoint(Vector2 p)
            {
                float d1 = Sign(p, a.position, b.position);
                float d2 = Sign(p, b.position, c.position);
                float d3 = Sign(p, c.position, a.position);

                bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
                bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);

                return !(hasNeg && hasPos);
            }

            static float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
            {
                return (p1.x - p3.x) * (p2.y - p3.y)
                     - (p2.x - p3.x) * (p1.y - p3.y);
            }
        }

        struct Edge
        {
            public Vertex a;
            public Vertex b;

            public Edge(Vertex a, Vertex b)
            {
                this.a = a;
                this.b = b;
            }

            public bool Equals(Edge other)
            {
                return (a.position == other.a.position && b.position == other.b.position)
                    || (a.position == other.b.position && b.position == other.a.position);
            }
        }

        public readonly List<Triangle> Triangles = new();

        public DelaunayTriangulation(List<Vertex> points)
        {
            Build(points);
        }

        void Build(List<Vertex> points)
        {
            Triangles.Clear();

            // Super triangle
            float minX = float.MaxValue, minY = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue;

            foreach (var v in points)
            {
                minX = Mathf.Min(minX, v.position.x);
                minY = Mathf.Min(minY, v.position.y);
                maxX = Mathf.Max(maxX, v.position.x);
                maxY = Mathf.Max(maxY, v.position.y);
            }

            float dx = maxX - minX;
            float dy = maxY - minY;
            float delta = Mathf.Max(dx, dy) * 10f;

            Vertex v1 = new() { position = new Vector2(minX - delta, minY - delta) };
            Vertex v2 = new() { position = new Vector2(minX - delta, maxY + delta) };
            Vertex v3 = new() { position = new Vector2(maxX + delta, minY - delta) };

            Triangles.Add(new Triangle { a = v1, b = v2, c = v3 });

            foreach (var point in points)
            {
                List<Triangle> bad = new();
                foreach (var t in Triangles)
                {
                    if (InCircumcircle(point.position, t))
                        bad.Add(t);
                }

                List<Edge> polygon = new();
                foreach (var t in bad)
                {
                    AddEdge(polygon, new Edge(t.a, t.b));
                    AddEdge(polygon, new Edge(t.b, t.c));
                    AddEdge(polygon, new Edge(t.c, t.a));
                }

                foreach (var t in bad)
                    Triangles.Remove(t);

                foreach (var edge in polygon)
                {
                    Triangles.Add(new Triangle
                    {
                        a = edge.a,
                        b = edge.b,
                        c = point
                    });
                }
            }

            Triangles.RemoveAll(t =>
                t.a.biome == null ||
                t.b.biome == null ||
                t.c.biome == null);
        }

        static void AddEdge(List<Edge> edges, Edge edge)
        {
            for (int i = 0; i < edges.Count; i++)
            {
                if (edges[i].Equals(edge))
                {
                    edges.RemoveAt(i);
                    return;
                }
            }
            edges.Add(edge);
        }

        static bool InCircumcircle(Vector2 p, Triangle t)
        {
            float ax = t.a.position.x - p.x;
            float ay = t.a.position.y - p.y;
            float bx = t.b.position.x - p.x;
            float by = t.b.position.y - p.y;
            float cx = t.c.position.x - p.x;
            float cy = t.c.position.y - p.y;

            float det =
                (ax * ax + ay * ay) * (bx * cy - cx * by)
              - (bx * bx + by * by) * (ax * cy - cx * ay)
              + (cx * cx + cy * cy) * (ax * by - bx * ay);

            return det < 0f;
        }

        public Triangle? FindContainingTriangle(Vector2 p)
        {
            foreach (var t in Triangles)
                if (t.ContainsPoint(p))
                    return t;

            return null;
        }
    }
}