using System.Collections.Generic;
using UnityEngine;

// Builds the endless road: a few chunks in front of the player, and a chunk that is behind him
// is moved to the front again (so no new objects are created while running).
public class TrackGenerator : MonoBehaviour
{
    [SerializeField] private TrackChunk chunkPrefab;
    [SerializeField] private ObstaclePlacer placer;
    [SerializeField] private Transform player;
    [SerializeField] private int chunksAhead = 8;
    [SerializeField] private float removeDistanceBehind = 25f;

    private readonly Queue<TrackChunk> chunks = new Queue<TrackChunk>();
    private float nextZ;

    private void Start()
    {
        float length = chunkPrefab.Length;
        nextZ = -length;

        // one chunk behind the player and enough in front of him
        for (int i = 0; i < chunksAhead + 1; i++)
        {
            TrackChunk chunk = Instantiate(chunkPrefab, transform);
            Place(chunk);
            chunks.Enqueue(chunk);
        }
    }

    private void Update()
    {
        while (chunks.Peek().EndZ < player.position.z - removeDistanceBehind)
        {
            TrackChunk chunk = chunks.Dequeue();
            chunk.ClearObstacles();
            Place(chunk);
            chunks.Enqueue(chunk);
        }
    }

    private void Place(TrackChunk chunk)
    {
        chunk.transform.position = new Vector3(0f, 0f, nextZ);
        nextZ += chunk.Length;

        chunk.Randomize();
        placer.Populate(chunk);
    }
}
