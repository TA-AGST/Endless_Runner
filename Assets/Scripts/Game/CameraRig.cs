using System.Collections;
using UnityEngine;

// The camera runs behind the player. It follows him a little in the lanes, opens the view (wider angle)
// as the speed grows and shakes when the player crashes.
public class CameraRig : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Camera cam;
    [SerializeField] private Vector3 offset = new Vector3(0f, 4.2f, -7.5f);
    [SerializeField] private float lookAhead = 8f;
    [SerializeField, Range(0f, 1f)] private float sideFollow = 0.6f;
    [SerializeField] private float baseFov = 62f;
    [SerializeField] private float maxFov = 80f;

    private float shake;

    private void Start()
    {
        GameManager.Instance.Crashed += OnCrashed;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null) GameManager.Instance.Crashed -= OnCrashed;
    }

    private void OnCrashed()
    {
        StartCoroutine(Shake());
    }

    private IEnumerator Shake()
    {
        for (float t = 0f; t < 0.4f; t += Time.deltaTime)
        {
            shake = 1f - t / 0.4f;
            yield return null;
        }
        shake = 0f;
    }

    private void LateUpdate()
    {
        Vector3 p = target.position;
        Vector3 wanted = new Vector3(p.x * sideFollow + offset.x, offset.y, p.z + offset.z);
        wanted += Random.insideUnitSphere * (0.25f * shake);

        transform.position = Vector3.Lerp(transform.position, wanted, 1f - Mathf.Exp(-14f * Time.deltaTime));
        transform.rotation = Quaternion.LookRotation(new Vector3(p.x * 0.8f, 1.2f, p.z + lookAhead) - transform.position);

        float fov = Mathf.Lerp(baseFov, maxFov, GameManager.Instance.Difficulty);
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, fov, 3f * Time.deltaTime);
    }
}
