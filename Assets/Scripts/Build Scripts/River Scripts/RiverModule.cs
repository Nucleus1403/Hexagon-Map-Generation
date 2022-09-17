using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

public class RiverModule : MonoBehaviour
{
    public RiverData RiverData;

    public List<GameObject> SpherePositions = new List<GameObject>();
    public GameObject InitialGameObject;

    private string _riverType;
    private int _edgeCount;

    public void Start()
    {
        CreateModule();
    }

    [ContextMenu(itemName: "CreateModule")]
    private void CreateModule()
    {
        SearchForEdges();
        SearchForModuleType();
    }

    private void SearchForModuleType()
    {
        foreach (var type in RiverData.Models)
        {
            if (type.EdgeCount != _edgeCount)
                continue;

            var result = CheckPermutations(type.Type);
            if (result == -1)
                continue;


            var gameObj = Instantiate(type.Prefab, transform.position, Quaternion.identity);

            gameObj.transform.eulerAngles = new Vector3(gameObj.transform.eulerAngles.x, 60 * result, gameObj.transform.eulerAngles.z);
            gameObj.transform.parent = transform;
            gameObj.transform.localPosition = Vector3.zero;

            Invoke(nameof(DisableGo), 0.5f);
        }
    }

    private void DisableGo()
    {
        InitialGameObject.SetActive(false);

        foreach (var s in SpherePositions)
        {
            s.SetActive(false);
        }
    }

    private int CheckPermutations(string moduleType)
    {
        if (moduleType == _riverType)
            return 0;

        for (var index = 1; index < moduleType.Length; index++)
        {
            var result = string.Join("", moduleType.Skip(1)) + moduleType[0];

            if (result == _riverType)
                return 6 - index;

            moduleType = result;
        }

        return -1;
    }


    private void SearchForEdges()
    {
        bool conectedToWater = false;
        foreach (var sphere in SpherePositions)
        {
            var hit = Physics.OverlapSphere(sphere.transform.position, 0.4f);
            var data = 0;

            foreach (var collider in hit)
            {
                if (collider.tag == "water_ocean")
                {
                    if (conectedToWater == false)
                        data = 1;

                    conectedToWater = true;
                    break;
                }

                if (collider.tag == "water")
                {
                    if (conectedToWater == false)
                        data = 1;

                    conectedToWater = true;

                    break;
                }

                if (collider.tag == "river")
                {
                    data = 1;
                    break;
                }
            }

            _edgeCount += data;
            _riverType = string.Concat(_riverType, data);
        }

    }
}

