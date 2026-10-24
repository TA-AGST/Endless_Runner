using UnityEngine;
using UnityEngine.UI;

// Distance, speed and a bar that shows how hard the game is now. After a crash it shows the result.
public class RunHud : MonoBehaviour
{
    [SerializeField] private Text distanceText;
    [SerializeField] private Text speedText;
    [SerializeField] private RectTransform difficultyFill;
    [SerializeField] private Text godModeText;
    [SerializeField] private GameObject crashPanel;
    [SerializeField] private Text crashText;

    private void Start()
    {
        crashPanel.SetActive(false);
        GameManager.Instance.Crashed += OnCrashed;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null) GameManager.Instance.Crashed -= OnCrashed;
    }

    private void OnCrashed()
    {
        crashPanel.SetActive(true);
        crashText.text = "CRASHED\n<size=36>You ran " + GameManager.Instance.Distance.ToString("0") + " m</size>\n<size=28>press R to run again</size>";
    }

    private void Update()
    {
        GameManager game = GameManager.Instance;

        distanceText.text = game.Distance.ToString("0") + " m";
        if (game.State == GameState.Running) speedText.text = (game.Speed * 3.6f).ToString("0") + " km/h";
        difficultyFill.localScale = new Vector3(game.Difficulty, 1f, 1f);
        godModeText.gameObject.SetActive(game.GodMode);
    }
}
