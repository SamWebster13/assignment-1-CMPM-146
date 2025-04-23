using UnityEngine;
using System.Collections.Generic;

public class PathFinder : MonoBehaviour
{
    // Assignment 2: Implement AStar
    //
    // DO NOT CHANGE THIS SIGNATURE (parameter types + return type)
    // AStar will be given the start node, destination node and the target position, and should return 
    // a path as a list of positions the agent has to traverse to reach its destination, as well as the
    // number of nodes that were expanded to find this path
    // The last entry of the path will be the target position, and you can also use it to calculate the heuristic
    // value of nodes you add to your search frontier; the number of expanded nodes tells us if your search was
    // efficient
    //
    // Take a look at StandaloneTests.cs for some test cases
    // public static (List<Vector3>, int) AStar(GraphNode start, GraphNode destination, Vector3 target)
    // {
    //     // Implement A* here
    //     List<Vector3> path = new List<Vector3>() { target };

    //     // return path and number of nodes expanded
    //     return (path, 0);

    // }
    
    public static (List<Vector3>, int) AStar(GraphNode start, GraphNode destination, Vector3 target)
{
    var frontier = new List<GraphNode> { start };
    var cameFrom = new Dictionary<GraphNode, (GraphNode, Wall)>();
    var costSoFar = new Dictionary<GraphNode, float> { [start] = 0f };
    int nodesExpanded = 0;

    while (frontier.Count > 0)
    {
        // Find the node with the lowest estimated cost (g + h)
        GraphNode current = frontier[0];
        float bestCost = costSoFar[current] + Vector3.Distance(current.GetCenter(), target);
        foreach (var node in frontier)
        {
            float cost = costSoFar[node] + Vector3.Distance(node.GetCenter(), target);
            if (cost < bestCost)
            {
                bestCost = cost;
                current = node;
            }
        }

        frontier.Remove(current);
        nodesExpanded++;

        if (current == destination)
        {
            // Reconstruct path using wall midpoints
            List<Vector3> path = new List<Vector3>();
            GraphNode n = current;

            while (cameFrom.ContainsKey(n))
            {
                var (prevNode, wall) = cameFrom[n];
                path.Insert(0, wall.midpoint);
                n = prevNode;
            }

            path.Add(target); // Add final destination point
            return (path, nodesExpanded);
        }

        foreach (GraphNeighbor neighbor in current.GetNeighbors())
        {
            GraphNode next = neighbor.GetNode();
            float newCost = costSoFar[current] + Vector3.Distance(current.GetCenter(), next.GetCenter());

            if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
            {
                costSoFar[next] = newCost;
                cameFrom[next] = (current, neighbor.GetWall());

                if (!frontier.Contains(next))
                {
                    frontier.Add(next);
                }
            }
        }
    }

    // No path found
    return (new List<Vector3>(), nodesExpanded);
}

    public Graph graph;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        EventBus.OnTarget += PathFind;
        EventBus.OnSetGraph += SetGraph;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetGraph(Graph g)
    {
        graph = g;
    }

    // entry point
    public void PathFind(Vector3 target)
    {
        if (graph == null) return;

        // find start and destination nodes in graph
        GraphNode start = null;
        GraphNode destination = null;
        foreach (var n in graph.all_nodes)
        {
            if (Util.PointInPolygon(transform.position, n.GetPolygon()))
            {
                start = n;
            }
            if (Util.PointInPolygon(target, n.GetPolygon()))
            {
                destination = n;
            }
        }
        if (destination != null)
        {
            // only find path if destination is inside graph
            EventBus.ShowTarget(target);
            (List<Vector3> path, int expanded) = PathFinder.AStar(start, destination, target);

            Debug.Log("found path of length " + path.Count + " expanded " + expanded + " nodes, out of: " + graph.all_nodes.Count);
            EventBus.SetPath(path);
        }
        

    }

    

 
}
