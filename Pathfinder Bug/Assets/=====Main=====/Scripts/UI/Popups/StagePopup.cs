using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using Clouds.SignalSystem; 
using Clouds.Ultilities; // Required for GetOrAddComponent extension method

public class StagePopup : BasePopup
{
    // --- Assign these references in the Unity Inspector ---
    [Header("Prefabs")]
    [SerializeField] protected StageUIElement stageUIElementPrefab;
    [SerializeField] protected GameObject lineHorPrefab; 
    [SerializeField] protected GameObject lineVerPrefab;
    [Header("Components")]
    [SerializeField] protected ScrollRect stageScrollRect;
    [SerializeField] protected RectTransform contentRect; 
    [SerializeField] protected GridLayoutGroup gridLayoutGroup; 
    [SerializeField] protected RectTransform viewportRect; 
    // --- Stage Data ---
    [Header("Data")]
    [SerializeField] private StageListAsset stageListAsset; 
    [SerializeField] protected float lineHorOffset = 0f; 
    [SerializeField] protected float lineVerOffset = 0f; 
    IStageDataControler stageDataControler; 
    [Header("Pooling Settings")]
    protected IObjectPooler objectPooler;
    public int initialPoolSize = 64; 
    
    private List<StageData> allStageData; 
    private Dictionary<int, StageUIElement> activeUIElements; 
    private Dictionary<int, RectTransform> activeHorLines; 
    private Dictionary<int, RectTransform> activeVerLines; 
    private float cellWidth; 
    private float cellHeight;  
    private float spacingX;
    private float spacingY;
    private float paddingLeft;
    private float paddingRight;
    private float paddingTop;
    private float paddingBottom;

    private float contentHeightWhenFull; 
    private int numberOfColumns; 
    private int visibleRows; 
    private int totalRowsInAllStages; // Total actual number of rows for all stages
    
    private int currentFirstVisibleRow = -1; 
    private int currentLastVisibleRow = -1;  

    protected override void Awake()
    {
        stageDataControler = DataManager.Instance;
        allStageData = stageDataControler.stageDynamicData.stageDatas;
        base.Awake();
    }
    protected void Start()
    {
        objectPooler = ObjectPooler.Instance;
        SetupLayoutMetrics();
        InitializeObjectPoolerPools();
        UpdateVisibleStages();       
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        stageScrollRect.onValueChanged.AddListener(OnScroll);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        stageScrollRect.onValueChanged.RemoveListener(OnScroll);
    }

    void SetupLayoutMetrics()
    {
        // Get information from GridLayoutGroup (for design in Editor)
        cellWidth = gridLayoutGroup.cellSize.x; 
        cellHeight = gridLayoutGroup.cellSize.y;
        spacingX = gridLayoutGroup.spacing.x;
        spacingY = gridLayoutGroup.spacing.y;
        paddingLeft = gridLayoutGroup.padding.left;
        paddingRight = gridLayoutGroup.padding.right;
        paddingTop = gridLayoutGroup.padding.top;
        paddingBottom = gridLayoutGroup.padding.bottom;
        numberOfColumns = 4; 
        if (numberOfColumns == 0) numberOfColumns = 1; 

        gridLayoutGroup.enabled = false; 

        // Calculate total actual number of rows for all stages
        totalRowsInAllStages = Mathf.CeilToInt((float)allStageData.Count / numberOfColumns);

        // Calculate total height of Content
        contentHeightWhenFull = totalRowsInAllStages * (cellHeight + spacingY) - spacingY; // Subtract final spacing Y
        contentHeightWhenFull += paddingTop + paddingBottom; // Add padding

        // Set height for Content RectTransform
        // With Start Corner: LowerLeft, Content will expand from bottom upwards.
        contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, contentHeightWhenFull);

        // LineHolder setup removed

        // Number of visible rows in the viewport (rounded up and with buffer)
        visibleRows = Mathf.CeilToInt(viewportRect.rect.height / (cellHeight + spacingY)) + 2; 
    }

    void InitializeObjectPoolerPools()
    {
        activeUIElements = new Dictionary<int, StageUIElement>();
        activeHorLines = new Dictionary<int, RectTransform>();
        activeVerLines = new Dictionary<int, RectTransform>();
        objectPooler.CreatePool(stageUIElementPrefab.gameObject, initialPoolSize);
        objectPooler.CreatePool(lineHorPrefab, visibleRows + 2);
        objectPooler.CreatePool(lineVerPrefab, visibleRows + 2);
    }

    void OnScroll(Vector2 normalizedPosition)
    {
        UpdateVisibleStages();
    }

    /// <summary>
    /// Updates which stage UI elements are visible based on scroll position,
    /// pooling out hidden elements and pooling in newly visible ones.
    /// </summary>
    void UpdateVisibleStages()
    {
        float currentScrollY = contentRect.anchoredPosition.y;

        float offsetFromContentBottom = -currentScrollY + paddingBottom; 
        
        int newFirstVisibleRow = Mathf.FloorToInt(offsetFromContentBottom / (cellHeight + spacingY));
        
        // Add a buffer row for smooth scrolling
        newFirstVisibleRow = Mathf.Max(0, newFirstVisibleRow - 1); 

        int newLastVisibleRow = Mathf.Min(
            totalRowsInAllStages - 1, 
            newFirstVisibleRow + visibleRows 
        );

        // If visible rows haven't changed, do nothing
        if (newFirstVisibleRow == currentFirstVisibleRow && newLastVisibleRow == currentLastVisibleRow)
        {
            return;
        }

        currentFirstVisibleRow = newFirstVisibleRow;
        currentLastVisibleRow = newLastVisibleRow;

        int newFirstVisibleStageIndex = currentFirstVisibleRow * numberOfColumns;
        int newLastVisibleStageIndex = (currentLastVisibleRow + 1) * numberOfColumns - 1;         

        // Create a temporary list to avoid modifying dictionary while iterating its keys
        List<int> activeStageIndexesToRemove = activeUIElements.Keys.ToList(); 
        foreach (int stageIndex in activeStageIndexesToRemove)
        {
            int row = stageIndex / numberOfColumns;
            if (row < newFirstVisibleRow || row > newLastVisibleRow)
            {
                StageUIElement uiElement = activeUIElements[stageIndex];
                objectPooler.ReturnPooledObject(uiElement.gameObject); // Return to central pooler
                activeUIElements.Remove(stageIndex);
            }
        }

        // Horizontal Lines
        List<int> activeHorLineRowsToRemove = activeHorLines.Keys.ToList();
        foreach (int row in activeHorLineRowsToRemove)
        {
            if (row < newFirstVisibleRow || row > newLastVisibleRow)
            {
                RectTransform line = activeHorLines[row];
                objectPooler.ReturnPooledObject(line.gameObject); // Return to central pooler
                activeHorLines.Remove(row);
            }
        }

        // Vertical Lines
        List<int> activeVerLineRowsToRemove = activeVerLines.Keys.ToList();
        foreach (int row in activeVerLineRowsToRemove)
        {
            // Note: Vertical lines are typically between rows, so newLastVisibleRow might need careful adjustment
            if (row < newFirstVisibleRow || row >= newLastVisibleRow) 
            {
                RectTransform line = activeVerLines[row];
                objectPooler.ReturnPooledObject(line.gameObject); // Return to central pooler
                activeVerLines.Remove(row);
            }
        }


        for (int row = newFirstVisibleRow; row <= newLastVisibleRow; row++)
        {
            if (row < 0 || row >= totalRowsInAllStages) continue;
            if (activeHorLines.ContainsKey(row)) continue;

            RectTransform horLineRect = GetOrCreateHorizontalLine();
            horLineRect.SetParent(contentRect.transform); 
            horLineRect.localScale = Vector3.one;
            horLineRect.anchorMin = new Vector2(0f, 0f);
            horLineRect.anchorMax = new Vector2(0f, 0f);
            horLineRect.pivot = new Vector2(0.5f, 0.5f); 

            float rowMidY = paddingBottom + row * (cellHeight + spacingY) + cellHeight / 2f;
            float horLinePosY = rowMidY + lineHorOffset;
            float horLineWidth = (cellWidth + spacingX) * numberOfColumns - spacingX; 
            float horLinePosX = paddingLeft + horLineWidth / 2f; 

            horLineRect.anchoredPosition = new Vector2(horLinePosX, horLinePosY);
            horLineRect.sizeDelta = new Vector2(horLineWidth, lineHorPrefab.GetComponent<RectTransform>().sizeDelta.y);
            horLineRect.gameObject.SetActive(true);
            horLineRect.SetAsFirstSibling(); // Ensure lines are rendered behind stages
            activeHorLines.Add(row, horLineRect);
        }

        // Vertical Lines (Add next to be rendered underneath stages)
        for (int row = newFirstVisibleRow; row < newLastVisibleRow; row++) // Loop up to newLastVisibleRow - 1 because lines are between rows
        {
            if (row < 0 || row >= totalRowsInAllStages - 1) continue; 
            if (activeVerLines.ContainsKey(row)) continue;

            RectTransform verLineRect = GetOrCreateVerticalLine();
            verLineRect.SetParent(contentRect.transform); 
            verLineRect.localScale = Vector3.one;
            verLineRect.anchorMin = new Vector2(0f, 0f);
            verLineRect.anchorMax = new Vector2(0f, 0f);
            verLineRect.pivot = new Vector2(0.5f, 0.5f); 

            float verLinePosX;
            if (row % 2 == 0) // Snake pattern for vertical lines based on row parity
            {
                verLinePosX = paddingLeft + (numberOfColumns - 1) * (cellWidth + spacingX) + cellWidth / 2f;
            }
            else 
            {
                verLinePosX = paddingLeft + cellWidth / 2f;
            }

            float verLineMidY = paddingBottom + (row + 1) * (cellHeight + spacingY) - spacingY / 2f;
            float verLinePosY = verLineMidY + lineVerOffset;
            
            verLineRect.anchoredPosition = new Vector2(verLinePosX, verLinePosY);
            float verLineHeight = cellHeight + spacingY; 
            verLineRect.sizeDelta = new Vector2(lineVerPrefab.GetComponent<RectTransform>().sizeDelta.x, verLineHeight);
            verLineRect.gameObject.SetActive(true);
            verLineRect.SetAsFirstSibling(); // Ensure lines are rendered behind stages
            activeVerLines.Add(row, verLineRect);
        }
        for (int i = newFirstVisibleStageIndex; i <= newLastVisibleStageIndex; i++)
        {
            if (i < 0 || i >= allStageData.Count) continue; 
            if (activeUIElements.ContainsKey(i)) continue; 

            StageUIElement uiElement = GetOrCreateUIElement();
            uiElement.transform.SetParent(contentRect.transform); 
            uiElement.transform.localScale = Vector3.one; 
            RectTransform elementRect = uiElement.GetComponent<RectTransform>();
            elementRect.anchorMin = new Vector2(0f, 0f); 
            elementRect.anchorMax = new Vector2(0f, 0f); 
            elementRect.pivot = new Vector2(0.5f, 0.5f); 
            
            int row = i / numberOfColumns;
            int col = i % numberOfColumns;
            float visualCol;
            if (row % 2 == 0) 
            {
                visualCol = col;
            }
            else 
            {
                visualCol = (numberOfColumns - 1) - col;
            }
            
            float basePosX = paddingLeft + visualCol * (cellWidth + spacingX);
            float basePosY = paddingBottom + row * (cellHeight + spacingY);
            float centeredPosX = basePosX + cellWidth / 2f;
            float centeredPosY = basePosY + cellHeight / 2f;
            
            elementRect.anchoredPosition = new Vector2(centeredPosX, centeredPosY);
            elementRect.sizeDelta = new Vector2(cellWidth, cellHeight); 
            uiElement.stageIndex = i; 
            uiElement.Init(allStageData[i]); 
            uiElement.gameObject.SetActive(true); 
            activeUIElements.Add(i, uiElement);
        }
        currentFirstVisibleRow = newFirstVisibleRow; 
        currentLastVisibleRow = newLastVisibleRow;    
    }

    StageUIElement GetOrCreateUIElement()
    {
        GameObject obj = objectPooler.GetPooledObject(stageUIElementPrefab.gameObject, Vector3.zero, Quaternion.identity, contentRect.transform);
        StageUIElement uiElement = obj.GetComponent<StageUIElement>();

        // Assign original prefab for tracking when returning to pool
        PooledObjectInfo info = obj.GetOrAddComponent<PooledObjectInfo>(); // Use GetOrAddComponent from Clouds.Ultilities
        info.originalPrefab = stageUIElementPrefab.gameObject;

        return uiElement;
    }

    RectTransform GetOrCreateHorizontalLine()
    {
        GameObject obj = objectPooler.GetPooledObject(lineHorPrefab, Vector3.zero, Quaternion.identity, contentRect.transform);
        RectTransform lineRect = obj.GetComponent<RectTransform>();

        // Assign original prefab for tracking when returning to pool
        PooledObjectInfo info = obj.GetOrAddComponent<PooledObjectInfo>();
        info.originalPrefab = lineHorPrefab;

        return lineRect;
    }

    RectTransform GetOrCreateVerticalLine()
    {
        GameObject obj = objectPooler.GetPooledObject(lineVerPrefab, Vector3.zero, Quaternion.identity, contentRect.transform);
        RectTransform lineRect = obj.GetComponent<RectTransform>();

        // Assign original prefab for tracking when returning to pool
        PooledObjectInfo info = obj.GetOrAddComponent<PooledObjectInfo>();
        info.originalPrefab = lineVerPrefab;

        return lineRect;
    }

    void ResetStageUI ()
    {
        int stage_unlocked = Random.Range(0, allStageData.Count);

        for (int i = 0; i < allStageData.Count; i++)
        {
            if (i < stage_unlocked)
            {
                allStageData[i].isLock = false;
                allStageData[i].StarGot = Random.Range(1, 4); // Random.Range(min, max) is exclusive for int max
            }
            else
            {
                allStageData[i].isLock = true;
                allStageData[i].StarGot = 0;
            }
        }
        stageDataControler.stageDynamicData.stageDatas = allStageData;
        stageDataControler.SaveData();

        // Update currently active UI elements to reflect new data
        foreach (var entry in activeUIElements)
        {
            int stageIndex = entry.Key;
            StageUIElement uiElement = entry.Value;
            if (stageIndex < allStageData.Count) // Ensure the index is still valid
            {
                uiElement.Init(allStageData[stageIndex]);
            }
        }
        UpdateVisibleStages();
    }
    
    /// <summary>
    /// Handles virtual updates from the SignalSystem.
    /// </summary>
    /// <param name="message">The SignalMessage received.</param>
    public override void UpdateVirtual(SignalMessage message)
    {
        base.UpdateVirtual(message);
        if(message.Type == SignalMessage.SignalType.Command1)
        {
           ResetStageUI();
        }
    }
}