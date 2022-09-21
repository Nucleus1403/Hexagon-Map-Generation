using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildList_Popup : MonoBehaviour
{
    #region Fields

    private int childCount;
    [SerializeField] private float componentHeight = 550.0f;
    [SerializeField] private float childHeight = 150.0f;

    [SerializeField] private Button buildingButtonPrefab;

    #endregion Fields

    #region Mono

    private void Awake()
    {
        
    }

    #endregion Mono

    #region Methods

    public void Initialize(List<HexData> AcceptedHex)
    {
        foreach (var item in AcceptedHex)
        {
            Instantiate(buildingButtonPrefab);
            buildingButtonPrefab.GetComponentInChildren<Image>().sprite = item.Sprite;
            buildingButtonPrefab.transform.parent = this.transform;
        }
        AdaptComponentHeight();
    }

    private void AdaptComponentHeight()
    {
        childCount = transform.childCount;
        Debug.Log("BuildList button count: " + childCount);
        if (childCount > 3)
        {
            this.gameObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (componentHeight + childHeight * childCount + 20 * (childCount + 1)));
        }
    }

    #endregion Methods
}