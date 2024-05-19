using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HullDelaunayVoronoi.Delaunay;
using HullDelaunayVoronoi.Primitives;
using HullDelaunayVoronoi.Voronoi;
using HullDelaunayVoronoi.Hull;

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

        public float speed = 1;
        public bool showDelaunay;
        public bool showVoronoi;
        public bool showNormals;

        // Start is called before the first frame update
        void Start()
        {
            lineMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));

            Random.InitState(seed);
            for (float i = 0; i < NumberOfVertices; i++)
            {
                float x = Random.Range(-1.0f, 1.0f);
                float y = size * Random.Range(-1f, 1f);

                //float x = ((i / (float)NumberOfVertices) * 2f - 1f);
                //float y = size * Mathf.Pow(Mathf.Pow(Mathf.Cos(Mathf.Exp(x)), 2) - Mathf.Pow(x, 4), 1f/3f);
                //float y = x;
                //Debug.Log(x + ", " + y);


                vertices.Add(new Vertex2(size * x, y));
                OGvertices.Add(new Vertex2(size * x, y));
            }

            delaunay = new DelaunayTriangulation2();
            delaunay.Generate(vertices);

            //foreach(DelaunayCell<Vertex2> cell in delaunay.Cells)
            //{
            //    Debug.Log(" ");
            //    foreach (Vertex2 v in cell.Simplex.Vertices)
            //    {
            //        Debug.Log(v.X + ", " + v.Y);
            //    }
            //}

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
            for (int i = 0; i < vertices.Count; i++)
            {
                offsets.Add(Random.Range(1.0f, 360.0f));
            }
            while (true)
            {
                t += Time.deltaTime * speed;
                //foreach (Vertex2 v in vertices)
                //{
                //    v.X += 0.01f;
                //    v.Y = 0.1f;
                //}

                for (int i = 0; i < vertices.Count; i++)
                {
                    Vertex2 v = vertices[i];
                    Vertex2 ogV = OGvertices[i];
                    float offset = offsets[i];

                    //v.X = ogV.X + (Mathf.Repeat(t, 100)/100f * 2f -1f);

                    //float x = v.X/size;
                    //v.Y = size * (Mathf.Pow(Mathf.Pow(Mathf.Cos(Mathf.Exp(x)), 2) - Mathf.Pow(x, 4), 1f / 3f));

                    v.X = ogV.X + Mathf.Cos(t + offset);
                    v.Y -= 0.01f * speed;
                    if (v.Y < -size)
                    {
                        v.Y = size;
                    }
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

            lineMaterial.SetPass(0);

            GL.Begin(GL.LINES);

            GL.Color(Color.red);
            if (showDelaunay)
            {
                foreach (DelaunayCell<Vertex2> cell in delaunay.Cells)
                {
                    DrawSimplex(cell.Simplex);
                }
            }

            //GL.Color(Color.green);

            //foreach (DelaunayCell<Vertex2> cell in delaunay.Cells)
            //{
            //    DrawCircle(cell.CircumCenter, cell.Radius, 32);
            //}

            if (showNormals)
            {
                GL.Color(Color.magenta);

                foreach (DelaunayCell<Vertex2> cell in delaunay.Cells)
                {
                    DrawSimplexNormalLines(cell.Simplex);
                }
            }

            GL.Color(Color.cyan);
            foreach (DelaunayCell<Vertex2> cell in delaunay.Cells)
            {
                DrawCellVectors(cell);
            }

            GL.End();

            // DRAW VORONOI LINES
            lineMaterial.SetPass(0);
            GL.Begin(GL.LINES);

            GL.Color(Color.green);

            if (showVoronoi)
            {
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
            }

            GL.End();

            // DRAW POINTS
            GL.Begin(GL.QUADS);
            GL.Color(Color.white);

            foreach (Vertex2 v in vertices)
            {
                DrawPoint(v);
            }

            GL.End();

            GL.Begin(GL.QUADS);
            GL.Color(Color.blue);
            if (showVoronoi)
            {
                foreach (DelaunayCell<Vertex2> cell in delaunay.Cells)
                {
                    DrawPoint(cell.CircumCenter);
                }
            }

            GL.End();

            GL.Begin(GL.QUADS);
            if (showDelaunay)
            {
                GL.Color(Color.yellow);

                foreach (DelaunayCell<Vertex2> cell in delaunay.Cells)
                {
                    Simplex<Vertex2> s = cell.Simplex;
                    Vertex2 v1 = new Vertex2((s.Vertices[0].X + s.Vertices[1].X) / 2f, (s.Vertices[0].Y + s.Vertices[1].Y) / 2f);
                    Vertex2 v2 = new Vertex2((s.Vertices[0].X + s.Vertices[2].X) / 2f, (s.Vertices[0].Y + s.Vertices[2].Y) / 2f);
                    Vertex2 v3 = new Vertex2((s.Vertices[1].X + s.Vertices[2].X) / 2f, (s.Vertices[1].Y + s.Vertices[2].Y) / 2f);

                    DrawPoint(v1);
                    DrawPoint(v2);
                    DrawPoint(v3);
                }
            }

            GL.End();

            GL.PopMatrix();
        }

        private void DrawCellVectors(DelaunayCell<Vertex2> cell)
        {
            Vertex2 p1 = cell.Simplex.Vertices[0];
            Vertex2 p2 = cell.Simplex.Vertices[1];
            Vertex2 p3 = cell.Simplex.Vertices[2];

            // Midpoints of simplex lines
            Vertex2 m1 = new Vertex2((p1.X + p2.X) / 2f, (p1.Y + p2.Y) / 2f);
            Vertex2 m2 = new Vertex2((p1.X + p3.X) / 2f, (p1.Y + p3.Y) / 2f);
            Vertex2 m3 = new Vertex2((p2.X + p3.X) / 2f, (p2.Y + p3.Y) / 2f);

            Vertex2 circumCenter = cell.CircumCenter;

            Vector2 v1 = new Vector2(m1.X - circumCenter.X, m1.Y - circumCenter.Y);
            Vector2 v2 = new Vector2(m2.X - circumCenter.X, m2.Y - circumCenter.Y);
            Vector2 v3 = new Vector2(m3.X - circumCenter.X, m3.Y - circumCenter.Y);

            //DrawLine(m1, circumCenter);
            //DrawLine(m2, circumCenter);
            //DrawLine(m3, circumCenter);

            List<Vector2> circumCenterVectors = new List<Vector2>();
            foreach (VoronoiRegion<Vertex2> region in voronoi.Regions)
            {
                foreach (VoronoiEdge<Vertex2> edge in region.Edges)
                {
                    Vertex2 from = edge.From.CircumCenter;
                    Vertex2 to = edge.To.CircumCenter;

                    //DrawLine(from, to);

                    Vector2 circumCenterVector = new Vector2(to.X - from.X, to.Y - from.Y);
                    circumCenterVectors.Add(circumCenterVector);
                }
            }

            List<Vector2> cellVectors = new List<Vector2>();
            cellVectors.Add(v2);
            cellVectors.Add(v1);
            cellVectors.Add(v3);

            List<Vector2> extendVectors = new List<Vector2>();
            List<Vector2> adjacentCenterVectors = new List<Vector2>();

            Vector2 vectorToExtend = cellVectors[2];
            


                for (int i = 0; i < cellVectors.Count; i++)
                {
                bool correctVector = true;
                Vector2 vec2 = cellVectors[i];
                foreach (Vector2 v in circumCenterVectors)
                {
                    float crossZ = vec2.x * v.y - vec2.y * v.x;
                    crossZ *= -1f;
                    if (Mathf.Abs(crossZ) < 0.0001f)
                    {
                        adjacentCenterVectors.Add(v);
                        correctVector = false;
                    }
                }
                if (correctVector)
                {
                    extendVectors.Add(vec2);
                }
                }


            //Debug.Log(adjacentCenterVectors.Count);

            bool isoutofSimplex = isCircumCenterInCellSimplex(cell);
            foreach (Vector2 v in extendVectors)
            {
                Vertex2[] potentialLineVertices = CalculateVerticesFromVector2(circumCenter, v);

                bool goodVector;

                Debug.Log("Pos: " + CalcMinAngle(adjacentCenterVectors, v) + ", Neg: " + CalcMinAngle(adjacentCenterVectors, -1f * v));
                if(CalcMinAngle(adjacentCenterVectors, v) > CalcMinAngle(adjacentCenterVectors, -1f * v))
                {
                    goodVector = true;
                } else
                {
                    goodVector=false;
                }

                Debug.Log(goodVector && isoutofSimplex);

                if (isoutofSimplex)
                {
                    //Debug.Log("CircumCenter out Simplex");
                    if (goodVector)
                    {
                        DrawLine(circumCenter, potentialLineVertices[0]);
                    }
                    else if(!goodVector)
                    {
                        DrawLine(circumCenter, potentialLineVertices[1]);
                    }
                }
                else
                {
                    //Debug.Log("CircumCenter in Simplex");
                    DrawLine(circumCenter, potentialLineVertices[0]);
                }
            }
        }

        private float CalcMinAngle(List<Vector2> compareList, Vector2 v)
        {
            float minAngle = 360f;
            foreach (Vector2 av in compareList)
            {
                if (Vector3.Angle(av.normalized, v.normalized) < minAngle)
                {
                    minAngle = Vector3.Angle(av.normalized, v.normalized);
                }
            }
            Debug.Log(minAngle);
            return minAngle;
        }

        private bool isCircumCenterInCellSimplex(DelaunayCell<Vertex2> cell)
        {
            Vertex2 p1 = cell.Simplex.Vertices[0];
            Vertex2 p2 = cell.Simplex.Vertices[1];
            Vertex2 p3 = cell.Simplex.Vertices[2];

            // Midpoints of simplex lines (COMPONENTS ARE NOT DIVIDED BY 2)
            Vertex2 m1 = new Vertex2((p1.X + p2.X), (p1.Y + p2.Y));
            Vertex2 m2 = new Vertex2((p1.X + p3.X), (p1.Y + p3.Y));
            Vertex2 m3 = new Vertex2((p2.X + p3.X), (p2.Y + p3.Y));

            Vertex2 circumCenter = new Vertex2(cell.CircumCenter.X * 2f, cell.CircumCenter.Y * 2f);

            Vertex2[] comparePoints = { m1, m2, m3, circumCenter };

            Vertex2 maxX = comparePoints[0];
            Vertex2 maxY = comparePoints[0];
            Vertex2 minX = comparePoints[0];
            Vertex2 minY = comparePoints[0];

            for (int i = 0; i < comparePoints.Length; i++)
            {
                if (comparePoints[i].X > maxX.X)
                {
                    maxX = comparePoints[i];
                }

                if (comparePoints[i].Y > maxY.Y)
                {
                    maxY = comparePoints[i];
                }

                if(comparePoints[i].X < minX.X)
                {
                    minX = comparePoints[i];
                }

                if (comparePoints[i].Y < minY.Y)
                {
                    minY = comparePoints[i];
                }
            }

            return (maxX == circumCenter) || (maxY == circumCenter) || (minX == circumCenter) || (minY == circumCenter);
        }

        private void DrawSimplexNormalLines(Simplex<Vertex2> s)
        {
            Vertex2 v1 = s.Vertices[0];
            Vertex2 v2 = s.Vertices[1];
            Vertex2 v3 = s.Vertices[2];

            Vertex2 m1 = new Vertex2((v1.X + v2.X) / 2f, (v1.Y + v2.Y) / 2f);
            Vertex2 m2 = new Vertex2((v1.X + v3.X) / 2f, (v1.Y + v3.Y) / 2f);
            Vertex2 m3 = new Vertex2((v2.X + v3.X) / 2f, (v2.Y + v3.Y) / 2f);

            Vector2 normalVector1 = Vector2.Perpendicular(new Vector2(v1.X - v2.X, v1.Y - v2.Y));
            Vector2 normalVector2 = Vector2.Perpendicular(new Vector2(v1.X - v3.X, v1.Y - v3.Y));
            Vector2 normalVector3 = Vector2.Perpendicular(new Vector2(v2.X - v3.X, v2.Y - v3.Y));

            Vertex2[] normalVertices1 = CalculateVerticesFromVector2(m1, normalVector1);
            Vertex2[] normalVertices2 = CalculateVerticesFromVector2(m2, normalVector2);
            Vertex2[] normalVertices3 = CalculateVerticesFromVector2(m3, normalVector3);

            DrawLine(normalVertices1[0], normalVertices1[1]);
            DrawLine(normalVertices2[0], normalVertices2[1]);
            DrawLine(normalVertices3[0], normalVertices3[1]);
        }

        private Vertex2[] CalculateVerticesFromVector2(Vertex2 initialPos, Vector2 dir)
        {
            Vector2 vectorPos1 = new Vector2(initialPos.X, initialPos.Y) + dir.normalized * 100f;
            Vector2 vectorPos2 = new Vector2(initialPos.X, initialPos.Y) + dir.normalized * -100f;

            //foreach(DelaunayCell<Vertex2> cell in delaunay.Cells)
            //{
            //    Vertex2 circumCenter = cell.CircumCenter;

            //    Vector2 testVector = new Vector2(circumCenter.X - initialPos.X, circumCenter.Y - initialPos.X);

            //   // DrawLine(circumCenter, initialPos);

            //    if(Vector3.Magnitude(Vector3.Cross(testVector.normalized, dir.normalized)) == 0)
            //    {
            //        Debug.Log("Found CircumCenter");
            //        vectorPos1 = new Vector2(circumCenter.X, circumCenter.Y);
            //    }

            //    if(Vector3.Magnitude(Vector3.Cross(testVector.normalized, dir.normalized* -1f)) == 0)
            //    {
            //        Debug.Log("Found CircumCenter");
            //        vectorPos2 = new Vector2(circumCenter.X, circumCenter.Y);
            //    }
            //}
            Vertex2[] ans = { new Vertex2(vectorPos1.x, vectorPos1.y), new Vertex2(vectorPos2.x, vectorPos2.y) };
            return ans;
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
