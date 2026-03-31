using UnityEngine;
using System.Collections.Generic;

public class MazeCellView : MonoBehaviour
{
    public GameObject topWall;
    public GameObject rightWall;
    public GameObject bottomWall;
    public GameObject leftWall;
    public void SetupWalls(Dictionary<string, bool> wallStates)
    {
        if (topWall != null) topWall.SetActive(wallStates["Top"]);
        if (rightWall != null) rightWall.SetActive(wallStates["Right"]);
        if (bottomWall != null) bottomWall.SetActive(wallStates["Bottom"]);
        if (leftWall != null) leftWall.SetActive(wallStates["Left"]);
    }
}