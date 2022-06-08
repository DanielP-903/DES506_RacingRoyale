using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public struct RoadPart
{
    public float rotation;
    public GameObject part;
}

public class Pathfinder : MonoBehaviour
{
    public GameObject playerRef;

    public List<GameObject> straightPieces;
    public List<GameObject> cornerPieces;

    [FormerlySerializedAs("WIDTH")] public int width;
    [FormerlySerializedAs("HEIGHT")] public int height;

    private readonly Node[,] _grid = new Node[17, 17];
    public Node nodeRef;

    public Vector2Int startPos;
    public Vector2Int endPos;

    public int Scale;

    private readonly List<Node> _path = new List<Node>();
    private readonly List<Node> _originalPath = new List<Node>();

    private readonly List<Node> _openNodes = new List<Node>();
    private readonly List<Node> _closedNodes = new List<Node>();
    private readonly List<Node> _childrenNodes = new List<Node>();
    
    private readonly List<Node> _obstacles = new List<Node>();
    private readonly List<Node> _usedBlocks = new List<Node>();
    private readonly List<Node> _originalNodes = new List<Node>();
                 
    private Node _end;
    private Node _start;
                 
    private Node _originalEnd;
    private Node _originalStart;
    private Node _current;
    private Node _temp;
    private CarController _carController;

    private List<RoadPart> _roadParts = new List<RoadPart>();
    void OnDrawGizmos()
    {
        if (_grid[0, 0] != null)
        {
            foreach (Node n in _usedBlocks)
            {
                if (_originalPath.Contains(n))
                {
                    Gizmos.color = Color.green;
                }

                if (_path.Contains(n))
                {
                    Gizmos.color = Color.cyan;
                    if (n == _start)
                    {
                        Gizmos.color = Color.yellow;
                    }
                    if (n == _end)
                    {
                        Gizmos.color = Color.blue;
                    }
                }

                if (_originalPath.Contains(n) && _path.Contains(n))
                {
                    //Debug.Log("Uh oh...");
                }

                if (_originalNodes.Contains(n))
                {
                    if (n == _originalStart)
                    {
                        Gizmos.color = Color.yellow;
                    }
                    if (n == _originalEnd)
                    {
                        Gizmos.color = Color.blue;
                    }
                }
                
                if (_obstacles.Contains(n))
                {
                    Gizmos.color = Color.magenta;
                }

                Gizmos.DrawWireCube(n.transform.position,
                    new Vector3(0.95f * Scale, 0.95f * Scale, 0.95f * Scale));
            }
        }
    }

    private void Setup()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                _grid[i, j] = Instantiate(nodeRef, transform);
                _grid[i, j].gridPosition = new Vector3Int(i, j, 0);
                _grid[i, j].nodeType = 0;
                _grid[i, j].transform.position = new Vector3Int(i * Scale, 10, j * Scale);//, k * Scale);
            }
        }

        _openNodes.Clear();
        _closedNodes.Clear();
        _childrenNodes.Clear();
        _obstacles.Clear();
        _usedBlocks.Clear();
        _originalNodes.Clear();
        _path.Clear();
        _originalPath.Clear();

        MakeObstacle();
        MakeObstacle();
        MakeObstacle();
        MakeObstacle();
        MakeObstacle();
        MakeObstacle();
        MakeObstacle();
        MakeObstacle();
        MakeObstacle();
        MakeObstacle();
        MakeObstacle();

        Vector3Int randomVec = new Vector3Int(-1, -1, -1)
        {
            x = Random.Range(0, width),
            y = Random.Range(0, height)
        };

        _start = _grid[startPos.x, startPos.y];

        randomVec.y = Random.Range(0, height);

        _end = _grid[endPos.x, endPos.y];

        _originalStart = _start;
        _originalEnd = _end;
        _usedBlocks.Add(_start);
        _usedBlocks.Add(_end);
        _originalNodes.Add(_start);
        _originalNodes.Add(_end);

        _start.g = 0;
        _start.h = Heuristic(_start, _end);
        _start.f = 0;

        _current = _start;

        _temp = _current;
        _temp.f = float.MaxValue;

        AStar();

        GeneratePath();
    }

    private void GeneratePath()
    {
        Vector3 currentDirection = Vector3.zero;
        Vector3 previousDirection = Vector3.zero;
        for (var index = 1; index < _originalPath.Count-1; index++)
        {
            Node current = _originalPath[index];
            if (index + 1 < _originalPath.Count)
            {
                Node next = _originalPath[index + 1];
                GameObject chosenRoadPart;
                if (next != null)
                {
                    currentDirection = (next.transform.position - current.transform.position).normalized;
                    if (currentDirection != previousDirection)
                    {
                        chosenRoadPart = cornerPieces[0];
                    }
                    else
                    {
                        chosenRoadPart = straightPieces[0];
                    }

                    RoadPart roadPart = new RoadPart();
                    roadPart.part = chosenRoadPart;
                    roadPart.rotation = Vector3.Angle(previousDirection,currentDirection);
                    _roadParts.Add(roadPart);

                    //chosenRoadPart.transform.rotation = Quaternion.LookRotation(currentDirection); // * _currentTransform.rotation;
                    //roadPart.SetActive(false);

                }
            }

            //_currentTransform.rotation = chosenRoadPart.transform.rotation;
            previousDirection = currentDirection;
        }

        StartCoroutine(Delay());
    }

    private IEnumerator Delay()
    {
        float currentRotation = 0;
        for (var index = 1; index < _originalPath.Count-1; index++)
        {
            Node n = _originalPath[index];
            yield return new WaitForSeconds(0.2f);
            RoadPart road = _roadParts[index-1];
            currentRotation += road.rotation;
            road.part.transform.Rotate(currentRotation * Vector3.up);

            GameObject roadPart =
                Instantiate(road.part, n.transform.position, road.part.transform.rotation); //, _originalPath[index].transform);
        }
    }

    private void AddUsedBlocks()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (_path.Contains(_grid[i, j]))
                {
                    _usedBlocks.Add(_grid[i, j]);
                }
                else if (_originalPath.Contains(_grid[i, j]))
                {
                    _usedBlocks.Add(_grid[i, j]);
                }
            }
        }
    }

    private void MakeObstacle(int preX = -1, int preY = -1)
    {
        if (preX == -1)
        {
            int x = Random.Range(1, width);
            int y = Random.Range(1, height);

            _grid[x, y].nodeType = 1;
            _obstacles.Add(_grid[x, y]);
            _usedBlocks.Add(_grid[x, y]);
        }
        else
        {
            _grid[preX, preY].nodeType = 1;
            _obstacles.Add(_grid[preX, preY]);
            _usedBlocks.Add(_grid[preX, preY]);
        }

    }

    private void DestroyGrid()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                Destroy(_grid[i, j].gameObject);
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        _carController = playerRef.GetComponent<CarController>();
        Setup();
        AddUsedBlocks();
    }

    // Update is called once per frame
    void Update()
    {
        if (_carController.GetBoost())
        {
            DestroyGrid();
            Setup();
            AddUsedBlocks();
        }
    }

    // Get a heuristic distance between two nodes
    float Heuristic(Node a, Node b)
    {
        return Vector3.Distance(a.transform.position, b.transform.position);
    }


    // Find lowest f value in all open nodes
    Node FindLowest()
    {
        foreach (Node n in _openNodes)
        {
            if (n.f < _temp.f)
            {
                _temp = n;
            }
        }
        return _temp;
    }

    // Get the neighbours of a particular node
    void FindNeighbours()
    {
        // Iterate through all 4 directions
        for (int i = 0; i < 4; i++)
        {
            switch (i)
            {
                case (0): // Right
                    {
                        if (_current.gridPosition.x + 1 < width)
                        {
                            if (_grid[_current.gridPosition.x + 1, _current.gridPosition.y].nodeType != 1 && _originalPath.Contains(_grid[_current.gridPosition.x + 1, _current.gridPosition.y]) == false)
                            {
                                _childrenNodes.Add(_grid[_current.gridPosition.x + 1, _current.gridPosition.y]);
                            }
                        }
                        break;
                    }
                case (1): // Left
                    {
                        if (_current.gridPosition.x - 1 >= 0)
                        {
                            if (_grid[_current.gridPosition.x - 1, _current.gridPosition.y].nodeType != 1 && _originalPath.Contains(_grid[_current.gridPosition.x - 1, _current.gridPosition.y]) == false)
                            {
                                _childrenNodes.Add(_grid[_current.gridPosition.x - 1, _current.gridPosition.y]);
                            }
                        }
                        break;
                    }
                case (2): // Up
                    {
                        if (_current.gridPosition.y + 1 < height)
                        {
                            if (_grid[_current.gridPosition.x, _current.gridPosition.y + 1].nodeType != 1 && _originalPath.Contains(_grid[_current.gridPosition.x, _current.gridPosition.y+1]) == false)
                            {
                                _childrenNodes.Add(_grid[_current.gridPosition.x, _current.gridPosition.y + 1]);
                            }
                        }
                        break;
                    }
                case (3): // Down
                    {
                        if (_current.gridPosition.y - 1 >= 0)
                        {
                            if (_grid[_current.gridPosition.x, _current.gridPosition.y - 1].nodeType != 1 && _originalPath.Contains(_grid[_current.gridPosition.x, _current.gridPosition.y-1]) == false)
                            {
                                _childrenNodes.Add(_grid[_current.gridPosition.x, _current.gridPosition.y - 1]);
                            }
                        }
                        break;
                    }
            }
        }
    }

    void AStar()
    {
        _openNodes.Add(_current);
        while (_openNodes.Count != 0)
        {
            _temp.f = float.MaxValue;
            _current = FindLowest();

            FindNeighbours();

            if (_current == _end)
            {
                Debug.Log("We've found a path!");
                break;
            }

            _openNodes.Remove(_current);
            _closedNodes.Add(_current);

            foreach (Node n in _childrenNodes)
            {
                if (_closedNodes.Contains(n))
                {
                    continue;
                }

                float newG = _current.g + 1;

                if (!_openNodes.Contains(n))
                {
                    _openNodes.Add(n);
                }
                else if (newG >= n.g)
                {
                    continue;
                }

                n.g = newG;
                n.f = n.g + Heuristic(n, _end);
                n.parent = _current;
            }

            _childrenNodes.Clear();
        }

        if (_originalPath.Count < 1)
        {
            _current = _end;
            while (_current != _start)
            {
                _originalPath.Add(_grid[_current.gridPosition.x, _current.gridPosition.y]);
                _current = _current.parent;
            }
            _originalPath.Add(_start);
            _originalPath.Reverse();
        }
        // else
        // {
        //     _current = _end;
        //     while (_current != _start)
        //     {
        //         if (!_originalPath.Contains(_current))
        //         {
        //             _path.Add(_grid[_current.gridPosition.x, _current.gridPosition.y]);
        //         }
        //         _current = _current.parent;
        //     }
        //     _path.Add(_start);
        //     _path.Reverse();
        // }
    }
}