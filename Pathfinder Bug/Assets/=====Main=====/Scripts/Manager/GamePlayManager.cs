using Clouds.Ultilities;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Required for Image and RectTransformUtility

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
    protected void PlaceMaze()
    {
        Maze.gameObject.SetActive(true);
        Vector3[] corners = new Vector3[4];
        gamePlayUIData.MazeField.GetWorldCorners(corners);
        float width = Vector3.Distance(corners[0], corners[3]);
        float height = Vector3.Distance(corners[0], corners[1]);
        float ratio = Mathf.Min(width / mazeData.MazeSize.x, height / mazeData.MazeSize.y);
        Maze.transform.localScale = Vector3.one * ratio;
        Vector3 center = (corners[0] + corners[2]) * 0.5f;
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

        // Convert screen coordinates to local coordinates within the background's RectTransform
        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(backgroundRect, screenPoint, cameraHolder.Camera, out localPoint))
        {
            // Normalize local coordinates to UV (0-1)
            // localPoint ranges from (-rect.width/2, -rect.height/2) to (rect.width/2, rect.height/2)
            // Need to convert it to a (0,1) range
            float uvX = (localPoint.x + backgroundRect.rect.width / 2) / backgroundRect.rect.width;
            float uvY = (localPoint.y + backgroundRect.rect.height / 2) / backgroundRect.rect.height;

            Vector2 revealCenterUV = new Vector2(uvX, uvY);

            // Set _RevealCenter for the shader material
            backgroundMaterial.SetVector("_RevealCenter", new Vector4(revealCenterUV.x, revealCenterUV.y, 0, 0));
        }
    }

}