using Clouds.Ultilities;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; 

public class GamePlayManager : Singleton<GamePlayManager>,IGamePlayController
{
    [SerializeField] protected IMazeGenerator mazeGenerator;
    [SerializeField] protected baseCanvas gamePlayCanvas;
    [SerializeField] protected IGamePlayUIData gamePlayUIData;
    [SerializeField] protected IBackGroundUIData backGroundUIData;
    [SerializeField] protected IMazeData mazeData;
    public Maze Maze;
    public Bug Bug;
    public Gate Gate;
    private Material backgroundMaterial;
    Maze IGamePlayController.Maze => Maze;
    Bug IGamePlayController.Bug => Bug;
    Gate IGamePlayController.Gate => Gate;
    void IGamePlayController.PlaceMaze() => PlaceMaze();
    public override void Awake()
    {
        base.Awake();
        mazeGenerator = GetComponent<IMazeGenerator>();
        gamePlayUIData = gamePlayCanvas.GetComponent<GamePlayCanvas>();
        backGroundUIData = GUIManager.Instance.BackGroundCanvas; // Get BackGroundCanvas from GUIManager
        GUIManager.Instance.SwitchCanvas(gamePlayCanvas);
        Image backgroundImage = backGroundUIData.BackGroundImage;
        if (backgroundImage != null)
        {
            backgroundMaterial = backgroundImage.material;
        }
    }
    protected void Start()
    {
        this.StartLevel();
        this.UISettup();
    }
    protected void UISettup()
    {
        gamePlayUIData.btnFindPath.OnPointerClickCallBack_Completed += Maze.ShowLinePath;
        gamePlayUIData.btnAutoMove.OnPointerClickCallBack_Completed += () => Bug.MovetoPath(mazeData.Path);
        gamePlayUIData.btnBack.OnPointerClickCallBack_Completed += BackToMenu;
    }
    protected async void BackToMenu()
    {
        backGroundUIData.AppearAnimaiton.Play();
        do
        {
            await UniTask.DelayFrame(1);
        }
        while( backGroundUIData.AppearAnimaiton.IsPlaying) ;
        await SceneManager.LoadSceneAsync("Menu");
        backGroundUIData.AppearAnimaiton.Restart();
    }
    public async void StartLevel()
    {
        Maze = mazeGenerator.GenerateMaze();
        mazeData = Maze.GetComponent<IMazeData>();
        Bug = mazeGenerator.InstantiateBug(Maze);
        Gate = mazeGenerator.InstantiateGate(Maze);
        mazeGenerator.InstantiateCellPrefabs(Maze);
        Maze.gameObject.SetActive(false);
        await UniTask.DelayFrame(1); // Wait for canvas to resize
        PlaceMaze();
        SetPivotRevealBackGroundCenter(); // Call this function after the maze and gate have been positioned
    }
    public async void EndLevel()
    {
        backGroundUIData.AppearAnimaiton.Play();
        do
        {
            await UniTask.DelayFrame(1);
        }
        while( backGroundUIData.AppearAnimaiton.IsPlaying);
        await SceneManager.LoadSceneAsync("Play");
        backGroundUIData.AppearAnimaiton.Restart();
    }
    protected void PlaceMaze() // Place the Maze
    {
        // Activate the Maze GameObject so it becomes visible.
        Maze.gameObject.SetActive(true); 

        Vector3[] corners = new Vector3[4];
        // Get the world space coordinates of the four corners of the MazeField UI element.
        // corners[0] = bottom-left, corners[1] = top-left, corners[2] = top-right, corners[3] = bottom-right
        gamePlayUIData.MazeField.GetWorldCorners(corners);

        // Calculate the width of the MazeField in world space.
        float width = Vector3.Distance(corners[0], corners[3]);
        // Calculate the height of the MazeField in world space.
        float height = Vector3.Distance(corners[0], corners[1]);

        // Determine the scaling ratio needed to fit the maze within the MazeField.
        // We take the minimum of (field_width / maze_width) and (field_height / maze_height)
        // to ensure the maze fits entirely without distortion, scaling down if necessary.
        float ratio = Mathf.Min(width / mazeData.MazeSize.x, height / mazeData.MazeSize.y);

        // Apply the calculated scale to the Maze's transform.
        // This scales the entire maze (including cells, bug, gate) to fit the UI field.
        Maze.transform.localScale = Vector3.one * ratio;

        // Calculate the center point of the MazeField in world space.
        Vector3 center = (corners[0] + corners[2]) * 0.5f;

        // Position the Maze.
        // The Maze's local origin (0,0) is assumed to be its bottom-left corner after instantiation.
        // We want to align this bottom-left corner with the calculated bottom-left of the scaled MazeField.
        // Therefore, we take the center of the MazeField and subtract half of the scaled maze's dimensions
        // to effectively move the maze's (0,0) to the bottom-left of the MazeField.
        Maze.transform.localPosition = center - new Vector3(mazeData.MazeSize.x / 2, mazeData.MazeSize.y / 2) * ratio;
    }    
    protected void SetPivotRevealBackGroundCenter()
    {
        if (Gate == null || backGroundUIData == null || backGroundUIData.BackGroundImage == null || backgroundMaterial == null)
        {
            Debug.LogWarning("Required components for setting reveal center are missing.");
            return;
        }

        RectTransform backgroundRect = backGroundUIData.BackGroundImage.GetComponent<RectTransform>();
        ICameraHolder cameraHolder = GUIManager.Instance.BackGroundCanvas.GetComponent<ICameraHolder>(); // Assuming BaseCanvas implements ICameraHolder or has a Camera property

        if (backgroundRect == null || cameraHolder == null || cameraHolder.Camera == null)
        {
            Debug.LogError("Background RectTransform or Camera not found for reveal center calculation.");
            return;
        }

        // Get world position of Gate
        Vector3 gateWorldPosition = Gate.transform.position;
        Vector2 screenPoint = cameraHolder.Camera.WorldToScreenPoint(gateWorldPosition);
        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(backgroundRect, screenPoint, cameraHolder.Camera, out localPoint))
        {
            // Normalize local coordinates to UV (0-1)
            // localPoint ranges from (-rect.width/2, -rect.height/2) to (rect.width/2, rect.height/2)
            // Need to convert it to a (0,1) range
            float uvX = (localPoint.x + backgroundRect.rect.width / 2) / backgroundRect.rect.width;
            float uvY = (localPoint.y + backgroundRect.rect.height / 2) / backgroundRect.rect.height;

            Vector2 revealCenterUV = new Vector2(uvX, uvY);
            backgroundMaterial.SetVector("_RevealCenter", new Vector4(revealCenterUV.x, revealCenterUV.y, 0, 0));
        }
    }

}