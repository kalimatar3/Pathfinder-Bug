using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Clouds.Ultilities;
public class PanelManager : Singleton<PanelManager>
{
    [SerializeField] protected List<BasePopup> ListPanels;
    public string Curpanel;
    protected virtual void LoadComponents()
    {
        this.LoadListpanel();
    }
    protected void Start() {
        Application.targetFrameRate = 60;
    }
    public List<BasePopup> getListPanels()
    {
        return this.ListPanels;
    }
    protected void LoadListpanel()
    {
        if (ListPanels.Count > 0) return;
        foreach (Transform panel in transform) 
        {
            if(panel.GetComponent<BasePopup>()) ListPanels.Add(panel.GetComponent<BasePopup>());
        }
    }
    protected void OnEnable() {
        this.Curpanel = ListPanels[0].name; 
    }
    public void ReturntoMainMenu()
    {
        this.DeActiveAll();
        this.GetPanelbyName(ListPanels[0].name).gameObject.SetActive(true);
    }
    public void DeActiveAll()
    {
        foreach (var ele in ListPanels)
        {
            ele.gameObject.SetActive(false);
        }
    }
    public Transform GetPanelbyName(string panelname) {
        this.Curpanel = panelname;
        foreach(var ele in ListPanels)
        {
            if (ele.name == panelname) return ele.transform;
        }
        Debug.Log(this.transform.name + "Cant found " + panelname);
        return null;
    }
    public Transform DeActivePanel(string panelname)
    {
        foreach (var ele in ListPanels)
        {
            if (ele.name == panelname)
            {
                ele.gameObject.SetActive(false);
                return ele.transform;
            }
        }
        Debug.Log(this.transform.name + "Cant found " + panelname);
        return null;
    }
}
