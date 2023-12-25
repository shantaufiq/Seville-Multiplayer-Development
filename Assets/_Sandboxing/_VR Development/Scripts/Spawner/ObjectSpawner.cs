using Fusion;
using UnityEngine;

public class ObjectSpawner : NetworkObject
{
    public NetworkObject objectPrefab;
    public Transform spawnPossition;

    private void Start()
    {
        SpawnObject();
    }

    public void SpawnObject()
    {
        this.GetComponent<NetworkObject>().Runner.Spawn(objectPrefab, Utils.GetRandomSpawnPoint(spawnPossition));
    }
}