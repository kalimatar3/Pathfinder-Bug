using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Linq;

public class MazeGenerator : MonoBehaviour, IMazeGenerator
{
    public int mazeWidth = 10;
    public int mazeHeight = 13;
    [SerializeField] protected GameObject cellPrefab; 
    [SerializeField] protected GameObject bugPrefab;    
    [SerializeField] protected GameObject targetPrefab; 
    [SerializeField] protected Maze MazePrefab;
    private Cell[,] grid; 
    public float creatureZOffset = -0.1f; 

    Maze IMazeGenerator.GenerateMaze() => GenerateMaze();
    void IMazeGenerator.InstantiateCellPrefabs(IMazeData maze) => this.InstantiateCellPrefabs(maze);
    Bug IMazeGenerator.InstantiateBug(IMazeData maze) => this.InstantiateBug(maze);
    Gate IMazeGenerator.InstantiateGate(IMazeData maze) => InstantiateGate(maze);

    Maze GenerateMaze()
    {
        Maze Maze = Instantiate(MazePrefab);
        IMazeData mazeData = Maze.GetComponent<IMazeData>();
        mazeData.MazeSize = new Vector2(mazeWidth,mazeHeight);
        grid = new Cell[mazeWidth, mazeHeight];
        InitializeGrid(); 
        Maze.startCell = grid[0, mazeHeight - 1]; 
        do
        {
            int randomX = Random.Range(0, mazeWidth);
            int randomY = Random.Range(0, mazeHeight);
            Maze.endCell = grid[randomX, randomY]; 
        } while ( Maze.endCell == Maze.startCell);

        // Generate the guaranteed primary path ---
        mazeData.Path = GeneratePrimaryPath(Maze.endCell, Maze.startCell);
        if (mazeData.Path.Count == 0)
        {
            Debug.LogError("Failed to generate a primary path. Aborting maze generation.");
            return null;
        }

        // Fill in the rest of the maze using Recursive Backtracker (DFS)
        // This loop now ensures all parts of the maze are connected
        for (int y = 0; y < mazeHeight; y++) 
        {
            for(int x = 0; x < mazeWidth;x++)
            {
                if (!grid[x, y].Visited)
                {
                    // If a cell hasn't been visited, it means it's part of an isolated region.
                    // Try to connect it to an already visited part of the maze.
                    List<Cell> visitedNeighbors = GetNeighbors(grid[x,y]).Where(n => n.Visited).ToList();
                    if(visitedNeighbors.Count > 0)
                    {
                        // Choose a random visited neighbor and break a wall to connect
                        Cell connectionNeighbor = visitedNeighbors[Random.Range(0, visitedNeighbors.Count)];
                        RemoveWallsBetween(grid[x,y], connectionNeighbor);
                        grid[x,y].Visited = true; // Mark as visited to ensure RecursiveBacktracker starts from a connected point
                    }

                    // Now run RecursiveBacktracker on this cell.
                    // If it was just connected, it will expand from there.
                    // If it's still isolated (e.g., first cell of a new region), it will start a new branch.
                    RecursiveBacktracker(grid[x, y]); 
                }              
            }
        }
        mazeData.grid = this.grid;
        return Maze.GetComponent<Maze>();
    }

    void InitializeGrid()
    {
        for (int x = 0; x < mazeWidth; x++) 
        {
            for (int y = 0; y < mazeHeight; y++) 
            {
                grid[x, y] = new Cell(x, y); 
            }
        }
    }

    // Method to generate a guaranteed path from start to end using a modified DFS-like random walk
    List<Cell> GeneratePrimaryPath(Cell start, Cell end)
    {
        Stack<Cell> pathStack = new Stack<Cell>();
        Dictionary<Cell, Cell> pathCameFrom = new Dictionary<Cell, Cell>();
        HashSet<Cell> pathVisited = new HashSet<Cell>(); 

        pathStack.Push(start);
        pathVisited.Add(start);
        start.Visited = true; 

        Cell current = start;

        while (pathStack.Count > 0)
        {
            current = pathStack.Peek();

            if (current == end)
            {
                break; // Path found
            }

            List<Cell> possiblePathNeighbors = new List<Cell>();
            int x = current.X;
            int y = current.Y; 

            // Directions for (x,y) indexing: Right (x+1,y), Left (x-1,y), Up (x,y+1), Down (x,y-1)
            // Check Right neighbor
            if (x < mazeWidth - 1) { Cell neighbor = grid[x + 1, y]; if (!pathVisited.Contains(neighbor)) possiblePathNeighbors.Add(neighbor); }
            // Check Left neighbor
            if (x > 0) { Cell neighbor = grid[x - 1, y]; if (!pathVisited.Contains(neighbor)) possiblePathNeighbors.Add(neighbor); }
            // Check Up neighbor
            if (y < mazeHeight - 1) { Cell neighbor = grid[x, y + 1]; if (!pathVisited.Contains(neighbor)) possiblePathNeighbors.Add(neighbor); }
            // Check Down neighbor
            if (y > 0) { Cell neighbor = grid[x, y - 1]; if (!pathVisited.Contains(neighbor)) possiblePathNeighbors.Add(neighbor); }
            
            if (possiblePathNeighbors.Count > 0)
            {
                Cell nextCell = possiblePathNeighbors[Random.Range(0, possiblePathNeighbors.Count)];
                RemoveWallsBetween(current, nextCell); // Pre-remove walls for the primary path
                nextCell.Visited = true; // Mark as visited for subsequent maze generation to avoid re-processing
                pathVisited.Add(nextCell);
                pathCameFrom[nextCell] = current;
                pathStack.Push(nextCell);
            }
            else
            {
                pathStack.Pop();
            }
        }

        // Reconstruct the path from end to start
        List<Cell> primaryPath = new List<Cell>();
        Cell temp = end;
        while (temp != start)
        {
            if (!pathCameFrom.ContainsKey(temp)) 
            {
                Debug.LogError("Path reconstruction failed. CameFrom dictionary incomplete.");
                return new List<Cell>();
            }
            primaryPath.Add(temp);
            temp = pathCameFrom[temp];
        }
        primaryPath.Add(start);
        return primaryPath;
    }

    // Main maze generation algorithm (Recursive Backtracker / DFS)
    void RecursiveBacktracker(Cell currentCell)
    {
        currentCell.Visited = true; // Mark the current cell as visited
        Stack<Cell> stack = new Stack<Cell>();
        stack.Push(currentCell); // Push the current cell onto the stack

        while (stack.Count > 0)
        {
            currentCell = stack.Peek(); // Look at the top cell on the stack

            List<Cell> unvisitedNeighbors = GetUnvisitedNeighbors(currentCell);

            if (unvisitedNeighbors.Count > 0)
            {
                // If there are unvisited neighbors, choose one randomly
                int randomIndex = Random.Range(0, unvisitedNeighbors.Count);
                Cell chosenNeighbor = unvisitedNeighbors[randomIndex];
                RemoveWallsBetween(currentCell, chosenNeighbor); // Remove the wall between current and chosen
                chosenNeighbor.Visited = true; // Mark the chosen neighbor as visited
                stack.Push(chosenNeighbor); // Push the chosen neighbor onto the stack
            }
            else
            {
                stack.Pop();
            }
        }
    }

    List<Cell> GetNeighbors(Cell cell) // Returns ALL neighbors, visited or not
    {
        List<Cell> neighbors = new List<Cell>();
        int x = cell.X;
        int y = cell.Y;
        // Right neighbor (grid[x+1, y])
        if (x < mazeWidth - 1 )
            neighbors.Add(grid[x + 1, y]);
        // Left neighbor (grid[x-1, y])
        if (x > 0 )
            neighbors.Add(grid[x - 1, y]);
        // Up neighbor (grid[x, y+1])
        if (y < mazeHeight - 1 )
            neighbors.Add(grid[x, y + 1]);
        // Down neighbor (grid[x, y-1])
        if (y > 0 )
            neighbors.Add(grid[x, y - 1]);

        return neighbors;
    }

    List<Cell> GetUnvisitedNeighbors(Cell cell) // Returns only UNVISITED neighbors
    {
        List<Cell> neighbors = new List<Cell>();
        int x = cell.X;
        int y = cell.Y;

        // Possible directions for maze generation (only unvisited cells)
        // Right neighbor (grid[x+1, y])
        if (x < mazeWidth - 1 && !grid[x + 1, y].Visited)
            neighbors.Add(grid[x + 1, y]);
        // Left neighbor (grid[x-1, y])
        if (x > 0 && !grid[x - 1, y].Visited)
            neighbors.Add(grid[x - 1, y]);
        // Up neighbor (grid[x, y+1])
        if (y < mazeHeight - 1 && !grid[x, y + 1].Visited)
            neighbors.Add(grid[x, y + 1]);
        // Down neighbor (grid[x, y-1])
        if (y > 0 && !grid[x, y - 1].Visited)
            neighbors.Add(grid[x, y - 1]);

        return neighbors;
    }
    void RemoveWallsBetween(Cell cell1, Cell cell2)
    {
        // Determine the relative position of cell2 to cell1 (now based on x,y)
        if (cell1.X == cell2.X) // Same column (vertical movement)
        {
            if (cell1.Y > cell2.Y) // cell2 is below cell1
            {
                cell1.RemoveWall("Bottom");
                cell2.RemoveWall("Top");
            }
            else // cell2 is above cell1
            {
                cell1.RemoveWall("Top");
                cell2.RemoveWall("Bottom");
            }
        }
        else if (cell1.Y == cell2.Y) // Same row (horizontal movement)
        {
            if (cell1.X > cell2.X) // cell2 is to the left of cell1
            {
                cell1.RemoveWall("Left");
                cell2.RemoveWall("Right");
            }
            else // cell2 is to the right of cell1
            {
                cell1.RemoveWall("Right");
                cell2.RemoveWall("Left");
            }
        }
    }

    // This method will instantiate the Unity GameObjects for the maze
    void InstantiateCellPrefabs(IMazeData maze)
    {
        for (int x = 0; x < mazeWidth; x++) 
        {
            for (int y = 0; y < mazeHeight; y++) 
            {
                Cell cellData = grid[x, y]; 

                GameObject mazeCellGO = Instantiate(cellPrefab, maze.GetTranform().position + new Vector3(x,y), Quaternion.identity, maze.GetTranform());
                mazeCellGO.name = $"Cell_{x}_{y}";

                MazeCellView cellView = mazeCellGO.GetComponent<MazeCellView>();
                if (cellView != null)
                {
                    cellView.SetupWalls(cellData.Walls);
                }
                else
                {
                    Debug.LogError($"MazeCellView component not found on cellPrefab for Cell({x},{y}). Make sure your prefab has MazeCellView attached!");
                }
            }
        }
    }

    protected Gate InstantiateGate(IMazeData maze)
    {
        // Instantiate the Target prefab
        if (targetPrefab != null)
        {
            GameObject GateGO = Instantiate(targetPrefab, maze.GetTranform().position + new Vector3(maze.endCell.X + .5f,maze.endCell.Y + .5f) + Vector3.forward * creatureZOffset, Quaternion.identity, maze.GetTranform());
            return GateGO.GetComponent<Gate>();
        }
        else
        {
            Debug.LogWarning("Target Prefab is not assigned in MazeGenerator.");
            return null;
        }
    }
    protected Bug InstantiateBug(IMazeData maze)
    {
        if (bugPrefab != null)
        {
            GameObject BugGO = Instantiate(bugPrefab, maze.GetTranform().position + new Vector3(maze.startCell.X + .5f,maze.startCell.Y + .5f) + Vector3.forward * creatureZOffset, Quaternion.identity, maze.GetTranform());
            return BugGO.GetComponent<Bug>();
        }
        else
        {
            Debug.LogWarning("Bug Prefab is not assigned in MazeGenerator.");
            return null;
        }      
    }
}