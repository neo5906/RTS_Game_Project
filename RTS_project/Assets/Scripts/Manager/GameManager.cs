using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : SingletonManager<GameManager>
{
    public Unit ActiveUnit;
    [SerializeField] private float detectedRadius = 0.3f;//探测半径
    [SerializeField] private GameObject pointer;

    [Header("UI Parameters")]
    [SerializeField] private ActionBar ActionBar;
    [SerializeField] private PlaceBuildingUI PlaceBuildingUI;
    [SerializeField] private TrainingUnitUI TrainingUnitUI;
    [SerializeField] private TrainingUI TrainingUI;
    [SerializeField] private Image GameOverUI;
    [SerializeField] private Canvas mainCanvas;

    [Header("Registered Target")]
    public List<Unit> RegisteredUnits = new List<Unit>();

    [Header("Camera Options")]
    [SerializeField] private float PanSpeed;
    [SerializeField] private CameraBounds CameraBounds;

    [Header("Resources Amount")]
    public int CultureAmount;
    public int GoldAmount;
    public int WoodAmount;
    public int RockAmount;
    public UnityAction onResourcesChanged;

    private Vector2 m_MousePosition;
    private LineRenderer ActiveRay;
    private PlacementProcess m_PlacementProcess;
    private TilemapManager m_TilemapManager;

    private CameraController m_CameraController;


    private void Start()
    {
        m_TilemapManager = TilemapManager.Get();
        m_CameraController = new(PanSpeed, CameraBounds);
        //m_CameraController = new(PanSpeed);
    }

    private void Update()
    {
        m_CameraController.Update();
        if (m_PlacementProcess != null)
        {
            m_PlacementProcess.Update();
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            m_MousePosition = mousePosition;
            HandleClick(mousePosition);
        }
        UpdateMovementRay();

    }

    private void InitializeRay()
    {
        GameObject go = new GameObject("MovementRay");
        ActiveRay = go.AddComponent<LineRenderer>();
        ActiveRay.material = new Material(Shader.Find("Sprites/Default"));
        //射线图层
        ActiveRay.sortingOrder = 50;
        //射线颜色
        ActiveRay.startColor = Color.green;
        ActiveRay.endColor = Color.green;
        //射线宽度
        ActiveRay.startWidth = 0.1f;
        ActiveRay.endWidth = 0.1f;
        StartCoroutine(HideRayAfterDelay(go));
    }

    private void HandleClick(Vector2 _mousePosition)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(_mousePosition, detectedRadius);

        foreach (var collider in colliders)
        {
            if (collider.TryGetComponent(out Unit unit)) 
            {
                SelectNewUnit(unit);
                return;
            }     
        }
        if (ActiveUnit != null && ActiveUnit.TryGetComponent(out HumanoidUnit _))
        {
            (ActiveUnit as HumanoidUnit).MoveToDestination(_mousePosition);
            InitializeRay();
            Instantiate(pointer, _mousePosition, Quaternion.identity);
        }
    }

    private void UpdateMovementRay()
    {
        if (ActiveUnit != null && ActiveRay != null)
        {
            ActiveRay.positionCount = 2;
            ActiveRay.SetPosition(0, ActiveUnit.transform.position);
            ActiveRay.SetPosition(1, m_MousePosition);
        }
    }

    private IEnumerator HideRayAfterDelay(GameObject _ray)
    {
        yield return new WaitForSeconds(1f);//等待1s后射线消失
        Destroy(_ray);
        ActiveRay = null;
    }

    private void SelectNewUnit(Unit _unit)
    {
        if (_unit.TryGetComponent(out StructureUnit structure) && structure.IsUnderConstruction && ActiveUnit is WorkerUnit worker)
        {
            worker.AssignTarget(structure);
            worker.currentTask = WorkerTask.Building;
            return;
        }
        else if (_unit.TryGetComponent(out TreeUnit tree) && !tree.IsDead && ActiveUnit is WorkerUnit worker_2)
        {
            worker_2.AssignTarget(tree);
            worker_2.currentTask = WorkerTask.Chopping;
            return;
        }
        else if (_unit.CompareTag("RedUnit") && !_unit.IsDead)
        {
            if (ActiveUnit != null && (!ActiveUnit.TryGetComponent(out WorkerUnit _) || !ActiveUnit.TryGetComponent(out StructureUnit _)))
            {
                (ActiveUnit as HumanoidUnit).AssignTarget(_unit);
                return;
            }
        }

        ActiveUnit = _unit.CompareTag("BlueUnit") ? _unit : null;
        ActionBar.ClearAllActionButtons();
        ActionBar.HideActionBar();

        if (ActiveUnit != null && ActiveUnit.Actions.Count > 0)
        {
            ActionBar.ShowActionBar();

            foreach (var action in ActiveUnit.Actions)
            {
                ActionBar.RegisterActionButton(action.Icon, () => action.ExecuteAction());
            }
        }
    }

    public void StartBuildingProcess(BuildingActionSO _action)
    {
        

        m_PlacementProcess = new(_action, m_TilemapManager);

        PlaceBuildingUI.ShowRectangle(_action.GoldCost,_action.WoodCost, _action.RockCost);
        PlaceBuildingUI.RegisterHooks(()=>ConfirmPlacement(_action), CanclePlacement);
    }

    private void ConfirmPlacement(BuildingActionSO _action)
    {
        if (WoodAmount >= _action.WoodCost && GoldAmount >= _action.GoldCost && RockAmount >= _action.RockCost)
        {
            WoodAmount -= _action.WoodCost;
            GoldAmount -= _action.GoldCost;
            RockAmount -= _action.RockCost;
            onResourcesChanged?.Invoke();
        }
        else
        {
            return;
        }

        var buildintAction = m_PlacementProcess.BuildingAction;

        if (buildintAction == null)
            return;

        if (m_PlacementProcess.CanPlaceBuilding(out Vector3 placePosition))
        {
            new BuildingProcess(buildintAction, placePosition);
            ClearActionBarUI();
            ClearPlacement();
        }
    }

    private void CanclePlacement()
    {
        ClearPlacement();
    }

    private void ClearActionBarUI()
    {
        ActionBar.ClearAllActionButtons();
        ActionBar.HideActionBar();
    }

    private void ClearPlacement()
    {
        if (m_PlacementProcess != null)
        {
            m_PlacementProcess.ClearupPlacement();
            m_PlacementProcess = null;
            PlaceBuildingUI.HideRectangle();
        }
    }

    public void StartTrainingProcess(TrainingActionSO _trainingAction)
    {
        TrainingUnitUI.ShowRectangle(_trainingAction.TrainingCost);
        TrainingUnitUI.RegisterHooks(() => ConfirmTraining(_trainingAction), CancleTraining);
    }

    private void ConfirmTraining(TrainingActionSO _trainingAction)
    {
        if (GoldAmount >= _trainingAction.TrainingCost)
        {
            GoldAmount -= _trainingAction.TrainingCost;
            onResourcesChanged();
        }
        else
        {
            return;
        }

        TrainingUI.RegisterTrainingUnit(_trainingAction.UnitType, _trainingAction.TrainingTime, ActiveUnit as StructureUnit, _trainingAction.UnitPrefab);
    }

    private void CancleTraining()
    {
        TrainingUnitUI.HideRectangle();
        ClearActionBarUI();
    }

    public bool CanEnemyPlaceBuilding(BuildingActionSO _buildingAction, Vector3 _placePosition)
    {
        var placementProcess = new PlacementProcess(_buildingAction, m_TilemapManager, true);

        if (placementProcess.CanPlaceBuilding(_placePosition))
        {
            return true;
        }
        return false;
    }

    public void RegisterUnit(Unit _unit)
    {
        RegisteredUnits.Add(_unit);
    }

    public void RemoveUnit(Unit _unit)
    {
        RegisteredUnits.Remove(_unit);

        bool HasActiveBlueBuilding = RegisteredUnits.Where(unit => unit != null && !unit.IsDead && unit.TryGetComponent(out StructureUnit _) && unit.CompareTag("BlueUnit")).Any();
        bool HasActiveRedBuilding = RegisteredUnits.Where(unit => unit != null && !unit.IsDead && unit.TryGetComponent(out StructureUnit _) && unit.CompareTag("RedUnit")).Any();

        if (!HasActiveBlueBuilding)
        {
            GameOverUI.gameObject.SetActive(true);
            RegisteredUnits.Where(unit => !unit.IsDead && unit.CompareTag("BlueUnit")).ToList().ForEach(unit => unit.Death());
            StartCoroutine(GameOver());
        }

        if (!HasActiveRedBuilding)
        {
            GameOverUI.gameObject.SetActive(true);
            RegisteredUnits.Where(unit => unit.CompareTag("RedUnit")).ToList().ForEach(unit => unit.Death());
            StartCoroutine(GameOver());
        }
    }

    private IEnumerator GameOver()
    {
        yield return new WaitForSeconds(2f);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }

    // GameManager.cs 中新增内容

    [Header("Tech Tree")]
    [SerializeField] private TechnologyTreeSO techTree;
    [SerializeField] public GameObject techTreePanelPrefab;      // 科技树UI预制体
    [SerializeField] public GameObject techDetailPanelPrefab;    // 详情UI预制体

    private GameObject currentTechTreePanel;
    private TechTreeUI currentTechTreeUI;

    // 打开科技树界面
    public void OpenTechTree()
    {
        if (currentTechTreePanel == null)
        {
            currentTechTreePanel = Instantiate(techTreePanelPrefab, mainCanvas.transform);
            currentTechTreeUI = currentTechTreePanel.GetComponent<TechTreeUI>();
            
        }
        currentTechTreePanel.SetActive(true);
        currentTechTreeUI.Initialize(techTree, this);
        Debug.Log($"TechTreePanel active: {currentTechTreePanel.activeSelf}");
        Debug.Log($"TechTreePanel activeInHierarchy: {currentTechTreePanel.activeInHierarchy}");
        Debug.Log($"TechTreePanel parent: {currentTechTreePanel.transform.parent.name}");
    }

    // 关闭科技树界面
    public void CloseTechTree()
    {
        if (currentTechTreePanel != null)
            currentTechTreePanel.SetActive(false);
    }

    // 尝试解锁科技节点
    public bool TryUnlockTechNode(TechnologyNodeSO node)
    {
        // 检查前置条件和资源
        if (!node.ArePrerequisitesMet())
        {
            Debug.Log("前置科技未解锁");
            return false;
        }

        if (CultureAmount < node.CultureCost)
        {
            Debug.Log("文化值不足");
            return false;
        }

        // 消耗文化值
        CultureAmount -= node.CultureCost;
        node.Unlock();
        onResourcesChanged?.Invoke();

        // 更新UI（可以发送事件通知科技树UI刷新）
        currentTechTreeUI?.RefreshAllNodes();
        return true;
    }

    public void OpenTechDetail(TechnologyNodeSO node)
    {
        if (techDetailPanelPrefab == null)
        {
            Debug.LogError("未指定 techDetailPanelPrefab，无法打开详情界面。");
            return;
        }

        // 实例化详情面板
        GameObject detailObj = Instantiate(techDetailPanelPrefab, mainCanvas.transform);
        TechDetailUI detailUI = detailObj.GetComponent<TechDetailUI>();
        if (detailUI != null)
        {
            detailUI.Setup(node, this);
        }
        else
        {
            Debug.LogError("TechDetailPanel 预制体缺少 TechDetailUI 组件！");
        }
    }
}
