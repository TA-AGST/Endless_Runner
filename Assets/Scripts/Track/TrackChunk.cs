using UnityEngine;

// One piece of road (30 m). The generator moves it forward again and again, so the road never ends.
// Every time it is reused the buildings on both sides get a new size and colour.
public class TrackChunk : MonoBehaviour
{
    [SerializeField] private float length = 30f;
    [SerializeField] private Transform obstacleRoot;
    [SerializeField] private Transform[] buildings;      // the boxes on the left and right of the road
    [SerializeField] private Color[] palette;

    private MaterialPropertyBlock block;
    private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

    public float Length => length;
    public float StartZ => transform.position.z;
    public float EndZ => transform.position.z + length;
    public Transform ObstacleRoot => obstacleRoot;

    // new look for the buildings
    public void Randomize()
    {
        if (block == null) block = new MaterialPropertyBlock();

        for (int i = 0; i < buildings.Length; i++)
        {
            Transform b = buildings[i];
            float side = b.localPosition.x < 0f ? -1f : 1f;
            float height = Random.Range(3f, 10f);
            float width = Random.Range(3f, 6f);

            b.localScale = new Vector3(width, height, Random.Range(5f, 9f));
            b.localPosition = new Vector3(side * (9.5f + width * 0.5f + Random.Range(0f, 3f)), height * 0.5f - 0.2f, Mathf.Lerp(2f, length - 2f, (i / 2) / (buildings.Length / 2f)));

            block.SetColor(BaseColor, palette[Random.Range(0, palette.Length)]);
            b.GetComponent<Renderer>().SetPropertyBlock(block);
        }
    }

    public void ClearObstacles()
    {
        for (int i = obstacleRoot.childCount - 1; i >= 0; i--) Destroy(obstacleRoot.GetChild(i).gameObject);
    }
}
