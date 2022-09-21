using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RiverGenerator : MonoBehaviour
{
    public HexData RiverData;

    private Transform _target;


    [ContextMenu(itemName: "Start Searching")]
    public void StartSearching()
    {
        SearchForWater();

        if (_target != null)
        {
            StartSearchForPath();
        }
        else
        {
            SearchForOcean();

            if (_target != null)
            {

                StartSearchForPath();
            }
        }
    }

    private void SearchForOcean()
    {
        var hitCollides = Physics.OverlapSphere(transform.position, 10f);

        Collider best = null;
        var bestDistance = 9999f;

        foreach (var hitCollider in hitCollides)
        {
            if (hitCollider.tag != "water_ocean") continue;

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
    private void SearchForWater()
    {
        var hitCollides = Physics.OverlapSphere(transform.position, 10f);

        Collider bestWater = null;
        var bestWaterDistance = 9999f;

        Collider bestRiver = null;
        var bestRiverDistance = 9999f;

        foreach (var hitCollider in hitCollides)
        {
            if (Vector3.Distance(transform.position, hitCollider.transform.position) == 0)
                continue;

            if (hitCollider.tag == "river_start")
                continue;

            if (hitCollider.tag != "water" && !hitCollider.name.ToLower().Contains("river")) continue;

            if (hitCollider.name.ToLower().Contains("river"))
                if (hitCollider.transform.position.y < transform.position.y)
                {
                    var distanceRiver = Vector3.Distance(transform.position, hitCollider.transform.position);

                    if (distanceRiver < bestRiverDistance)
                    {
                        bestRiverDistance = distanceRiver;
                        bestRiver = hitCollider;
                    }
                    continue;
                }

            var distance = Vector3.Distance(transform.position, hitCollider.transform.position);

            if (distance < bestWaterDistance)
            {
                bestWaterDistance = distance;
                bestWater = hitCollider;
            }
        }

        if (bestRiver != null)
        {
            _target = bestRiver.transform;
            return;
        }

        if (bestWater == null)
            return;

        _target = bestWater.transform;

    }

    private List<Transform> _pathList = new List<Transform>();
    private List<Transform> _seenList = new List<Transform>();

    public void StartSearchForPath()
    {
        CastWaterFall(transform, Vector3.Distance(transform.position, _target.transform.position));

        if (_pathList.Count == 0)
        {
            _seenList.Clear();

            Debug.LogWarning("second cast" + transform.position + " " + _target.transform.position);

            CastWaterFall(transform, Vector3.Distance(transform.position, _target.transform.position), true);

            if (_pathList.Count == 0)
                return;
        }

        var go = Instantiate(RiverData.Prefab, transform.position, Quaternion.identity);
        go.transform.parent = transform.parent;

        var location = MapBuilder.Instance.GetTileLocationByPosition(new Vector2(go.transform.position.x, go.transform.position.z));
        MapBuilder.Instance.SetRiverLocation(go, location);

        foreach (var path in _pathList)
        {
            go = Instantiate(RiverData.Prefab, path.transform.position, Quaternion.identity);
            go.transform.parent = transform.parent;

            location = MapBuilder.Instance.GetTileLocationByPosition(new Vector2(go.transform.position.x, go.transform.position.z));
            MapBuilder.Instance.SetRiverLocation(go, location);

            Destroy(path.transform.parent.gameObject);
        }

        Destroy(transform.gameObject);
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

    public bool CastWaterFall(Transform parenTransform, float distance, bool direct = false)
    {
        var hitCollides = Physics.OverlapSphere(parenTransform.position, 1f).ToList();
        var bestPos = GetBestPositionCollider(hitCollides.ToArray());

        hitCollides.Remove(bestPos);

        if (bestPos.name == "water" || bestPos.name == "river")
            return true;

        if (!_seenList.Contains(bestPos.transform))
        {
            _seenList.Add(bestPos.transform);

            if (direct)
                if (Vector3.Distance(bestPos.transform.position, _target.transform.position) < distance)
                {
                    if (bestPos.transform.position.y < parenTransform.position.y)
                    {
                        if (CastWaterFall(bestPos.transform,
                            Vector3.Distance(bestPos.transform.position, _target.transform.position), true))
                        {
                            _pathList.Add(bestPos.transform);
                            return true;
                        }
                    }
                    else if (Math.Abs(bestPos.transform.position.y - parenTransform.position.y) < 0.01f)
                    {

                        if (CastWaterFall(bestPos.transform,
                            Vector3.Distance(bestPos.transform.position, _target.transform.position), true))
                        {
                            _pathList.Add(bestPos.transform);
                            return true;
                        }
                    }
                }
        }

        var rnd = new System.Random();
        var randomized = hitCollides.OrderBy(item => rnd.Next());


        foreach (var hitCollider in randomized)
        {

            if (hitCollider.name == "water" || hitCollider.name == "river")
                return true;

            if (_seenList.Contains(hitCollider.transform))
                continue;

            _seenList.Add(hitCollider.transform);

            if (Vector3.Distance(hitCollider.transform.position, _target.transform.position) >= distance)
                continue;

            if (hitCollider.transform.position.y < parenTransform.position.y)
            {
                if (CastWaterFall(hitCollider.transform, Vector3.Distance(hitCollider.transform.position, _target.transform.position), direct))
                {
                    _pathList.Add(hitCollider.transform);
                    return true;
                }
            }
            else if (Math.Abs(hitCollider.transform.position.y - parenTransform.position.y) < 0.01f)
            {

                if (CastWaterFall(hitCollider.transform, Vector3.Distance(hitCollider.transform.position, _target.transform.position), direct))
                {
                    _pathList.Add(hitCollider.transform);
                    return true;
                }
            }

        }

        return false;
    }

}
