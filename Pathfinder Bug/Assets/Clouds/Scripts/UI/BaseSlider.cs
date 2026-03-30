using UnityEngine;
using UnityEngine.UI;
public abstract class BaseSlider : baseUI
{
    [SerializeField] protected Slider slider;
    public float value;
    protected void LoadSlider()
    {
        Slider slider =  GetComponent<Slider>();
        if(slider == null)
        {
            Debug.LogWarning(this.transform.name + " Cant Found Slider");
            return;
        }
        this.slider = slider;
    }
    public virtual void SetvalueSlider(float number)
    {
        this.value = number;
    }
    public virtual float getvalueSlider()
    {
        return this.slider.value;
    }
}
