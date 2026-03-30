using UnityEngine;

public class GamePlayCanvas : baseCanvas,IGamePlayUIData,ICameraHolder
{
    [SerializeField] protected RectTransform _mazeField;
    RectTransform IGamePlayUIData.MazeField => _mazeField;

    [SerializeField] protected CloudsButton btnFindPath;
    [SerializeField] protected CloudsButton btnAutoMove;
    [SerializeField] protected CloudsButton btnBack;
    [SerializeField] protected Camera _camera;
    CloudsButton IGamePlayUIData.btnFindPath => btnFindPath;
    CloudsButton IGamePlayUIData.btnAutoMove => btnAutoMove;
    CloudsButton IGamePlayUIData.btnBack => btnBack;
    public Camera Camera => _camera;
}
