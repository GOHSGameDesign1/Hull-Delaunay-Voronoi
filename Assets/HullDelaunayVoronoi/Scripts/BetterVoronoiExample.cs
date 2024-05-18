using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HullDelaunayVoronoi.Delaunay;
using HullDelaunayVoronoi.Primitives;
using HullDelaunayVoronoi.Voronoi;

namespace HullDelaunayVoronoi
{
    public class BetterVoronoiExample : MonoBehaviour
    {
        public int NumberOfVertices = 1000;

        public float size = 10;

        public int seed = 0;

        private DelaunayTriangulation2 delaunay;

        private VoronoiMesh2 voronoi;

        private Material lineMaterial;

        List<Vertex2> vertices = new List<Vertex2>();

        List<Vertex2> OGvertices = new List<Vertex2>();

        // Start is called before the first frame update
        void Start()
        {
            lineMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));

            Random.InitState(seed);
            for (int i = 0; i < NumberOfVertices; i++)
            {
                float x = size * Random.Range(-1.0f, 1.0f);
                float y = size * Random.Range(-1.0f, 1.0f);

                vertices.Add(new Vertex2(x, y));
                OGvertices.Add(new Vertex2(x, y));
            }

            delaunay = new DelaunayTriangulation2();
            delaunay.Generate(vertices);

            GenerateVoronoiFromDelaunay(delaunay);

            StartCoroutine(MovePoints());
        }

        private void GenerateVoronoiFromDelaunay(DelaunayTriangulation2 delaunay)
        {
            voronoi = new VoronoiMesh2();
            List<Vertex2> voronoiVertices = new List<Vertex2>();
            foreach (DelaunayCell<Vertex2> cell in delaunay.Cells)
            {
                voronoiVertices.Add(cell.CircumCenter);
            }
            voronoi.Generate(vertices);
        }

        // Update is called once per frame
        void Update()
        {

        }

        IEnumerator MovePoints()
        {
            float t = 0;
            List<float> offsets = new List<float>();
            for(int i=0;i<vertices.Count; i++)
            {
                offsets.Add(Random.Range(1.0f, 5.0f));
            }
            while(true)
            {
                t += Time.deltaTime;
                //foreach (Vertex2 v in vertices)
                //{
                //    v.X += 0.01f;
                //    v.Y = 0.1f;
                //}

                for(int i = 0; i < vertices.Count; i++)
                {
                    Vertex2 v = vertices[i];
                    Vertex2 ogV = OGvertices[i];
                    float offset = offsets[i];

                    v.X = ogV.X + Mathf.Cos(t * offset);
                    v.Y = ogV.Y + Mathf.Sin(t * offset);
                }

                delaunay.Generate(vertices);
                GenerateVoronoiFromDelaunay(delaunay);

                yield return null;
            }
        }

        private void OnPostRender()
        {
            if (delaunay == null || delaunay.Cells.Count == 0 || delaunay.Vertices.Count == 0) return;

            GL.PushMatrix();

            GL.LoadIdentity();
            GL.MultMatrix(GetComponent<Camera>().worldToCameraMatrix);
            GL.LoadProjectionMatrix(GetComponent<Camera>().projectionMatrix);

            //lineMaterial.SetPass(0);
            //GL.Begin(GL.LINES);

            //GL.Color(Color.red);

            //foreach (DelaunayCell<Vertex2> cell in delaunay.Cells)
            //{
            //    DrawSimplex(cell.Simplex);
            //}

            //GL.Color(Color.green);

            //foreach (DelaunayCell<Vertex2> cell in delaunay.Cells)
            //{
            //    DrawCircle(cell.CircumCenter, cell.Radius, 32);
            //}

            //GL.End();

            // DRAW VORONOI LINES
            lineMaterial.SetPass(0);
            GL.Begin(GL.LINES);

            GL.Color(Color.green);

            foreach (VoronoiRegion<Vertex2> region in voronoi.Regions)
            {
                bool draw = true;

                foreach (DelaunayCell<Vertex2> cell in region.Cells)
                {
                    if (!InBound(cell.CircumCenter))
                    {
                        //draw = false;
                        //break;
                    }
                }

                if (!draw) continue;

                foreach (VoronoiEdge<Vertex2> edge in region.Edges)
                {
                    Vertex2 v0 = edge.From.CircumCenter;
                    Vertex2 v1 = edge.To.CircumCenter;

                    DrawLine(v0, v1);
                }
            }

            GL.End();

            // DRAW POINTS
            GL.Begin(GL.QUADS);
            GL.Color(Color.yellow);

            foreach (Vertex2 v in delaunay.Vertices)
            {
                DrawPoint(v);
            }

            GL.End();

            GL.Begin(GL.QUADS);
            GL.Color(Color.blue);

            foreach (DelaunayCell<Vertex2> cell in delaunay.Cells)
            {
                DrawPoint(cell.CircumCenter);
            }

            GL.End();

            GL.PopMatrix();
        }

        private void DrawLine(Vertex2 v0, Vertex2 v1)
        {
            GL.Vertex3(v0.X, v0.Y, 0.0f);
            GL.Vertex3(v1.X, v1.Y, 0.0f);
        }

        private void DrawSimplex(Simplex<Vertex2> f)
        {

            GL.Vertex3(f.Vertices[0].X, f.Vertices[0].Y, 0.0f);
            GL.Vertex3(f.Vertices[1].X, f.Vertices[1].Y, 0.0f);

            GL.Vertex3(f.Vertices[0].X, f.Vertices[0].Y, 0.0f);
            GL.Vertex3(f.Vertices[2].X, f.Vertices[2].Y, 0.0f);

            GL.Vertex3(f.Vertices[1].X, f.Vertices[1].Y, 0.0f);
            GL.Vertex3(f.Vertices[2].X, f.Vertices[2].Y, 0.0f);

        }

        private bool InBound(Vertex2 v)
        {
            if (v.X < -size || v.X > size) return false;
            if (v.Y < -size || v.Y > size) return false;

            return true;
        }

        private void DrawPoint(Vertex2 v)
        {
            float x = v.X;
            float y = v.Y;
            float s = 0.05f;

            GL.Vertex3(x + s, y + s, 0.0f);
            GL.Vertex3(x + s, y - s, 0.0f);
            GL.Vertex3(x - s, y - s, 0.0f);
            GL.Vertex3(x - s, y + s, 0.0f);
        }

        private void DrawCircle(Vertex2 v, float radius, int segments)
        {
            float ds = Mathf.PI * 2.0f / (float)segments;

            for (float i = -Mathf.PI; i < Mathf.PI; i += ds)
            {
                float dx0 = Mathf.Cos(i);
                float dy0 = Mathf.Sin(i);

                float x0 = v.X + dx0 * radius;
                float y0 = v.Y + dy0 * radius;

                float dx1 = Mathf.Cos(i + ds);
                float dy1 = Mathf.Sin(i + ds);

                float x1 = v.X + dx1 * radius;
                float y1 = v.Y + dy1 * radius;

                GL.Vertex3(x0, y0, 0.0f);
                GL.Vertex3(x1, y1, 0.0f);
            }

        }
    }
}
