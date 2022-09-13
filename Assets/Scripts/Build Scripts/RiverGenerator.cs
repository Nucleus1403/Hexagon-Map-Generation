using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RiverGenerator : MonoBehaviour
{
    public bool DrawGizmoRadius = false;

    public HexData RiverData;

    private Transform _target;

    [ContextMenu(itemName: "Start Searching")]
    public void StartSearching()
    {
        SearchForWater();

        if (_target)
        {
            StartSearchForPath();
        }
    }

    private void SearchForWater()
    {
        var hitCollides = Physics.OverlapSphere(transform.position, 10f);

        Collider best = null;
        var bestDistance = 9999f;

        foreach (var hitCollider in hitCollides)
        {
            if (hitCollider.name != "water") continue;

            var distance = Vector3.Distance(transform.position, hitCollider.transform.position);

            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = hitCollider;
            }
        }

        if (best == null) return;

        _target = best.transform;

    }

    private List<Transform> _pathList = new List<Transform>();
    private List<Transform> _seenList = new List<Transform>();

    public void StartSearchForPath()
    {
        CastWaterFall(transform, Vector3.Distance(transform.position, _target.transform.position));

        if (_pathList.Count == 0)
            return;

        Instantiate(RiverData.Prefab, transform.position, Quaternion.identity);

        foreach (var path in _pathList)
        {
            Instantiate(RiverData.Prefab, path.transform.position, Quaternion.identity);
            Destroy(path.transform.parent.gameObject);
        }

        Destroy(transform.parent.gameObject);
    }

    public Collider GetBestPositionCollider(Collider[] colliders)
    {
        Collider bestCollider = null;
        float bestDistance = 9999;

        foreach (var collider in colliders)
        {
            var dist = Vector3.Distance(collider.transform.position, _target.transform.position);

            if (dist < bestDistance)
            {
                bestDistance = dist;
                bestCollider = collider;
            }
        }

        return bestCollider;
    }

    public bool CastWaterFall(Transform parenTransform, float distance)
    {
        var hitCollides = Physics.OverlapSphere(parenTransform.position, 1f).ToList();
        var bestPos = GetBestPositionCollider(hitCollides.ToArray());

        hitCollides.Remove(bestPos);

        if (bestPos.name == "water")
            return true;

        if (!_seenList.Contains(bestPos.transform))
        {
            _seenList.Add(bestPos.transform);

            if (Vector3.Distance(bestPos.transform.position, _target.transform.position) < distance)
            {
                if (bestPos.transform.position.y < parenTransform.position.y)
                {
                    if (CastWaterFall(bestPos.transform,
                        Vector3.Distance(bestPos.transform.position, _target.transform.position)))
                    {
                        _pathList.Add(bestPos.transform);
                        return true;
                    }
                }
                else if (Math.Abs(bestPos.transform.position.y - parenTransform.position.y) < 0.01f)
                {

                    if (CastWaterFall(bestPos.transform,
                        Vector3.Distance(bestPos.transform.position, _target.transform.position)))
                    {
                        _pathList.Add(bestPos.transform);
                        return true;
                    }
                }
            }
        }

        foreach (var hitCollider in hitCollides)
        {
            if (hitCollider.name == "water")
                return true;

            if (_seenList.Contains(hitCollider.transform))
                continue;

            _seenList.Add(hitCollider.transform);

            if (Vector3.Distance(hitCollider.transform.position, _target.transform.position) >= distance)
                continue;

            if (hitCollider.transform.position.y < parenTransform.position.y)
            {
                if (CastWaterFall(hitCollider.transform, Vector3.Distance(hitCollider.transform.position, _target.transform.position)))
                {
                    _pathList.Add(hitCollider.transform);
                    return true;
                }
            }
            else if (Math.Abs(hitCollider.transform.position.y - parenTransform.position.y) < 0.01f)
            {

                if (CastWaterFall(hitCollider.transform, Vector3.Distance(hitCollider.transform.position, _target.transform.position)))
                {
                    _pathList.Add(hitCollider.transform);
                    return true;
                }
            }

        }

        return false;
    }

    public void OnDrawGizmos()
    {
        if (DrawGizmoRadius)
            Gizmos.DrawSphere(transform.position, 1f);
    }
}
