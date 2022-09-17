using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    #region Fields

    public static UIManager Instance;

    public List<Popup> UIPopup = new List<Popup>();

    #endregion Fields

    #region Mono

    private void Awake()
    {
        Instance = this;
    }

    #endregion Mono

    #region Methods

    public GameObject ShowPopup(PopupNames name)
    {
        foreach (var item in UIPopup)
        {
            if (name == item.PopupName)
            {
                if (!item.Prefab.activeInHierarchy) 
                    item.Prefab.SetActive(true);   // Auto in anim

                return item.Prefab;
            }
        }
        return null;
    }

    public GameObject HidePopup(PopupNames name)
    {
        foreach (var item in UIPopup)
        {
            if (name == item.PopupName)
            {
                // Need time for out anim
                item.Prefab.SetActive(false);
                return item.Prefab;
            }
        }
        return null;
    }

    #endregion Methods
}

public enum PopupNames
{
    BuildList_Popup,
    BuildInfo_Popup
}

[Serializable]   // this made the class as a struct
public class Popup
{
    public GameObject Prefab;
    public PopupNames PopupName;
}