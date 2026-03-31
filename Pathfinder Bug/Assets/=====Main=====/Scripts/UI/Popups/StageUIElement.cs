using UnityEngine;
using System.Collections.Generic; 
using TMPro;
using UnityEngine.SceneManagement;

public class StageUIElement : BaseButton, IPoolable 
{
    [HideInInspector] 
    public int stageIndex; 
    [SerializeField] protected List<GameObject> Stars;
    [SerializeField] protected TextMeshProUGUI StageName_Text;
    [SerializeField] protected GameObject Lock; 
    [SerializeField] protected GameObject Tut;
    protected IMenuController menuController;
    public GameObject GameObject => this.gameObject;
    protected void Start()
    {
        menuController = MenuManager.Instance;
    }
    public void OnGetFromPool()
    {
        ResetUI(); 
        gameObject.SetActive(true); // Ensure the GameObject is active when taken from pool
    }

    public void OnReturnToPool()
    {
        gameObject.SetActive(false); 
    }
    public void Init(StageData data)
    {
        this.stageIndex = data.stageindex; // Store the index of the current stage
        this.SetStars(data.StarGot); // Update the number of stars
        
        // Use stageName from StageListAsset.StageData
        if(data.stageindex == 0)
        {
            StageName_Text.gameObject.SetActive(false);
            Tut.gameObject.SetActive(true);
        }
        else
        {
            StageName_Text.gameObject.SetActive(true);
            Tut.gameObject.SetActive(false);            
        }
        if (StageName_Text != null) 
        {
            StageName_Text.text = (data.stageindex +1).ToString();
        }
        
        // Update lock state
        if (Lock != null)
        {
            Lock.gameObject.SetActive(data.isLock);
        }
    }

    protected void SetStars(int stargots)
    {
        for(int i = 0; i < Stars.Count;i++)
        {
            if(Stars[i] != null) // Ensure the star object is not null
            {
                Stars[i].gameObject.SetActive(i < stargots);
            }
        }
    }

    public void ResetUI() 
    {
        if (StageName_Text != null) StageName_Text.text = string.Empty;
        if (Lock != null) Lock.gameObject.SetActive(false);
        foreach (var star in Stars)
        {
            if (star != null) star.gameObject.SetActive(false);
        }
        stageIndex = -1; // Set index to an invalid value
    }

    protected override void OnButtonClicked()
    {
        base.OnButtonClicked();
       menuController.Play();
    }
}