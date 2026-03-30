using UnityEngine;

public interface IGamePlayUIData
{
    public RectTransform MazeField {get;}
    CloudsButton btnFindPath {get;}
    CloudsButton btnAutoMove {get;}
    CloudsButton btnBack{get;}
}