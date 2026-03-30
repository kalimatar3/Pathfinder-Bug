public interface IGamePlayController
{
    Maze Maze {get;}
    Bug Bug {get;}
    Gate Gate {get;} 
    void PlaceMaze();
    void StartLevel();
    void EndLevel();
}