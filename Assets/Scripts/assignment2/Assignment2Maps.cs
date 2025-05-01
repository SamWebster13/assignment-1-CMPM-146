using UnityEngine;
using System.Collections.Generic;

public class Assignment2Maps : MapCollection
{
    public void StudentMaps(int which)
    {
        
        /* you can create your own test cases:
              create an outline (list of vertices); coordinates should range from ~-100 to +100; it's helpful to draw this on paper
              define individual nodes, where each node is associated with a polygon; together all polygons should cover the outline
                the polygon vertices should be listed in counterclockwise order
              AddNode can add a polygon to the list of nodes
              FindNeighbors will then connect adjacent nodes as neighbors
           Call EventBus.SetMap **and** EventBus.SetGraph afterwards (in assignment 3, we will skip the SetGraph call)
        
           you can take a look at (which == 2) below to see an example. For example, the first two nodes are:

               AddNode(nodes, new float[] { -20, 0, 20,
                                           0, 0,-20,
                                          20, 0, 20});

              AddNode(nodes, new float[] { -20, 0, 20,
                                          20, 0, 20,
                                           10, 0,50});

            these are both triangles, where last edge of the first triangle goes from (20,0,20) to (-20,0,20), and the first edge of the second triangle
               goes from (-20,0,20) to (20,0,20); this is the same edge (in opposite directions, because each is traversed counterclockwise), and 
               therefore FindNeighbors will connect these two nodes as neighbors.
        */
        // make your own graph here

            // Don't forget to call:
            // EventBus.SetMap(g.outline);
            // EventBus.SetGraph(g);
        
        if (which == 3)
        {
            Graph g = new Graph();
            List<GraphNode> nodes = new List<GraphNode>();

            // Top-left quadrant
            AddNode(nodes, new float[] {
                -40, 0, 40,
                0, 0, 40,
                0, 0, 0,
                -40, 0, 0
            });

            // Top-right quadrant
            AddNode(nodes, new float[] {
                0, 0, 40,
                40, 0, 40,
                40, 0, 0,
                0, 0, 0
            });

            // Bottom-right quadrant
            AddNode(nodes, new float[] {
                0, 0, 0,
                40, 0, 0,
                40, 0, -40,
                0, 0, -40
            });

            // Bottom-left quadrant
            AddNode(nodes, new float[] {
                -40, 0, 0,
                0, 0, 0,
                0, 0, -40,
                -40, 0, -40
            });

            FindNeighbors(nodes);

            List<Wall> outer_walls = FindOuterWalls(nodes);
            g.outline = GraphGenerator.GetOutline(outer_walls);
            g.all_nodes = nodes;
            EventBus.SetMap(g.outline);
            EventBus.SetGraph(g);
            return;
        }

        //octogon maze---------------------------------------
        void MakeOctagon(List<GraphNode> nodes, Vector3 center, float radius, int sides)
        {
            float angle_offset = 360f / (2f * sides); // Half-angle offset to orient nicely
            List<float> verts = new List<float>();
            for (int i = 0; i < sides; i++)
            {
                float angle = angle_offset + i * (360f / sides);
                float rad = Mathf.Deg2Rad * angle;
                float x = center.x + radius * Mathf.Cos(rad);
                float z = center.z + radius * Mathf.Sin(rad);
                verts.Add(x); verts.Add(center.y); verts.Add(z);
            }
            AddNode(nodes, verts.ToArray());
        }



        if (which == 4)
        {
            Graph g = new Graph();
            List<GraphNode> nodes = new List<GraphNode>();

            float radius = 15f;
            int sides = 8; // Octagon

            Vector3 c1 = new Vector3(-10, 0, 0);       // Left octagon
            Vector3 c2 = new Vector3(15, 0, 0);        // Right octagon
            Vector3 c3 = new Vector3(2.5f, 0, 13);     // Top (yellow seam)

            // Add octagon nodes to list
            MakeOctagon(nodes, c1, radius, sides); // node 0
            MakeOctagon(nodes, c2, radius, sides); // node 1
            MakeOctagon(nodes, c3, radius, sides); // node 2

            GraphNode n1 = nodes[0];
            GraphNode n2 = nodes[1];
            GraphNode n3 = nodes[2];

            // Manually connect them (adjusted sides for octagons: 0–7)
            n1.AddNeighbor(n2, 0);
            n2.AddNeighbor(n1, 4);

            n1.AddNeighbor(n3, 1);
            n3.AddNeighbor(n1, 5);

            n2.AddNeighbor(n3, 2);
            n3.AddNeighbor(n2, 6);

            List<Wall> outline = OutlineFromArray(new float[] {
                60, 0, 60,
                -60, 0, 60,
                -60, 0, -60,
                60, 0, -60
            });

            g.outline = outline;
            g.all_nodes = nodes;
            EventBus.SetMap(g.outline);
            EventBus.SetGraph(g);
            return;
        }

        if (which == 5)
        {
            Graph g = new Graph();
            List<GraphNode> nodes = new List<GraphNode>();

            float radius = 10f;
            float spacing = radius * 2f; // spacing between centers
            int sides = 8;
            int cols = 6;
            int rows = 6;

            GraphNode[,] grid = new GraphNode[rows, cols];

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    float x = col * spacing - (cols * spacing / 2f);
                    float z = row * spacing - (rows * spacing / 2f);
                    Vector3 center = new Vector3(x, 0, z);

                    MakeOctagon(nodes, center, radius, sides);
                    GraphNode node = nodes[nodes.Count - 1]; // get the most recent node
                    grid[row, col] = node;
                    nodes.Add(node);
                }
            }
         

            // Connect orthogonal neighbors (up, down, left, right)
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    GraphNode node = grid[row, col];
                    if (node == null) continue;

                    int[,] directions = new int[,] {
                        { -1,  0, 0, 4 }, // up
                        {  1,  0, 4, 0 }, // down
                        {  0, -1, 6, 2 }, // left
                        {  0,  1, 2, 6 }  // right
                    };

                    for (int i = 0; i < 4; i++)
                    {
                        int nRow = row + directions[i, 0];
                        int nCol = col + directions[i, 1];
                        int side = directions[i, 2];
                        int reverse = directions[i, 3];

                        if (nRow >= 0 && nRow < rows && nCol >= 0 && nCol < cols)
                        {
                            GraphNode neighbor = grid[nRow, nCol];
                            if (neighbor != null)
                            {
                                node.AddNeighbor(neighbor, side);
                                neighbor.AddNeighbor(node, reverse);
                            }
                        }
                    }
                }
            }

            List<Wall> outline = OutlineFromArray(new float[] {
                80, 0, 80,
            -80, 0, 80,
            -80, 0, -80,
                80, 0, -80
            });

            g.outline = outline;
            g.all_nodes = nodes;
            EventBus.SetMap(g.outline);
            EventBus.SetGraph(g);
            return;
        }


        //Hexagons-------------------------------------
        // .. maps 3-7 are available for your own test cases
        List<Wall> MakeHex(Vector3 center, float radius)
        {
            List<Vector3> verts = new List<Vector3>();
            for (int i = 0; i < 6; i++)
            {
                float angle_deg = 60 * i - 30;
                float angle_rad = Mathf.Deg2Rad * angle_deg;
                float x = center.x + radius * Mathf.Cos(angle_rad);
                float z = center.z + radius * Mathf.Sin(angle_rad);
                verts.Add(new Vector3(x, 0, z));
            }

            List<Wall> walls = new List<Wall>();
            for (int i = 0; i < 6; i++)
            {
                walls.Add(new Wall(verts[i], verts[(i + 1) % 6]));
            }
            return walls;
        }

        if (which == 6)
        {
            Graph g = new Graph();
            List<GraphNode> nodes = new List<GraphNode>();

            float radius = 15f;
            Vector3 c1 = new Vector3(-10, 0, 0);
            Vector3 c2 = new Vector3(15, 0, 0);
            Vector3 c3 = new Vector3(2.5f, 0, 13); // Overlapping top yellow

            // First hex (left)
            GraphNode n1 = new GraphNode(0, MakeHex(c1, radius));
            // Second hex (right)
            GraphNode n2 = new GraphNode(1, MakeHex(c2, radius));
            // Overlapping hex (top yellow-ish)
            GraphNode n3 = new GraphNode(2, MakeHex(c3, radius));

            // Manually connect them
            n1.AddNeighbor(n2, 0); // connect n1 to n2
            n2.AddNeighbor(n1, 3); // reverse connection

            n1.AddNeighbor(n3, 1);
            n3.AddNeighbor(n1, 4);

            n2.AddNeighbor(n3, 2);
            n3.AddNeighbor(n2, 5);

            nodes.Add(n1);
            nodes.Add(n2);
            nodes.Add(n3);

            List<Wall> outline = OutlineFromArray(new float[] {
                60, 0, 60,
            -60, 0, 60,
            -60, 0, -60,
                60, 0, -60
            });

            g.outline = outline;
            g.all_nodes = nodes;
            EventBus.SetMap(g.outline);
            EventBus.SetGraph(g);
            return;
        }

        if (which == 7)
        {
            Graph g = new Graph();
            List<GraphNode> nodes = new List<GraphNode>();

            float radius = 10f;
            float hexWidth = radius * Mathf.Sqrt(3);
            float hexHeight = radius * 1.5f;

            int cols = 7;  // width of maze
            int rows = 7;  // height of maze

            GraphNode[,] grid = new GraphNode[rows, cols];

            int id = 0;
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    float xOffset = (row % 2 == 0) ? 0 : hexWidth / 2;
                    float x = col * hexWidth + xOffset - (cols * hexWidth / 2);
                    float z = row * hexHeight - (rows * hexHeight / 2);

                    Vector3 center = new Vector3(x, 0, z);
                    List<Wall> hexWalls = MakeHex(center, radius);
                    GraphNode node = new GraphNode(id++, hexWalls);
                    grid[row, col] = node;
                    nodes.Add(node);
                }
            }

            // Connect neighbors (up to 6 for each hex)
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    GraphNode node = grid[row, col];
                    if (node == null) continue;

                    // axial neighbor logic for offset rows
                    int[,] evenOffsets = new int[,] {
                        { -1,  0 }, { -1, -1 }, { 0, -1 },
                        { 1,  0 }, { 1, -1 }, { 0, 1 }
                    };

                    int[,] oddOffsets = new int[,] {
                        { -1,  1 }, { -1, 0 }, { 0, -1 },
                        { 1,  1 }, { 1, 0 }, { 0, 1 }
                    };

                    int[,] offsets = (row % 2 == 0) ? evenOffsets : oddOffsets;

                    for (int i = 0; i < 6; i++)
                    {
                        int nRow = row + offsets[i, 0];
                        int nCol = col + offsets[i, 1];

                        if (nRow >= 0 && nRow < rows && nCol >= 0 && nCol < cols)
                        {
                            GraphNode neighbor = grid[nRow, nCol];
                            if (neighbor != null)
                            {
                                node.AddNeighbor(neighbor, i);
                            }
                        }
                    }
                }
            }

            List<Wall> outline = OutlineFromArray(new float[] {
                80, 0, 80,
            -80, 0, 80,
            -80, 0, -80,
                80, 0, -80
            });

            g.outline = outline;
            g.all_nodes = nodes;
            EventBus.SetMap(g.outline);
            EventBus.SetGraph(g);
            return;
        }



    }


    public override void Generate(int which)
    {
        if (which == 0)
        {
            List<Wall> outline = OutlineFromArray(new float[] {
                   50, 0, 50,
                   -50, 0, 50,
                   -50, 0, -50,
                   50, 0, -50
                });

            GraphNode n = new GraphNode(0, outline);
            Graph g = new Graph();
            g.outline = outline;
            g.all_nodes = new List<GraphNode>() { n };
            EventBus.SetMap(outline);
            EventBus.SetGraph(g);
            return;
        }
        if (which == 1)
        {
            List<Wall> outline = OutlineFromArray(new float[] {
                   50, 0, 50,
                   -50, 0, 50,
                   -50, 0, -30,
                   -25, 0, -30,
                   -25, 0, -50,
                   50, 0, -50
                });


            GraphNode n = new GraphNode(0, new List<Wall> { outline[0], outline[1], outline[2], new Wall(new Vector3(-25, 0, -30), new Vector3(50, 0, 50)) });
            GraphNode n1 = new GraphNode(0, new List<Wall> { new Wall(new Vector3(50, 0, 50), new Vector3(-25, 0, -30)), outline[3], outline[4], outline[5] });
            n.AddNeighbor(n1, 3);
            n1.AddNeighbor(n, 0);
            Graph g = new Graph();
            g.outline = outline;
            g.all_nodes = new List<GraphNode>() { n, n1 };
            EventBus.SetMap(outline);
            EventBus.SetGraph(g);
            return;
        }

        if (which == 2)
        {
            Graph g = new Graph();
            List<GraphNode> nodes = new List<GraphNode>();

            AddNode(nodes, new float[] { -20, 0, 20,
                                           0, 0,-20,
                                          20, 0, 20});

            AddNode(nodes, new float[] { -20, 0, 20,
                                          20, 0, 20,
                                           10, 0,50});

            AddNode(nodes, new float[] {-30, 0, 26,
                                        -40, 0, 4,
                                        -20, 0, -40,
                                          0, 0, -20,
                                        -20, 0, 20
                                         });
 
            AddNode(nodes, new float[] {-20, 0, -40,
                                        0, 0, -70,
                                        30, 0, -50,
                                        30, 0, -30,
                                         0, 0, -20
                                         });

            AddNode(nodes, new float[] { -40, 0, 4,
                                          -60, 0, -16,
                                           -20, 0, -40});


            FindNeighbors(nodes);

            List<Wall> outer_walls = FindOuterWalls(nodes);
            g.outline = GraphGenerator.GetOutline(outer_walls);
            g.all_nodes = nodes;
            EventBus.SetMap(g.outline);
            EventBus.SetGraph(g);
            return;
        }

        
        if (which == 8)
        {
            Graph g = GraphGenerator.Generate(false);
            EventBus.SetMap(g.outline);
            EventBus.SetGraph(g);
            return;
        }

        if (which == 9)
        {
            Graph g = GraphGenerator.AdditiveGenerate(false);
            EventBus.SetMap(g.outline);
            EventBus.SetGraph(g);
            return;
        }

        StudentMaps(which);
    }
}
