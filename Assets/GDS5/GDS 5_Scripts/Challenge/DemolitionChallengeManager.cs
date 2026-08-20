using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DemolitionChallengeManager : MonoBehaviour
{
    // This component is the single controller for the complete demolition challenge.
    // It connects the timer, destructible houses, waypoint markers, result screen, and scene buttons.

    // ================================================================
    // SYSTEM 1: CHALLENGE SETTINGS AND SHARED DATA
    // These Inspector fields define the time limit, target group, camera, and marker position.
    // ================================================================
    [Header("Challenge")]
    [SerializeField, Range(1f, 300f)] private float challengeDurationSeconds = 300f;
    [SerializeField] private Transform destructibleHousesRoot;
    [SerializeField] private Camera gameplayCamera;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Waypoint Display")]
    [SerializeField, Min(0f)] private float markerEdgePadding = 45f;

    [Header("HUD Scene References")]
    [SerializeField] private Canvas hudCanvas;
    [SerializeField] private Image countdownPanelBackground;
    [SerializeField] private Text challengeTitleText;
    [SerializeField] private Text timerText;
    [SerializeField] private Image houseCounterPanelBackground;
    [SerializeField] private Text objectiveText;
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private Image resultPanelBackground;
    [SerializeField] private Text resultTitleText;
    [SerializeField] private Text resultDetailsText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private RectTransform markerContainer;
    [SerializeField] private RectTransform markerTemplate;

    [Header("HUD Text Content")]
    [Tooltip("{0} is replaced by the current minutes and seconds.")]
    [SerializeField] private string timerTextFormat = "{0}";
    [Tooltip("{0} is houses remaining. {1} is the total number of houses.")]
    [SerializeField] private string houseCounterTextFormat = "HOUSES REMAINING: {0} / {1}";
    [SerializeField] private string winTitle = "CHALLENGE COMPLETE";
    [SerializeField] private string loseTitle = "TIME UP";
    [Tooltip("{0} is the total number of houses. {1} is the remaining time.")]
    [SerializeField] private string winDetailsFormat =
        "All {0} houses were destroyed with {1} remaining.";
    [Tooltip("{0} is houses remaining. {1} is the total number of houses.")]
    [SerializeField] private string loseDetailsFormat = "{0} of {1} houses remain.";

    [Header("HUD Runtime Colours")]
    [Tooltip("Disable this to keep the timer colour set directly on the Text component.")]
    [SerializeField] private bool useUrgentTimerColor = true;
    [SerializeField, Min(0f)] private float urgentTimeSeconds = 30f;
    [SerializeField] private Color normalTimerColor = Color.white;
    [SerializeField] private Color urgentTimerColor = new Color(1f, 0.25f, 0.15f);
    [Tooltip("Disable this to keep the result title colour set directly on the Text component.")]
    [SerializeField] private bool useResultTitleColors = true;
    [SerializeField] private Color winTitleColor = new Color(0.35f, 1f, 0.45f);
    [SerializeField] private Color loseTitleColor = new Color(1f, 0.3f, 0.2f);

    private readonly List<HouseTarget> houseTargets = new List<HouseTarget>();
    private readonly Dictionary<BreakableSecond, HouseTarget> targetByPart =
        new Dictionary<BreakableSecond, HouseTarget>();

    private ChallengeState state = ChallengeState.Initializing;
    private float remainingTime;
    private int remainingHouseCount;
    private int totalHouseCount;

    // The challenge can only be in one of these states at a time.
    // This prevents a win and a loss from being triggered together.
    private enum ChallengeState
    {
        Initializing,
        Running,
        Won,
        Lost
    }

    private sealed class HouseTarget
    {
        // One HouseTarget groups a complete house, all of its breakable parts, and its UI marker.
        public Transform Root;
        public readonly List<BreakableSecond> Parts = new List<BreakableSecond>();
        public RectTransform Marker;
        public Vector3 MarkerWorldPosition;
        public bool IsComplete;
    }

    // ================================================================
    // SYSTEM 2: STARTUP AND CHALLENGE INITIALIZATION
    // ================================================================

    // Awake runs before Start. It accepts only the HUD objects assigned in the Inspector.
    // No runtime fallback is created, so the scene version remains the single source of truth.
    private void Awake()
    {
        Time.timeScale = 1f;

        if (!ValidateHudReferences())
        {
            enabled = false;
            return;
        }

        PrepareHudForPlay();
    }

    // Start checks the required references, registers all houses, and begins the countdown.
    private void Start()
    {
        if (destructibleHousesRoot == null)
        {
            destructibleHousesRoot = transform;
        }

        if (gameplayCamera == null)
        {
            Debug.LogError(
                $"{nameof(DemolitionChallengeManager)} requires the active gameplay camera.",
                this);
            enabled = false;
            return;
        }

        BuildHouseTargets();
        if (totalHouseCount == 0)
        {
            Debug.LogError(
                $"{nameof(DemolitionChallengeManager)} found no houses containing {nameof(BreakableSecond)} under '{destructibleHousesRoot.name}'.",
                this);
            enabled = false;
            return;
        }

        remainingTime = challengeDurationSeconds;
        state = ChallengeState.Running;
        UpdateTimerText();
        UpdateObjectiveText();
    }

    // ================================================================
    // SYSTEM 3: COUNTDOWN TIMER
    // ================================================================

    // Update subtracts real frame time from the countdown and causes a loss at zero seconds.
    private void Update()
    {
        if (state != ChallengeState.Running)
        {
            return;
        }

        remainingTime = Mathf.Max(0f, remainingTime - Time.deltaTime);
        UpdateTimerText();

        if (remainingTime <= 0f && remainingHouseCount > 0)
        {
            FinishChallenge(false);
        }
    }

    // LateUpdate moves the markers after normal object and camera movement has finished for the frame.
    private void LateUpdate()
    {
        if (state == ChallengeState.Running)
        {
            UpdateMarkers();
        }
    }

    // ================================================================
    // SYSTEM 4: HOUSE REGISTRATION AND DESTRUCTION TRACKING
    // ================================================================

    // BuildHouseTargets treats every direct child of Destructible Houses as one complete house.
    // It collects each house's BreakableSecond parts and creates one numbered marker for that house.
    private void BuildHouseTargets()
    {
        houseTargets.Clear();
        targetByPart.Clear();

        for (int childIndex = 0; childIndex < destructibleHousesRoot.childCount; childIndex++)
        {
            Transform houseRoot = destructibleHousesRoot.GetChild(childIndex);
            BreakableSecond[] parts = houseRoot.GetComponentsInChildren<BreakableSecond>(true);

            if (parts.Length == 0)
            {
                Debug.LogWarning(
                    $"Challenge target '{houseRoot.name}' has no breakable sections and will not be counted.",
                    houseRoot);
                continue;
            }

            HouseTarget target = new HouseTarget
            {
                Root = houseRoot,
                MarkerWorldPosition = CalculateMarkerPosition(houseRoot)
            };

            foreach (BreakableSecond part in parts)
            {
                target.Parts.Add(part);
                targetByPart[part] = target;

                if (!part.IsBroken)
                {
                    // Listen for the moment this individual section successfully breaks.
                    part.Broken += HandlePartBroken;
                }
            }

            target.Marker = CreateMarker(); //this here add number to the marker//
            houseTargets.Add(target);
        }

        totalHouseCount = houseTargets.Count;
        remainingHouseCount = totalHouseCount;
    }

    // CalculateMarkerPosition finds the visual centre and top area of a house for its waypoint.
    // It falls back to three units above the house root when the house has no visible renderer.
    private Vector3 CalculateMarkerPosition(Transform houseRoot)
    {
        Renderer[] renderers = houseRoot.GetComponentsInChildren<Renderer>(false);
        bool hasBounds = false;
        Bounds combinedBounds = new Bounds(houseRoot.position, Vector3.zero);

        foreach (Renderer houseRenderer in renderers)
        {
            if (houseRenderer == null || !houseRenderer.enabled)
            {
                continue;
            }

            if (!hasBounds)
            {
                combinedBounds = houseRenderer.bounds;
                hasBounds = true;
            }
            else
            {
                combinedBounds.Encapsulate(houseRenderer.bounds);
            }
        }

        if (!hasBounds)
        {
            return houseRoot.position + Vector3.up * 3f;
        }

        float heightOffset = Mathf.Max(1f, combinedBounds.extents.y * 0.25f);
        return combinedBounds.center + Vector3.up * heightOffset;
    }

    // HandlePartBroken is called by BreakableSecond after one house section breaks.
    // The whole house is only completed when none of its registered sections remain intact.
    private void HandlePartBroken(BreakableSecond brokenPart)
    {
        if (state != ChallengeState.Running ||
            !targetByPart.TryGetValue(brokenPart, out HouseTarget target) ||
            target.IsComplete)
        {
            return;
        }

        foreach (BreakableSecond part in target.Parts)
        {
            if (part != null && !part.IsBroken)
            {
                return;
            }
        }

        CompleteHouse(target);
    }

    // CompleteHouse removes one house from the counter, hides its marker, and checks for victory.
    private void CompleteHouse(HouseTarget target)
    {
        target.IsComplete = true;
        remainingHouseCount = Mathf.Max(0, remainingHouseCount - 1);

        if (target.Marker != null)
        {
            target.Marker.gameObject.SetActive(false);
        }

        UpdateObjectiveText();

        if (remainingHouseCount == 0)
        {
            FinishChallenge(true);
        }
    }

    // ================================================================
    // SYSTEM 5: WIN, LOSS, AND RESULT SCREEN
    // ================================================================

    // FinishChallenge can run only once. It stops the challenge and displays the correct result.
    private void FinishChallenge(bool playerWon)
    {
        if (state != ChallengeState.Running)
        {
            return;
        }

        state = playerWon ? ChallengeState.Won : ChallengeState.Lost;

        foreach (HouseTarget target in houseTargets)
        {
            if (target.Marker != null)
            {
                target.Marker.gameObject.SetActive(false);
            }
        }

        resultPanel.SetActive(true);
        resultTitleText.text = playerWon ? winTitle : loseTitle;

        if (useResultTitleColors)
        {
            resultTitleText.color = playerWon ? winTitleColor : loseTitleColor;
        }

        if (playerWon)
        {
            resultDetailsText.text = ApplyTextFormat(
                winDetailsFormat,
                "All {0} houses were destroyed with {1} remaining.",
                totalHouseCount,
                FormatTime(remainingTime));
        }
        else
        {
            resultDetailsText.text = ApplyTextFormat(
                loseDetailsFormat,
                "{0} of {1} houses remain.",
                remainingHouseCount,
                totalHouseCount);
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 0f;
    }

    // ================================================================
    // SYSTEM 6: SCENE NAVIGATION
    // ================================================================

    // RestartChallenge restores normal game speed and reloads the current gameplay scene.
    public void RestartChallenge()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameObject.scene.name);
    }

    // ReturnToMainMenu restores normal game speed and loads the configured main-menu scene.
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // UpdateTimerText replaces {0} with the current time.
    // The optional urgent colour change can be disabled in the Inspector.
    private void UpdateTimerText()
    {
        timerText.text = ApplyTextFormat(timerTextFormat, "{0}", FormatTime(remainingTime));

        if (useUrgentTimerColor)
        {
            timerText.color = remainingTime <= urgentTimeSeconds
                ? urgentTimerColor
                : normalTimerColor;
        }
    }

    // UpdateObjectiveText shows how many complete houses still need to be destroyed.
    private void UpdateObjectiveText()
    {
        objectiveText.text = ApplyTextFormat(
            houseCounterTextFormat,
            "HOUSES REMAINING: {0} / {1}",
            remainingHouseCount,
            totalHouseCount);
    }

    // FormatTime converts raw seconds into a clear minutes-and-seconds value such as 03:00.
    private string FormatTime(float timeInSeconds)
    {
        int displayedSeconds = Mathf.CeilToInt(Mathf.Max(0f, timeInSeconds));
        int minutes = displayedSeconds / 60;
        int seconds = displayedSeconds % 60;
        return $"{minutes:00}:{seconds:00}";
    }

    // ApplyTextFormat safely inserts live values into the wording chosen in the Inspector.
    // If the braces are invalid, the default wording is used and the Console identifies the problem.
    private string ApplyTextFormat(string customFormat, string fallbackFormat, params object[] values)
    {
        try
        {
            return string.Format(customFormat, values);
        }
        catch (System.FormatException)
        {
            Debug.LogWarning(
                $"Invalid HUD text format '{customFormat}'. Using '{fallbackFormat}' instead.",
                this);
            return string.Format(fallbackFormat, values);
        }
    }

    // ================================================================
    // SYSTEM 7: HOUSE WAYPOINT MARKERS
    // ================================================================

    // UpdateMarkers changes each house's world position into a Canvas position.
    // Off-screen or behind-camera markers are kept inside the screen edge to guide the player.
    private void UpdateMarkers()
    {
        Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

        foreach (HouseTarget target in houseTargets)
        {
            if (target.IsComplete || target.Marker == null)
            {
                continue;
            }

            Vector3 projectedPosition = gameplayCamera.WorldToScreenPoint(target.MarkerWorldPosition);
            Vector2 screenPoint = new Vector2(projectedPosition.x, projectedPosition.y);

            if (projectedPosition.z <= 0f)
            {
                Vector2 direction = screenPoint - screenCenter;
                if (direction.sqrMagnitude < 0.001f)
                {
                    direction = Vector2.up;
                }

                screenPoint = screenCenter - direction.normalized * Mathf.Max(Screen.width, Screen.height);
            }

            screenPoint.x = Mathf.Clamp(
                screenPoint.x,
                markerEdgePadding,
                Screen.width - markerEdgePadding);
            screenPoint.y = Mathf.Clamp(
                screenPoint.y,
                markerEdgePadding,
                Screen.height - markerEdgePadding);

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    markerContainer,
                    screenPoint,
                    null,
                    out Vector2 localPoint))
            {
                target.Marker.anchoredPosition = localPoint;
            }
        }
    }

    // ================================================================
    // SYSTEM 8: EDITABLE HUD CONNECTION
    // ================================================================

    // ValidateHudReferences checks the permanent scene objects before the challenge starts.
    // A missing reference stops this manager and reports the problem instead of replacing the design.
    private bool ValidateHudReferences()
    {
        bool allReferencesAssigned =
            hudCanvas != null &&
            countdownPanelBackground != null &&
            challengeTitleText != null &&
            timerText != null &&
            houseCounterPanelBackground != null &&
            objectiveText != null &&
            resultPanel != null &&
            resultPanelBackground != null &&
            resultTitleText != null &&
            resultDetailsText != null &&
            restartButton != null &&
            mainMenuButton != null &&
            markerContainer != null &&
            markerTemplate != null;

        if (!allReferencesAssigned)
        {
            Debug.LogError(
                $"{nameof(DemolitionChallengeManager)} has a missing HUD Scene Reference. " +
                "Select Destructible Houses and assign every HUD field in the Inspector.",
                this);
        }

        return allReferencesAssigned;
    }

    // PrepareHudForPlay hides edit-time preview objects and connects the two result buttons.
    private void PrepareHudForPlay()
    {
        resultPanel.SetActive(false);

        if (markerTemplate != null)
        {
            markerTemplate.gameObject.SetActive(false);
        }

        restartButton.onClick.RemoveListener(RestartChallenge);
        restartButton.onClick.AddListener(RestartChallenge);

        mainMenuButton.onClick.RemoveListener(ReturnToMainMenu);
        mainMenuButton.onClick.AddListener(ReturnToMainMenu);
    }

    // CreateMarker copies the editable marker template for one registered house.
    // Only the displayed number changes; the template's image, font, colour, and children are preserved.
    private RectTransform CreateMarker()//int targetNumber//)
    {
        GameObject markerCopy = Instantiate(
            markerTemplate.gameObject,
            markerContainer,
            false);
        markerCopy.name = $"House Target {houseTargets.Count + 1} Marker";
        
        markerCopy.SetActive(true);

        return markerCopy.GetComponent<RectTransform>();
    }

    // OnDestroy removes every break notification subscription when this manager is destroyed.
    // This prevents destroyed scene objects from continuing to call the old manager.
    private void OnDestroy()
    {
        foreach (KeyValuePair<BreakableSecond, HouseTarget> pair in targetByPart)
        {
            if (pair.Key != null)
            {
                pair.Key.Broken -= HandlePartBroken;
            }
        }
    }

    // OnValidate keeps Inspector values inside safe limits while the scene is being edited.
    private void OnValidate()
    {
        challengeDurationSeconds = Mathf.Clamp(challengeDurationSeconds, 1f, 300f);
        markerEdgePadding = Mathf.Max(0f, markerEdgePadding);
        urgentTimeSeconds = Mathf.Clamp(urgentTimeSeconds, 0f, challengeDurationSeconds);
    }
}

// DemolitionChallengeManager counts complete houses, runs the three-minute timer, clones editable target markers, and shows one final win or lose result.
