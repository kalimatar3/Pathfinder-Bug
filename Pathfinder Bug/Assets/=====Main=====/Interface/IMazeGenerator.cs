public interface IMazeGenerator
{
    Maze GenerateMaze();
    void InstantiateCellPrefabs(IMazeData maze);
    Bug InstantiateBug(IMazeData maze);
    Gate InstantiateGate(IMazeData maze);
}