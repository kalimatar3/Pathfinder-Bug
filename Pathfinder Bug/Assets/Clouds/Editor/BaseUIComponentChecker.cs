using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public static class BaseUIComponentChecker
{
    static BaseUIComponentChecker()
    {
        // Hook vào sự kiện thêm component trong Editor
        ObjectFactory.componentWasAdded += OnComponentAdded;
    }

    private static void OnComponentAdded(Component component)
    {
        if (component == null) return;

        var baseUI = component as baseUI;
        if (baseUI != null)
        {
            GameObject go = baseUI.gameObject;

            // Nếu chưa có UniqueIDComponent, thì thêm vào
            if (go.GetComponent<UniqueIDComponent>() == null)
            {
                go.GetComponent<baseUI>().idComponent =  Undo.AddComponent<UniqueIDComponent>(go); 
                
            }
        }
    }
}
