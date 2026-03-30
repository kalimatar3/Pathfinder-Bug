// File: Assets/Scripts/Maze/Cell.cs
using UnityEngine;
using System.Collections.Generic;
[System.Serializable]
public class Cell
{
    public int X { get; private set; }
    public int Y { get; private set; }
    public bool Visited { get; set; }

    public Dictionary<string, bool> Walls { get; private set; }

    public Cell(int x, int y)
    {
        X = x;
        Y = y;
        Visited = false;
        Walls = new Dictionary<string, bool>
        {
            { "Top", true },
            { "Right", true },
            { "Bottom", true },
            { "Left", true }
        };
    }

    public void RemoveWall(string direction)
    {
        if (Walls.ContainsKey(direction))
        {
            Walls[direction] = false;
        }
    }

    public override string ToString()
    {
        return $"Cell({X},{Y}, Visited={Visited}, Walls={string.Join(", ", Walls)})";
    }
}