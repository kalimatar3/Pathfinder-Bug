using UnityEngine;
using System.Collections.Generic; 
using System.Linq;
using UnityEngine.UI;
using Clouds.SignalSystem; 

public class StagePopup : BasePopup
{
    [Header("Prefabs")]
    [SerializeField] protected StageUIElement stageUIElementPrefab;
    [SerializeField] protected GameObject lineHorPrefab; 
    [SerializeField] protected GameObject lineVerPrefab; 
    [Header("Components")]
    [SerializeField] protected ScrollRect stageScrollRect;
    [SerializeField] protected RectTransform contentRect; 
    [SerializeField] protected GridLayoutGroup gridLayoutGroup; 
    [SerializeField] protected RectTransform viewportRect; 
    [Header("Data")]
    [SerializeField] private StageListAsset stageListAsset; 
    [SerializeField] protected float lineHorOffset = 0f; // Y offset for horizontal line (from cell center)
    [SerializeField] protected float lineVerOffset = 0f; // Y offset for vertical line (from the middle of the gap between 2 rows)
    IStageDataControler stageDataControler; 
    [Header("Pooling Settings")]
    public int initialPoolSize = 64; 
    
    private List<StageData> allStageData; 
    private Queue<StageUIElement> inactiveUIElementsPool;
    private Dictionary<int, StageUIElement> activeUIElements; 

    // Pools and Dictionaries for lines
    private Queue<RectTransform> inactiveHorLinesPool;
    private Dictionary<int, RectTransform> activeHorLines; // Key: row index
    private Queue<RectTransform> inactiveVerLinesPool;
    private Dictionary<int, RectTransform> activeVerLines; // Key: row index (the row from which the vertical line STARTS)

    // Layout parameters will be read from GridLayoutGroup and then used manually
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
        base.Awake();
    }
    protected void Start()
    {
        stageDataControler = DataManager.Instance;
        allStageData = stageDataControler.stageDynamicData.stageDatas;
        SetupLayoutMetrics();
        InitializePool();
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

        gridLayoutGroup.enabled = false; // Disable GridLayoutGroup component at runtime

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

    void InitializePool()
    {
        inactiveUIElementsPool = new Queue<StageUIElement>();
        activeUIElements = new Dictionary<int, StageUIElement>();

        inactiveHorLinesPool = new Queue<RectTransform>();
        activeHorLines = new Dictionary<int, RectTransform>();
        inactiveVerLinesPool = new Queue<RectTransform>();
        activeVerLines = new Dictionary<int, RectTransform>();

        // Pool Horizontal Lines (spawn first to be rendered underneath)
        for (int i = 0; i < visibleRows + 2; i++) 
        {
            GameObject lineObj = Instantiate(lineHorPrefab, contentRect.transform); // Child of contentRect
            lineObj.SetActive(false);
            inactiveHorLinesPool.Enqueue(lineObj.GetComponent<RectTransform>());
        }

        // Pool Vertical Lines (spawn first to be rendered underneath)
        for (int i = 0; i < visibleRows + 2; i++) 
        {
            GameObject lineObj = Instantiate(lineVerPrefab, contentRect.transform); // Child of contentRect
            lineObj.SetActive(false);
            inactiveVerLinesPool.Enqueue(lineObj.GetComponent<RectTransform>());
        }

        // Pool StageUIElements (spawn last to be rendered on top)
        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject uiObj = Instantiate(stageUIElementPrefab.gameObject, contentRect.transform);
            StageUIElement uiElement = uiObj.GetComponent<StageUIElement>();
            uiElement.gameObject.SetActive(false); 
            uiElement.ResetUI(); 
            inactiveUIElementsPool.Enqueue(uiElement);
        }
    }

    void OnScroll(Vector2 normalizedPosition)
    {
        UpdateVisibleStages();
    }

    void UpdateVisibleStages()
    {
        float currentScrollY = contentRect.anchoredPosition.y;

        float offsetFromContentBottom = -currentScrollY + paddingBottom; 
        
        int newFirstVisibleRow = Mathf.FloorToInt(offsetFromContentBottom / (cellHeight + spacingY));
        
        newFirstVisibleRow = Mathf.Max(0, newFirstVisibleRow - 1); 

        int newLastVisibleRow = Mathf.Min(
            totalRowsInAllStages - 1, 
            newFirstVisibleRow + visibleRows 
        );

        if (newFirstVisibleRow == currentFirstVisibleRow && newLastVisibleRow == currentLastVisibleRow)
        {
            return;
        }

        currentFirstVisibleRow = newFirstVisibleRow;
        currentLastVisibleRow = newLastVisibleRow;

        int newFirstVisibleStageIndex = currentFirstVisibleRow * numberOfColumns;
        int newLastVisibleStageIndex = (currentLastVisibleRow + 1) * numberOfColumns - 1;         
        // --- REMOVE & POOL OUT elements not in view ---

        // StageUIElements
        var activeStageIndexes = activeUIElements.Keys.ToList(); 
        foreach (int stageIndex in activeStageIndexes)
        {
            int row = stageIndex / numberOfColumns;
            if (row < newFirstVisibleRow || row > newLastVisibleRow)
            {
                StageUIElement uiElement = activeUIElements[stageIndex];
                uiElement.gameObject.SetActive(false);
                uiElement.ResetUI();
                inactiveUIElementsPool.Enqueue(uiElement);
                activeUIElements.Remove(stageIndex);
            }
        }

        // Horizontal Lines
        var activeHorLineRows = activeHorLines.Keys.ToList();
        foreach (int row in activeHorLineRows)
        {
            if (row < newFirstVisibleRow || row > newLastVisibleRow)
            {
                RectTransform line = activeHorLines[row];
                line.gameObject.SetActive(false);
                inactiveHorLinesPool.Enqueue(line);
                activeHorLines.Remove(row);
            }
        }

        // Vertical Lines
        var activeVerLineRows = activeVerLines.Keys.ToList();
        foreach (int row in activeVerLineRows)
        {
            if (row < newFirstVisibleRow || row >= newLastVisibleRow) 
            {
                RectTransform line = activeVerLines[row];
                line.gameObject.SetActive(false);
                inactiveVerLinesPool.Enqueue(line);
                activeVerLines.Remove(row);
            }
        }


        // --- ADD & POOL IN elements that are now in view ---

        // Horizontal Lines (Add first to be rendered underneath stages)
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
            // Line height taken from prefab, width calculated
            horLineRect.sizeDelta = new Vector2(horLineWidth, lineHorPrefab.GetComponent<RectTransform>().sizeDelta.y);
            horLineRect.gameObject.SetActive(true);
            horLineRect.SetAsFirstSibling(); 
            activeHorLines.Add(row, horLineRect);
        }

        // Vertical Lines (Add next to be rendered underneath stages)
        for (int row = newFirstVisibleRow; row < newLastVisibleRow; row++) 
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
            if (row % 2 == 0) 
            {
                verLinePosX = paddingLeft + (numberOfColumns - 1) * (cellWidth + spacingX) + cellWidth / 2f;
            }
            else 
            {
                verLinePosX = paddingLeft + cellWidth / 2f;
            }

            // Recalculate Y Position to match line height from prefab
            float verLineMidY = paddingBottom + (row + 1) * (cellHeight + spacingY) - spacingY / 2f;
            float verLinePosY = verLineMidY + lineVerOffset;
            
            verLineRect.anchoredPosition = new Vector2(verLinePosX, verLinePosY);
            // sizeDelta line removed to maintain prefab size
            verLineRect.gameObject.SetActive(true);
            verLineRect.SetAsFirstSibling(); 
            activeVerLines.Add(row, verLineRect);
        }

        // StageUIElements (Snake pattern positioning) (Add last to be rendered on top)
        for (int i = newFirstVisibleStageIndex; i <= newLastVisibleStageIndex; i++)
        {
            if (i < 0 || i >= allStageData.Count) continue; 
            if (activeUIElements.ContainsKey(i)) continue; 

            StageUIElement uiElement = GetOrCreateUIElement();
            uiElement.transform.SetParent(contentRect.transform); 
            uiElement.transform.localScale = Vector3.one; 
            RectTransform elementRect = uiElement.GetComponent<RectTransform>();

            // Keep AnchorMin and AnchorMax as is
            elementRect.anchorMin = new Vector2(0f, 0f); 
            elementRect.anchorMax = new Vector2(0f, 0f); 
            
            // Change Pivot to center (0.5, 0.5)
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
        if (inactiveUIElementsPool.Count > 0)
        {
            return inactiveUIElementsPool.Dequeue();
        }
        else
        {
            GameObject uiObj = Instantiate(stageUIElementPrefab.gameObject, contentRect.transform);
            StageUIElement uiElement = uiObj.GetComponent<StageUIElement>();
            return uiElement;
        }
    }

    RectTransform GetOrCreateHorizontalLine()
    {
        if (inactiveHorLinesPool.Count > 0)
        {
            return inactiveHorLinesPool.Dequeue();
        }
        else
        {
            Debug.LogWarning("Pool ran out of Horizontal Line elements. Consider increasing initial pool size.");
            GameObject lineObj = Instantiate(lineHorPrefab, contentRect.transform); // Parent is contentRect
            return lineObj.GetComponent<RectTransform>();
        }
    }

    RectTransform GetOrCreateVerticalLine()
    {
        if (inactiveVerLinesPool.Count > 0)
        {
            return inactiveVerLinesPool.Dequeue();
        }
        else
        {
            Debug.LogWarning("Pool ran out of Vertical Line elements. Consider increasing initial pool size.");
            GameObject lineObj = Instantiate(lineVerPrefab, contentRect.transform); // Parent is contentRect
            return lineObj.GetComponent<RectTransform>();
        }
    }
    void ResetStageUI ()
    {
        stageDataControler.stageDynamicData.stageDatas = stageDataControler.RandomStageData();
        stageDataControler.SaveData();
        
        allStageData = stageDataControler.stageDynamicData.stageDatas;
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
    public override void UpdateVirtual(SignalMessage message)
    {
        base.UpdateVirtual(message);
        if(message.Type == SignalMessage.SignalType.Command1)
        {
           ResetStageUI();
        }
    }
}