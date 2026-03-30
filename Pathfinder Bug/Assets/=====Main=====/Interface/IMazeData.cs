using System.Collections.Generic;
using UnityEngine;

public interface IMazeData
{
    public Cell[,] grid {get; set;}
    public List<Cell> Path {get;set;} 
    public Vector2 MazeSize {get;set;}
    Cell startCell {get;set;}
    Cell endCell {get;set;}
    Transform GetTranform();
}