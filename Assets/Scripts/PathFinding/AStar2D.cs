using UnityEngine;
using System.Collections.Generic;

public class AStar2D : MonoBehaviour
{
    public Transform player; // Pelaajan sijainti
    public float speed = 3f; // Vihollisen nopeus
    public LayerMask obstacleLayer; // Esteiden layer
    public float updatePathInterval = 5f; // Polun päivitysväli
    private Vector2[] path; // Löydetty polku
    private int pathIndex; // Polun indeksi
    private float pathTimer;

    private float gridSizeX = 20f; // Ruudukon leveys
    private float gridSizeY = 10f; // Ruudukon korkeus
    private float nodeSize = 1f; // Ruudun koko
    private Node[,] grid; // Dynaaminen ruudukko

    private Vector2 currentTarget; // Nykyinen kohdepiste
    private bool isMoving; // Liikkuuko vihollinen

    void Start()
    {
        pathTimer = updatePathInterval;
        currentTarget = transform.position; // Alusta nykyinen kohde vihollisen sijaintiin
        isMoving = false;
    }

    void Update()
    {
        // Päivitä polku ajoittain
        pathTimer -= Time.deltaTime;
        if (pathTimer <= 0f)
        {
            path = FindPath(transform.position, player.position);
            pathIndex = 0;
            pathTimer = updatePathInterval;

            // Aseta ensimmäinen kohdepiste, jos polku löytyy
            if (path != null && path.Length > 0)
            {
                currentTarget = path[pathIndex];
                isMoving = true;
            }
            else
            {
                isMoving = false;
            }
        }

        // Liiku sulavasti kohti nykyistä kohdetta
        if (isMoving)
        {
            transform.position = Vector2.MoveTowards(transform.position, currentTarget, speed * Time.deltaTime);

            // Jos ollaan lähellä kohdetta, siirry seuraavaan
            if (Vector2.Distance(transform.position, currentTarget) < 0.1f)
            {
                pathIndex++;
                if (path != null && pathIndex < path.Length)
                {
                    currentTarget = path[pathIndex];
                }
                else
                {
                    isMoving = false; // Lopeta liike, jos polku loppuu
                }
            }
        }
    }

    Vector2[] FindPath(Vector2 startPos, Vector2 targetPos)
    {
        Vector2 gridCenter = (startPos + targetPos) / 2;
        grid = CreateGrid(gridCenter);

        Vector2Int start = WorldToGrid(startPos, gridCenter);
        Vector2Int target = WorldToGrid(targetPos, gridCenter);

        int gridX = Mathf.RoundToInt(gridSizeX / nodeSize);
        int gridY = Mathf.RoundToInt(gridSizeY / nodeSize);

        if (start.x < 0 || start.x >= gridX || start.y < 0 || start.y >= gridY)
        {
            Debug.LogWarning($"Aloituspiste ({startPos.x}, {startPos.y}) ruudukon ulkopuolella!");
            return null;
        }
        if (target.x < 0 || target.x >= gridX || target.y < 0 || target.y >= gridY)
        {
            Debug.LogWarning($"Kohdepiste ({targetPos.x}, {targetPos.y}) ruudukon ulkopuolella!");
            return null;
        }

        List<Node> openList = new List<Node>();
        HashSet<Node> closedList = new HashSet<Node>();
        Node startNode = grid[start.x, start.y];
        startNode.G = 0;
        openList.Add(startNode);

        int maxIterations = gridX * gridY;
        int iteration = 0;

        while (openList.Count > 0)
        {
            if (iteration++ > maxIterations)
            {
                Debug.LogWarning("A* ylitti maksimisolmut!");
                return null;
            }

            Node current = openList[0];
            for (int i = 1; i < openList.Count; i++)
            {
                if (openList[i].F < current.F || (openList[i].F == current.F && openList[i].H < current.H))
                    current = openList[i];
            }

            openList.Remove(current);
            closedList.Add(current);

            if (current.gridX == target.x && current.gridY == target.y)
            {
                List<Node> pathNodes = new List<Node>();
                Node node = current;
                while (node != null)
                {
                    pathNodes.Add(node);
                    node = node.parent;
                }
                pathNodes.Reverse();
                return pathNodes.ConvertAll(n => n.worldPos).ToArray();
            }

            int[] dx = { 1, -1, 0, 0 };
            int[] dy = { 0, 0, 1, -1 };
            for (int i = 0; i < 4; i++)
            {
                int newX = current.gridX + dx[i];
                int newY = current.gridY + dy[i];

                if (newX >= 0 && newX < gridX && newY >= 0 && newY < gridY)
                {
                    Node neighbor = grid[newX, newY];
                    if (!neighbor.walkable || closedList.Contains(neighbor)) continue;

                    int newG = current.G + 1;
                    if (newG < neighbor.G)
                    {
                        neighbor.G = newG;
                        neighbor.H = Mathf.Abs(newX - target.x) + Mathf.Abs(newY - target.y);
                        neighbor.parent = current;

                        if (!openList.Contains(neighbor))
                            openList.Add(neighbor);
                    }
                }
            }
        }
        Debug.LogWarning($"Polkua ei löydetty! Vihollinen: ({startPos.x}, {startPos.y}), Pelaaja: ({targetPos.x}, {targetPos.y})");
        return null;
    }

    Node[,] CreateGrid(Vector2 center)
    {
        int gridX = Mathf.RoundToInt(gridSizeX / nodeSize);
        int gridY = Mathf.RoundToInt(gridSizeY / nodeSize);
        Node[,] newGrid = new Node[gridX, gridY];

        Vector2 gridOffset = new Vector2(gridSizeX / 2, gridSizeY / 2);
        for (int x = 0; x < gridX; x++)
        {
            for (int y = 0; y < gridY; y++)
            {
                Vector2 worldPos = new Vector2(x * nodeSize, y * nodeSize) - gridOffset + center;
                bool walkable = !Physics2D.OverlapBox(worldPos, Vector2.one * nodeSize * 0.9f, 0, obstacleLayer);
                newGrid[x, y] = new Node(walkable, worldPos, x, y);
            }
        }
        return newGrid;
    }

    Vector2Int WorldToGrid(Vector2 worldPos, Vector2 gridCenter)
    {
        int x = Mathf.RoundToInt((worldPos.x - (gridCenter.x - gridSizeX / 2)) / nodeSize);
        int y = Mathf.RoundToInt((worldPos.y - (gridCenter.y - gridSizeY / 2)) / nodeSize);
        return new Vector2Int(x, y);
    }

    void OnDrawGizmos()
    {
        if (grid != null)
        {
            for (int x = 0; x < grid.GetLength(0); x++)
            {
                for (int y = 0; y < grid.GetLength(1); y++)
                {
                    Node node = grid[x, y];
                    Gizmos.color = node.walkable ? Color.green : Color.red;
                    Gizmos.DrawWireCube(node.worldPos, new Vector3(nodeSize * 0.9f, nodeSize * 0.9f, 0));
                }
            }
        }

        if (path != null)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < path.Length - 1; i++)
            {
                Gizmos.DrawLine(path[i], path[i + 1]);
            }
            if (pathIndex < path.Length)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(path[pathIndex], 0.2f);
            }
        }

        Gizmos.color = Color.black;
        Gizmos.DrawSphere(transform.position, 0.3f);
        if (player != null)
            Gizmos.DrawSphere(player.position, 0.3f);
    }
}

public class Node
{
    public bool walkable;
    public Vector2 worldPos;
    public int gridX, gridY;
    public int G;
    public int H;
    public int F { get { return G + H; } }
    public Node parent;

    public Node(bool walkable, Vector2 worldPos, int gridX, int gridY)
    {
        this.walkable = walkable;
        this.worldPos = worldPos;
        this.gridX = gridX;
        this.gridY = gridY;
        this.G = int.MaxValue;
        this.H = 0;
        this.parent = null;
    }
}