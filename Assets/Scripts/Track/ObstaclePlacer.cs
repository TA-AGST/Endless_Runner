using System.Collections.Generic;
using UnityEngine;

// Decides where obstacles go. It places "rows" (all three lanes at one distance) along the road.
// Every row can be passed: there is always a free lane, or something to jump over or slide under.
// The next row keeps its free lane close to the last one and is far enough away to reach it, so the game is fair.
public class ObstaclePlacer : MonoBehaviour
{
    [SerializeField] private Obstacle lowBarrier;
    [SerializeField] private Obstacle highBeam;
    [SerializeField] private Obstacle wall;
    [SerializeField] private float laneWidth = 2.6f;
    [SerializeField] private float firstRowDistance = 50f;     // the start of the road is empty
    [SerializeField] private float minSecondsBetweenRows = 1f;

    private enum Pattern { Single, TwoWalls, FullLow, FullBeam, WallsAndLow, BeamsAndWall }

    private float nextRowZ;
    private int lastOpenLane;

    private void Awake()
    {
        nextRowZ = firstRowDistance;
    }

    // Fills a chunk with the rows that fall inside it.
    public void Populate(TrackChunk chunk)
    {
        GameManager game = GameManager.Instance;
        float difficulty = game.Difficulty;

        while (nextRowZ < chunk.EndZ)
        {
            if (nextRowZ >= chunk.StartZ) PlaceRow(chunk, nextRowZ, difficulty);

            // the faster we run, the more road we need between two rows to react
            float reaction = (game.Speed + 4f) * minSecondsBetweenRows;
            float gap = Mathf.Max(reaction, Mathf.Lerp(32f, 18f, difficulty)) + Random.Range(0f, 8f) * (1f - difficulty);
            nextRowZ += gap;
        }
    }

    private void PlaceRow(TrackChunk chunk, float z, float difficulty)
    {
        switch (PickPattern(difficulty))
        {
            case Pattern.Single: SingleObstacle(chunk, z); break;
            case Pattern.TwoWalls: TwoWalls(chunk, z); break;
            case Pattern.FullLow: AllLanes(chunk, lowBarrier, z); break;
            case Pattern.FullBeam: AllLanes(chunk, highBeam, z); break;
            case Pattern.WallsAndLow: WallsAndLow(chunk, z); break;
            case Pattern.BeamsAndWall: BeamsAndWall(chunk, z); break;
        }
    }

    private Pattern PickPattern(float d)
    {
        // harder patterns appear more often as the difficulty grows
        var weights = new List<(Pattern pattern, float weight)>
        {
            (Pattern.Single, 3f),
            (Pattern.FullLow, 2f),
            (Pattern.FullBeam, 2f),
            (Pattern.TwoWalls, 1f + 2f * d),
            (Pattern.WallsAndLow, d > 0.2f ? 2f * d : 0f),
            (Pattern.BeamsAndWall, d > 0.3f ? 1.5f * d : 0f),
        };

        float total = 0f;
        foreach (var w in weights) total += w.weight;

        float roll = Random.value * total;
        foreach (var w in weights)
        {
            roll -= w.weight;
            if (roll <= 0f) return w.pattern;
        }
        return Pattern.Single;
    }

    // ---------------- patterns ----------------

    private void SingleObstacle(TrackChunk chunk, float z)
    {
        int lane = Random.Range(-1, 2);
        Obstacle kind = Random.value < 0.5f ? lowBarrier : wall;
        Spawn(chunk, kind, lane, z);

        // the player will most likely stand next to it
        if (lane == lastOpenLane) lastOpenLane = Mathf.Clamp(lane + (lane == 1 ? -1 : 1), -1, 1);
    }

    private void TwoWalls(TrackChunk chunk, float z)
    {
        int open = NextOpenLane();
        for (int lane = -1; lane <= 1; lane++) if (lane != open) Spawn(chunk, wall, lane, z);
    }

    private void AllLanes(TrackChunk chunk, Obstacle kind, float z)
    {
        for (int lane = -1; lane <= 1; lane++) Spawn(chunk, kind, lane, z);
    }

    // one lane has a low barrier (jump), the other two are walls
    private void WallsAndLow(TrackChunk chunk, float z)
    {
        int open = NextOpenLane();
        for (int lane = -1; lane <= 1; lane++) Spawn(chunk, lane == open ? lowBarrier : wall, lane, z);
    }

    // a wall in one lane, beams (slide) in the other two
    private void BeamsAndWall(TrackChunk chunk, float z)
    {
        // the wall never goes where the player is most likely standing
        int wallLane;
        do { wallLane = Random.Range(-1, 2); } while (wallLane == lastOpenLane);

        for (int lane = -1; lane <= 1; lane++) Spawn(chunk, lane == wallLane ? wall : highBeam, lane, z);
    }

    // a free lane no more than one step away from the last free lane
    private int NextOpenLane()
    {
        int open = Mathf.Clamp(lastOpenLane + Random.Range(-1, 2), -1, 1);
        lastOpenLane = open;
        return open;
    }

    private void Spawn(TrackChunk chunk, Obstacle prefab, int lane, float z)
    {
        Obstacle o = Instantiate(prefab, chunk.ObstacleRoot);
        o.transform.position = new Vector3(lane * laneWidth, 0f, z);
    }
}
