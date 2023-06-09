using CardMachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class aStarPathfinding
{

    static Dictionary<Vector2Int, aStarNode> OpenNodes = new Dictionary<Vector2Int, aStarNode>();
    static List<Vector2Int> ClosedNodes = new List<Vector2Int>();

    static List<aStarNode> nodePath = new List<aStarNode>();
    static Vector2Int endPos;

    //the main algorithm, and how the pathfinding function is called;
    public static async void Pathfind(Vector2Int startPoint, Vector2Int endPoint)
    {
        DateTime before = DateTime.Now;


        OpenNodes.Clear();
        ClosedNodes.Clear();
        nodePath.Clear();

        endPos = endPoint;


        //spawning objects to represent the start and end of the path

        GameObject G = new GameObject("startPoint");
        G.transform.position = (Vector2)startPoint + new Vector2(0.5f, 0.5f);
        G.transform.position += new Vector3(0, 0, -3);
        SpriteRenderer S = G.AddComponent<SpriteRenderer>();
        S.sprite = Resources.Load<Sprite>("Base/Pathfind");
        S.color = Color.blue;

        GameObject G1 = new GameObject("endPoint");
        G1.transform.position = (Vector2)endPoint + new Vector2(0.5f, 0.5f);
        G1.transform.position += new Vector3(0, 0, -3);
        SpriteRenderer S1 = G1.AddComponent<SpriteRenderer>();
        S1.sprite = Resources.Load<Sprite>("Base/Pathfind");
        S1.color = Color.green;



        //current step
        int step = 1;

        //sets the first node at the start point
        aStarNode currentNode = new aStarNode(startPoint,0, Mathf.RoundToInt(Vector2Int.Distance(startPoint, endPos)),null);


        DateTime waitTime = DateTime.Now;


        for (int i = 0; i < step; i++)
        {

            //this is hacky crappy code to make it run over multiple frames, but it seems to work well enough

            TimeSpan waitDuration = DateTime.Now.Subtract(waitTime);

            if(waitDuration.Milliseconds > 20)
            {
                Debug.Log("Waiting Frame");

                await Task.Delay(1);
                waitTime = DateTime.Now;
            }


            //gets the next node in the series and increments step by 1

            currentNode = getNextNode(currentNode);

            step += 1;

            //ends the loop if the current node and endpoint are the same

            if (currentNode.pos == endPoint) { break;}


        }

        tracePath(currentNode);

        DateTime after = DateTime.Now;
        TimeSpan duration = after.Subtract(before);
        Debug.Log("Path Found in " + duration.Milliseconds + "ms with " + step + " steps");

    }

    //selects the best node from the open node list
    static aStarNode getNextNode(aStarNode lastNode)
    {
        getNodes(lastNode);

        OpenNodes.Remove(lastNode.pos);
        ClosedNodes.Add(lastNode.pos);


        List<aStarNode> values = Enumerable.ToList(OpenNodes.Values);

        aStarNode newNode = values[0];


        //the main logic for the a star algorithm
        foreach (aStarNode node in values)
        {
            if (node.F <= newNode.F)
            {
                if (node.H < newNode.H)
                {
                    newNode = node;
                }

            }
        }
        return newNode;

    }


    //gets the neighbor nodes for the last node
    static void getNodes(aStarNode node)
    {
        foreach (Vector2Int neighbour in neighbourPosCardinal)
        {
            Vector2Int newPos = node.pos + neighbour;

            if (!ClosedNodes.Contains(newPos) && !OpenNodes.ContainsKey(newPos))
            {

                //a custom tile class for my project, this can be replaced with any check for seeing if a tile is able to be moved across

                TileData tile = CMW.GetWorldTile(newPos);


                if (tile != null)
                {
                    if(tile.walkable == true)
                    {
                        int distfromEnd = Mathf.RoundToInt(Vector2Int.Distance(newPos, endPos));


                        aStarNode newNode = new aStarNode(newPos, node.G + 10, distfromEnd,node);


                        OpenNodes.Add(newPos,newNode);


                        //debug code for seeing every tile it searched

                    //    GameObject G = new GameObject(newNode.F.ToString());
                    //    G.transform.position = (Vector2)newNode.pos + new Vector2(0.5f,0.5f);
                    //    G.transform.position += new Vector3(0, 0, -1);
                    //    SpriteRenderer S = G.AddComponent<SpriteRenderer>();
                    //    S.sprite = Resources.Load<Sprite>("Base/Pathfind");
                    //    S.color= Color.red;

                    }
                }
            }
        }
        return;
    }


    //goes back from the end to the start tracing the path
    static void tracePath(aStarNode node)
    {


        GameObject G = new GameObject("path");
        G.transform.position = (Vector2)node.pos + new Vector2(0.5f, 0.5f);
        G.transform.position += new Vector3(0, 0, -3);
        SpriteRenderer S = G.AddComponent<SpriteRenderer>();
        S.sprite = Resources.Load<Sprite>("Base/Pathfind");
        S.color = Color.yellow;


        nodePath.Add(node);
        aStarNode nodeRoot = node.root;
        if(nodeRoot != null)
        {

            tracePath(nodeRoot);
        }
    }


    private static readonly Vector2Int[] neighbourPosCardinal =
    {
        Vector2Int.up,
        Vector2Int.right,
        Vector2Int.down,
        Vector2Int.left,
    };

    private static readonly Vector2Int[] neighbourPosDiagonal =
    {
        Vector2Int.up + Vector2Int.right,
        Vector2Int.up + Vector2Int.left,
        Vector2Int.down + Vector2Int.right,
        Vector2Int.down + Vector2Int.left
    };
}


//a class that stores the data for each node
public class aStarNode
{
    public aStarNode root; //the parent node that this node was created from
    public Vector2Int pos; //position this node is at
    public int G; //distance from start
    public int H; //distance from end
    public int F;

    public aStarNode(Vector2Int Pos, int g, int h, aStarNode Root)
    {
        pos = Pos;
        G = g;
        H = h;
        F = g + h;
        root = Root;
    }
}
