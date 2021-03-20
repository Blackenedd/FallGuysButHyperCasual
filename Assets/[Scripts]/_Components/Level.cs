using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    [SerializeField] private Transform spawnPointContainer;

    private List<Transform> spawnPositions = new List<Transform>();

    public static Level instance = null;
    private void Awake()
    {
        if (instance == null) instance = this;
        for (int i = 0; i < spawnPointContainer.childCount; i++) { spawnPositions.Add(spawnPointContainer.GetChild(i)); }
    }
    public Transform GetSpawnPoint(Vector3 _position)
    {
        Transform point = null;
        float closestDistance = Mathf.Infinity;
        
        foreach(Transform tr in spawnPositions)
        {
            if (_position.z > tr.position.z)
            {
                float currentDistance = Vector3.Distance(tr.position, _position);
                if (currentDistance < closestDistance)
                {
                    closestDistance = currentDistance;
                    point = tr;
                }
            }
        }
        return point != null ? point : spawnPositions[0];
    }
}
