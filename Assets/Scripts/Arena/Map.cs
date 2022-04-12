using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;

public class Map : MonoBehaviour
{

    [SerializeField]
    private int camFOV = 55;
    public int CamFOV
    {
        get { return camFOV; }
    }

    [SerializeField]
    private Transform[] spawns = new Transform[0];
    public int MaxPlayersCount
    {
        get { return spawns.Length; }
    }

    private bool[] availableSpawns;
    private NavMeshSurface navMesh;
    private bool initialized = false;

    void Init()
    {
        navMesh = gameObject.GetComponent<NavMeshSurface>();

        availableSpawns = new bool[spawns.Length];
        for (int i = 0; i < availableSpawns.Length; i++)
        {
            availableSpawns[i] = true;
        }

        initialized = true;
    }

    public Transform GetSpawnTransform()
    {
        if (!initialized)
            Init();
        List<int> emptys = new List<int>();
        for (int i = 0; i < availableSpawns.Length; i++)
        {
            if (availableSpawns[i])
                emptys.Add(i);
        }
        int index = emptys[Random.Range(0, emptys.Count)];
        availableSpawns[index] = false;
        return spawns[index];
    }

    public void BakeNavMesh()
    {
        navMesh.BuildNavMesh();
    }

}