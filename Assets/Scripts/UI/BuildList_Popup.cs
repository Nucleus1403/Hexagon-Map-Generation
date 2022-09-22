using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class BuildList_Popup : MonoBehaviour
{
    #region Fields

    [SerializeField] private float componentHeight = 550.0f;
    [SerializeField] private Button buildingButtonPrefab;
    [SerializeField] private GameObject buildingListContent;

    private List<int> activeBuildingButtonTypes = new List<int>();

    [SerializeField] private HexData testHexdata;   // Testing

    #endregion Fields

    #region Mono

    [ContextMenu("TEST")]
    private void TEST()
    {
        activeBuildingButtonTypes.Add((int)testHexdata.Type);

        Instantiate(buildingButtonPrefab, buildingListContent.transform);
        buildingButtonPrefab.GetComponentInChildren<Image>().sprite = testHexdata.Sprite;
    }

    #endregion Mono

    #region Methods

    public void Initialize(List<HexData> AcceptedHex)
    {
        foreach (var item in AcceptedHex)
        {
            activeBuildingButtonTypes.Add((int)item.Type);

            Instantiate(buildingButtonPrefab, buildingListContent.transform);
            buildingButtonPrefab.GetComponentInChildren<Image>().sprite = item.Sprite;
        }
    }

    #endregion Methods
}