using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;

public class Map : MonoBehaviour {

    [SerializeField]
    private Transform[] spawns = new Transform[0];
    public int MaxPlayersCount {
        get { return spawns.Length;}
    }
    //ressources sur la map ? 


    private bool[] availableSpawns;
    private NavMeshSurface navMesh;

    void Start(){
        navMesh = gameObject.GetComponent<NavMeshSurface>();

        availableSpawns = new bool[spawns.Length];
        for (int i = 0; i < availableSpawns.Length;i++){
            availableSpawns[i] = true;
        }
    }

    public Transform GetSpawnTransform(){
        List<int> emptys = new List<int>();
        for (int i = 0; i < availableSpawns.Length;i++){
            if (availableSpawns[i])
                emptys.Add(i);
        }
        int index = emptys[Random.Range(0,emptys.Count)];
        availableSpawns[index] = false;
        return spawns[index];
    }

    public void BakeNavMesh(){
        navMesh.BuildNavMesh();
    }
    
}