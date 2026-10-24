using UnityEngine;
using UnityEngine.InputSystem;

// The runner. It always moves forward, A / D (or the arrows) change the lane, Space jumps, S slides.
// Touching an obstacle ends the run. The running, leaning and sliding of the body is done in code.
[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlayerRunner : MonoBehaviour
{
    [Header("Lanes")]
    [SerializeField] private float laneWidth = 2.6f;
    [SerializeField] private float laneChangeSpeed = 16f;

    [Header("Jump and slide")]
    [SerializeField] private float jumpHeight = 1.7f;
    [SerializeField] private float gravity = -45f;
    [SerializeField] private float slideTime = 0.75f;
    [SerializeField] private float standHeight = 1.8f;
    [SerializeField] private float slideHeight = 0.9f;

    [Header("Body")]
    [SerializeField] private Transform model;
    [SerializeField] private Transform legLeft;
    [SerializeField] private Transform legRight;

    private CapsuleCollider capsule;
    private int lane;                 // -1 left, 0 middle, 1 right
    private float verticalSpeed;
    private float slideTimer;
    private float runCycle;
    private float lean;

    public int Lane => lane;
    public bool IsSliding => slideTimer > 0f;
    public bool IsGrounded { get; private set; } = true;

    private void Awake()
    {
        capsule = GetComponent<CapsuleCollider>();
        GetComponent<Rigidbody>().isKinematic = true;
    }

    private void Update()
    {
        GameManager game = GameManager.Instance;
        if (game.State != GameState.Running)
        {
            AnimateCrash();
            return;
        }

        ReadInput();
        Move(game);
        UpdateCollider();
        Animate(game);
        game.Distance = transform.position.z;
    }

    private void ReadInput()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        if (kb.aKey.wasPressedThisFrame || kb.leftArrowKey.wasPressedThisFrame) lane = Mathf.Max(-1, lane - 1);
        if (kb.dKey.wasPressedThisFrame || kb.rightArrowKey.wasPressedThisFrame) lane = Mathf.Min(1, lane + 1);

        bool jump = kb.spaceKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame;
        bool down = kb.sKey.wasPressedThisFrame || kb.downArrowKey.wasPressedThisFrame;

        if (jump && IsGrounded)
        {
            verticalSpeed = Mathf.Sqrt(2f * -gravity * jumpHeight);
            IsGrounded = false;
            slideTimer = 0f;
        }

        if (down)
        {
            if (IsGrounded) slideTimer = slideTime;
            else verticalSpeed = -22f;          // in the air: drop quickly, then slide
        }
    }

    private void Move(GameManager game)
    {
        Vector3 position = transform.position;

        position.z += game.Speed * Time.deltaTime;
        float targetX = lane * laneWidth;
        position.x = Mathf.Lerp(position.x, targetX, 1f - Mathf.Exp(-laneChangeSpeed * Time.deltaTime));

        verticalSpeed += gravity * Time.deltaTime;
        position.y += verticalSpeed * Time.deltaTime;
        if (position.y <= 0f)
        {
            position.y = 0f;
            if (!IsGrounded && verticalSpeed < -20f) slideTimer = slideTime;    // fast drop: slide on landing
            verticalSpeed = 0f;
            IsGrounded = true;
        }

        if (slideTimer > 0f) slideTimer -= Time.deltaTime;
        transform.position = position;
    }

    // While sliding the hit box is low, so a high beam passes over it.
    private void UpdateCollider()
    {
        float height = IsSliding ? slideHeight : standHeight;
        capsule.height = height;
        capsule.center = new Vector3(0f, height * 0.5f, 0f);
    }

    private void Animate(GameManager game)
    {
        // body leans into the lane change
        float sideSpeed = (lane * laneWidth - transform.position.x);
        lean = Mathf.Lerp(lean, Mathf.Clamp(-sideSpeed * 12f, -25f, 25f), 12f * Time.deltaTime);

        float squash = IsSliding ? 0.5f : 1f;
        model.localScale = Vector3.Lerp(model.localScale, new Vector3(1f, squash, 1f), 18f * Time.deltaTime);
        model.localRotation = Quaternion.Euler(IsSliding ? -50f : 0f, 0f, lean);

        // legs swing faster the faster we run
        runCycle += Time.deltaTime * game.Speed * 0.9f;
        float swing = IsGrounded && !IsSliding ? Mathf.Sin(runCycle) * 50f : 20f;
        legLeft.localRotation = Quaternion.Euler(swing, 0f, 0f);
        legRight.localRotation = Quaternion.Euler(-swing, 0f, 0f);

        float bob = IsGrounded && !IsSliding ? Mathf.Abs(Mathf.Sin(runCycle)) * 0.08f : 0f;
        model.localPosition = new Vector3(0f, bob, 0f);
    }

    // after a crash the body falls forward
    private void AnimateCrash()
    {
        model.localRotation = Quaternion.Slerp(model.localRotation, Quaternion.Euler(80f, 0f, 0f), 6f * Time.deltaTime);
        model.localPosition = Vector3.Lerp(model.localPosition, new Vector3(0f, -0.3f, 0f), 6f * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<Obstacle>() != null) GameManager.Instance.Crash();
    }
}
