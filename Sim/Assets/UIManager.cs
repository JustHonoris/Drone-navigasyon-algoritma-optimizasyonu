using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    [Header("Control Buttons")]
    [SerializeField] private Button resetButton;
    [Header("References")]
    public SimulationManager simulationManager;

    [Header("Panel References")]
    [SerializeField] private CanvasGroup parametersCanvasGroup;

    [Header("Left Panel - Settings")]
    [SerializeField] private TMP_InputField gridSizeX;
    [SerializeField] private TMP_InputField gridSizeY;
    [SerializeField] private TMP_InputField gridSizeZ;
    [SerializeField] private TMP_InputField nodeSizeInput;
    [SerializeField] private TMP_Dropdown algorithmDropdown;
    [SerializeField] private Toggle showGridToggle;

    [Header("Algorithm Parameters")]
    [SerializeField] private GameObject parametersContainer;
    [SerializeField] private GameObject parameterPrefab;

    [Header("Statistics Panel")]
    [SerializeField] private TextMeshProUGUI pathLengthText;
    [SerializeField] private TextMeshProUGUI executionTimeText;
    [SerializeField] private TextMeshProUGUI nodesExploredText;
    [SerializeField] private TextMeshProUGUI pathCostText;
    [SerializeField] private TextMeshProUGUI heightDifferenceText;

    private Dictionary<string, TMP_InputField> algorithmParameters = new Dictionary<string, TMP_InputField>();
    private bool isParameterEditMode = false;

    private void Start()
    {
        if (!ValidateReferences()) return;

        InitializeUI();
        SetupListeners();
        SetParametersPanelVisible(false);
    }

    private bool ValidateReferences()
    {
        if (simulationManager == null)
        {
            Debug.LogError("SimulationManager reference is missing!");
            return false;
        }
        return true;
    }

    private void InitializeUI()
    {
        // Initialize Grid Size inputs
        Vector3 currentGridSize = simulationManager.gridSize;
        gridSizeX.text = currentGridSize.x.ToString();
        gridSizeY.text = currentGridSize.y.ToString();
        gridSizeZ.text = currentGridSize.z.ToString();

        // Initialize Node Size input
        nodeSizeInput.text = simulationManager.nodeSize.ToString();

        // Initialize Algorithm Dropdown
        algorithmDropdown.ClearOptions();
        algorithmDropdown.AddOptions(new List<string> {
            "A*", "Hill Climbing", "RRT", "Beam Search"
        });
        algorithmDropdown.value = (int)simulationManager.selectedAlgorithm;

        // Initialize Show Grid Toggle
        showGridToggle.isOn = simulationManager.showGrid;

        // Create initial algorithm parameters
        CreateAlgorithmParameters();
    }

    private void SetupListeners()
    {
        // Grid Size listeners
        gridSizeX.onEndEdit.AddListener((_) => UpdateGridSettings());
        gridSizeY.onEndEdit.AddListener((_) => UpdateGridSettings());
        gridSizeZ.onEndEdit.AddListener((_) => UpdateGridSettings());

        // Node Size listener
        nodeSizeInput.onEndEdit.AddListener((_) => UpdateGridSettings());

        // Algorithm Selection listener
        algorithmDropdown.onValueChanged.AddListener(OnAlgorithmChanged);

        // Show Grid listener
        showGridToggle.onValueChanged.AddListener(OnShowGridChanged);
        if (resetButton != null)
        {
            resetButton.onClick.AddListener(() => {
                if (simulationManager != null)
                {
                    simulationManager.ResetSimulation();
                }
            });
        }
    }

    private void Update()
    {
        // ESC tuþu ile parametre panelini gizle
        if (Input.GetKeyDown(KeyCode.Escape) && isParameterEditMode)
        {
            SetParametersPanelVisible(false);
        }
    }
    public void ResetUI()
    {
        // Grid boyut input'larýný resetle
        gridSizeX.text = simulationManager.gridSize.x.ToString();
        gridSizeY.text = simulationManager.gridSize.y.ToString();
        gridSizeZ.text = simulationManager.gridSize.z.ToString();

        // Node boyutu input'unu resetle
        nodeSizeInput.text = simulationManager.nodeSize.ToString();

        // Algoritma seçimini resetle
        algorithmDropdown.value = (int)simulationManager.selectedAlgorithm;

        // Grid görünürlük toggle'ýný resetle
        showGridToggle.isOn = simulationManager.showGrid;

        // Algoritma parametrelerini resetle
        CreateAlgorithmParameters();

        // Ýstatistikleri resetle
        UpdateStatistics(0, 0, 0, 0, 0);
    }
    public void SetParametersPanelVisible(bool visible)
    {
        if (parametersCanvasGroup != null)
        {
            parametersCanvasGroup.alpha = visible ? 1f : 0f;
            parametersCanvasGroup.blocksRaycasts = visible;
            parametersCanvasGroup.interactable = visible;
            isParameterEditMode = visible;
        }
    }

    private void UpdateGridSettings()
    {
        if (float.TryParse(gridSizeX.text, out float x) &&
            float.TryParse(gridSizeY.text, out float y) &&
            float.TryParse(gridSizeZ.text, out float z) &&
            float.TryParse(nodeSizeInput.text, out float nodeSize))
        {
            simulationManager.UpdateGridSettings(
                new Vector3(x, y, z),
                simulationManager.gridOffset,
                nodeSize
            );
        }
        else
        {
            Debug.LogWarning("Invalid grid settings input!");
        }
    }

    private void OnAlgorithmChanged(int value)
    {
        simulationManager.selectedAlgorithm = (SimulationManager.PathfindingAlgorithm)value;
        CreateAlgorithmParameters();
        SetParametersPanelVisible(true);
    }

    private void OnShowGridChanged(bool value)
    {
        simulationManager.showGrid = value;
    }

    private void CreateAlgorithmParameters()
    {
        // Clear existing parameters
        foreach (Transform child in parametersContainer.transform)
        {
            Destroy(child.gameObject);
        }
        algorithmParameters.Clear();

        // Create algorithm-specific parameters
        switch (simulationManager.selectedAlgorithm)
        {
            case SimulationManager.PathfindingAlgorithm.AStar:
                CreateParameterUI("diagonalCost", "Diagonal Cost", "1.4");
                CreateParameterUI("heightCostMultiplier", "Height Cost", "2.0");  // Önceki test deðeriniz
                CreateParameterUI("maxHeightDifference", "Max Height", "1.5");    // Önceki test deðeriniz
                break;

            case SimulationManager.PathfindingAlgorithm.HillClimbing:
                CreateParameterUI("maxAttempts", "Max Attempts", "50000");
                CreateParameterUI("heightWeight", "Height Weight", "1.0");
                CreateParameterUI("maxRestarts", "Max Restarts", "8");
                CreateParameterUI("randomJumpProbability", "Random Jump Probability", "0.03");
                break;

            case SimulationManager.PathfindingAlgorithm.RRT:
                CreateParameterUI("stepSize", "Step Size", "2.0");
                CreateParameterUI("goalBias", "Goal Bias", "0.3");
                CreateParameterUI("maxIterations", "Max Iterations", "2000");
                CreateParameterUI("minDistance", "Min Distance", "1.5");
                break;

            case SimulationManager.PathfindingAlgorithm.BeamSearch:
                CreateParameterUI("beamWidth", "Beam Width", "200");
                CreateParameterUI("maxIterations", "Max Iterations", "40000");
                CreateParameterUI("heightWeight", "Height Weight", "1.0");
                break;
        }
    }

    private void CreateParameterUI(string paramName, string displayName, string defaultValue)
    {
        GameObject paramObj = Instantiate(parameterPrefab, parametersContainer.transform);

        // Setup parameter UI
        TextMeshProUGUI labelText = paramObj.transform.Find("Label").GetComponent<TextMeshProUGUI>();
        TMP_InputField inputField = paramObj.transform.Find("InputField").GetComponent<TMP_InputField>();

        labelText.text = displayName;
        inputField.text = defaultValue;

        // Add listener for value change
        inputField.onEndEdit.AddListener((value) => UpdateAlgorithmParameter(paramName, value));

        algorithmParameters[paramName] = inputField;
    }

    private void UpdateAlgorithmParameter(string paramName, string value)
    {
        if (float.TryParse(value, out float floatValue))
        {
            switch (simulationManager.selectedAlgorithm)
            {
                case SimulationManager.PathfindingAlgorithm.HillClimbing:
                    simulationManager.UpdateHillClimbingParameter(paramName, floatValue);
                    break;

                case SimulationManager.PathfindingAlgorithm.RRT:
                    simulationManager.UpdateRRTParameter(paramName, floatValue);
                    break;

                case SimulationManager.PathfindingAlgorithm.BeamSearch:
                    simulationManager.UpdateBeamSearchParameter(paramName, floatValue);
                    break;
                    case SimulationManager.PathfindingAlgorithm.AStar:
                simulationManager.UpdateAStarParameter(paramName, floatValue);
                break;
            }
        }
        else
        {
            Debug.LogWarning($"Invalid parameter value for {paramName}");
        }
    }

    public void UpdateStatistics(float pathLength, float executionTime, int nodesExplored, float pathCost, float heightDifference)
    {
        pathLengthText.text = $"Path Length: {pathLength:F2}m";
        executionTimeText.text = $"Execution Time: {executionTime:F3}ms";
        nodesExploredText.text = $"Nodes Explored: {nodesExplored}";
        pathCostText.text = $"Path Cost: {pathCost:F2}";
        heightDifferenceText.text = $"Height Difference: {heightDifference:F2}m"; // heightDifferenceText yerine heightDifference kullanýlmalý
    }
}