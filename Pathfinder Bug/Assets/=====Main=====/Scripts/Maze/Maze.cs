using System.Collections.Generic;
using System.IO;
using System.Threading;
using Sirenix.OdinInspector;
using UnityEngine;

public class Maze : MonoBehaviour,IMazeData
{
    private Cell[,] grid;
    [SerializeField] private List<Cell> _path; 
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private Cell _startcell;
    [SerializeField] private Cell _endcell;
    Cell[,] IMazeData.grid { get => grid; set => grid = value; }
    List<Cell> IMazeData.Path { get =>_path; set => _path = value; }
    public Cell startCell { get =>_startcell; set => _startcell = value; }
    public Cell endCell { get =>_endcell; set => _endcell = value; }
    Vector2 IMazeData.MazeSize { get ; set ; }
    public Transform GetTranform()
    {
        return this.transform;
    }
    [Button(ButtonSizes.Large)]
    public void ShowLinePath()
    {
        lineRenderer.positionCount = _path.Count;
        for(int i= 0 ; i < _path.Count;i++)
        {
            lineRenderer.SetPosition(i, new Vector3(_path[i].X + .5f,_path[i].Y + .5f));
        }
    }
}