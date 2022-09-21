using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildList_Popup : MonoBehaviour
{
    #region Fields

    private int childCount;
    [SerializeField] private float componentHeight = 550.0f;
    [SerializeField] private float childHeight = 150.0f;

    #endregion Fields

    #region Mono

    private void Awake()
    {
        childCount = transform.childCount;
        Debug.Log("BuildList button count: " + childCount);
        AdaptComponentHeight();
    }

    #endregion Mono

    #region Methods

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