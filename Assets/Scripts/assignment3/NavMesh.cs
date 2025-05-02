using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NavMesh : MonoBehaviour
{
    public Graph MakeNavMesh(List<Wall> outline)
    {
        Graph g = new Graph();
        g.all_nodes = new List<GraphNode>();

        // Convert walls to point list
        List<Vector3> vertices = new List<Vector3>();
        foreach (Wall w in outline)
            vertices.Add(w.start);

        //Recursively split into convex polygons
        List<List<Vector3>> convexPolys = TriangulateRecursive(vertices, outline);

        //Turn each polygon into GraphNode
        int id = 0;
        foreach (var polyVerts in convexPolys)
        {
            List<Wall> polyWalls = new List<Wall>();
            for (int i = 0; i < polyVerts.Count; i++)
            {
                polyWalls.Add(new Wall(polyVerts[i], polyVerts[(i + 1) % polyVerts.Count]));
            }

            g.all_nodes.Add(new GraphNode(id++, polyWalls));
        }

        //  Add neighbors 
        for (int i = 0; i < g.all_nodes.Count; i++)
        {
            for (int j = i + 1; j < g.all_nodes.Count; j++)
            {
                var a = g.all_nodes[i];
                var b = g.all_nodes[j];
                var polyA = a.GetPolygon();
                var polyB = b.GetPolygon();

                for (int edgeA = 0; edgeA < polyA.Count; edgeA++)
                {
                    for (int edgeB = 0; edgeB < polyB.Count; edgeB++)
                    {
                        if (polyA[edgeA].Same(polyB[edgeB]))
                        {
                            bool isOriginalWall = outline.Any(w => w.Same(polyA[edgeA]));
                            if (isOriginalWall)
                                continue;

                            a.AddNeighbor(b, edgeA);
                            b.AddNeighbor(a, edgeB);
                        }
                    }
                }
            }
        }

        g.outline = outline;
        return g;
    }

    bool IsReflex(Vector3 prev, Vector3 curr, Vector3 next)
    {
        Vector3 a = (curr - prev).normalized;
        Vector3 b = (next - curr).normalized;
        float cross = a.x * b.z - a.z * b.x;
        return cross < 0;
    }

    bool IsVisible(Vector3 from, Vector3 to, List<Vector3> polygon)
    {
        Wall testWall = new Wall(from, to);

        for (int i = 0; i < polygon.Count; i++)
        {
            Vector3 a = polygon[i];
            Vector3 b = polygon[(i + 1) % polygon.Count];

            // Don't check edges that share vertices
            if ((a == from || b == from || a == to || b == to)) continue;

            Wall edge = new Wall(a, b);
            if (testWall.Crosses(edge)) return false;
        }
        return true;
    }

    bool IsPointInsidePolygon(Vector3 point, List<Vector3> polygon)
    {
        int count = 0;
        for (int i = 0; i < polygon.Count; i++)
        {
            Vector3 a = polygon[i];
            Vector3 b = polygon[(i + 1) % polygon.Count];

            if (((a.z > point.z) != (b.z > point.z)) &&
                (point.x < (b.x - a.x) * (point.z - a.z) / (b.z - a.z + 0.0001f) + a.x))
            {
                count++;
            }
        }
        return count % 2 == 1; 
    }

    List<List<Vector3>> TriangulateRecursive(List<Vector3> polygon, List<Wall> original)
    {
        List<List<Vector3>> result = new List<List<Vector3>>();
        TriangulateRecursiveHelper(polygon, original, result);
        return result;
    }

    void TriangulateRecursiveHelper(List<Vector3> polygon, List<Wall> original, List<List<Vector3>> output)
    {
        for (int i = 0; i < polygon.Count; i++)
        {
            Vector3 prev = polygon[(i - 1 + polygon.Count) % polygon.Count];
            Vector3 curr = polygon[i];
            Vector3 next = polygon[(i + 1) % polygon.Count];

            if (IsReflex(prev, curr, next))
            {
                for (int j = 0; j < polygon.Count; j++)
                {
                    if (j == i || j == (i - 1 + polygon.Count) % polygon.Count || j == (i + 1) % polygon.Count)
                        continue;

                    if (IsVisible(curr, polygon[j], polygon))
                    {
                        Vector3 midpoint = (curr + polygon[j]) / 2f;
                        if (!IsPointInsidePolygon(midpoint, polygon))
                            continue;

                        List<Vector3> poly1 = new List<Vector3>();
                        List<Vector3> poly2 = new List<Vector3>();

                        int a = i;
                        while (a != j)
                        {
                            poly1.Add(polygon[a]);
                            a = (a + 1) % polygon.Count;
                        }
                        poly1.Add(polygon[j]);

                        int b = j;
                        while (b != i)
                        {
                            poly2.Add(polygon[b]);
                            b = (b + 1) % polygon.Count;
                        }
                        poly2.Add(polygon[i]);

                        TriangulateRecursiveHelper(poly1, original, output);
                        TriangulateRecursiveHelper(poly2, original, output);
                        return;
                    }
                }
            }
        }

        // If no reflex found, add polygon as convex
        output.Add(polygon);
    }

    List<Wall> outline;

    void Start()
    {
        EventBus.OnSetMap += SetMap;
    }

    void Update()
    {
    }

    public void SetMap(List<Wall> outline)
    {
        Graph navmesh = MakeNavMesh(outline);
        if (navmesh != null)
        {
            Debug.Log("got navmesh: " + navmesh.all_nodes.Count);
            EventBus.SetGraph(navmesh);
        }
    }
}
