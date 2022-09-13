using System.Collections.Generic;
using UnityEngine;

public class WaterMeshCombiner : MonoBehaviour
{
    [SerializeField] private List<MeshFilter> _sourceMeshFilters;
    [SerializeField] private MeshFilter _targetMeshFilter;

    [ContextMenu(itemName: "Combine Meshes")]
    public void CombineMeshes()
    {
        var combine = new CombineInstance[_sourceMeshFilters.Count];
        for (var i = 0; i < _sourceMeshFilters.Count; i++)
        {
            //_sourceMeshFilters[i].transform.parent.gameObject.SetActive(false);

            combine[i].mesh = _sourceMeshFilters[i].sharedMesh;
            combine[i].transform = _sourceMeshFilters[i].transform.localToWorldMatrix;

            Destroy(_sourceMeshFilters[i].transform.parent.gameObject);
        }
        
        var mesh = new Mesh
        {
            indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
        };
        mesh.CombineMeshes(combine);
        _targetMeshFilter.mesh = mesh;
    }

    public void AddMeshes(MeshFilter meshFilter)
    {
        _sourceMeshFilters.Add(meshFilter);
    }

    public void RemoveAllMeshes()
    {
        _sourceMeshFilters.Clear();
    }
}
