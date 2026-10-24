using UnityEngine;

public enum ObstacleKind
{
    LowBarrier,     // jump over it
    HighBeam,       // slide under it
    Wall            // cannot be passed: change lane
}

// Put on every obstacle (with a trigger collider). Touching it ends the run.
public class Obstacle : MonoBehaviour
{
    [SerializeField] private ObstacleKind kind;

    public ObstacleKind Kind => kind;
}
