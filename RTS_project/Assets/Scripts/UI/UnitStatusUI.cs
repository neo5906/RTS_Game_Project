using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitStatusUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI txtName;
    [SerializeField] private TextMeshProUGUI txtHealth;
    [SerializeField] private TextMeshProUGUI txtArmor;
    [SerializeField] private Button btnDeselect;
    [SerializeField] private GameObject workerPanel;          // 父物体，包裹下面的UI元素
    [SerializeField] private TextMeshProUGUI txtLabor;
    [SerializeField] private TextMeshProUGUI txtTask;
    [SerializeField] private Button btnRepair;
    [SerializeField] private Button btnFortify;
    [SerializeField] private Button btnDecorate;
    [Header("Building Panel")]
    [SerializeField] private GameObject buildingPanel;
    [SerializeField] private TextMeshProUGUI txtFortifyLevel;
    [SerializeField] private TextMeshProUGUI txtDecorateLevel;

    private WorkerUnit currentWorker;
    private StructureUnit currentBuilding;

    private void Start()
    {
        btnRepair.onClick.AddListener(() => currentWorker?.SetTask(WorkerTask.Repair));
        btnFortify.onClick.AddListener(() => currentWorker?.SetTask(WorkerTask.Fortify));
        btnDecorate.onClick.AddListener(() => currentWorker?.SetTask(WorkerTask.Decorate));
        btnDeselect.onClick.AddListener(() => GameManager.Get().DeselectUnit());
        gameObject.SetActive(false);
    }

    public void Show(Unit unit)
    {
        if (unit == null) { Hide(); return; }
        gameObject.SetActive(true);

        txtName.text = unit.UnitName;
        if (unit.stats != null)
        {
            txtHealth.text = $"{unit.stats.CurrentHealth} / {unit.stats.MaxHealth}";
            txtArmor.text = unit.stats.Armor.ToString();
        }

        currentWorker = unit as WorkerUnit;
        currentBuilding = unit as StructureUnit;

        // 根据类型切换面板
        workerPanel.SetActive(currentWorker != null);
        buildingPanel.SetActive(currentBuilding != null);

        if (currentWorker != null)
            UpdateWorkerDisplay();
        else if (currentBuilding != null)
            UpdateBuildingDisplay();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        currentWorker = null;
        currentBuilding = null;
    }

    public void UpdateBuildingDisplay()
    {
        if (currentBuilding == null) return;
        txtFortifyLevel.text = $"{currentBuilding.FortifyLevel} / {currentBuilding.MaxFortifyLevelAvailable}";
        txtDecorateLevel.text = $"{currentBuilding.DecorateLevel} / {currentBuilding.MaxDecorateLevelAvailable}";
    }
    public void UpdateWorkerDisplay()
    {
        if (currentWorker == null) return;
        txtLabor.text = $"{currentWorker.CurrentLabor} / {currentWorker.MaxLabor}";
        if (currentWorker.currentTask == WorkerTask.Repair)
        {
            txtTask.text = $"修复";
        }
        else if(currentWorker.currentTask == WorkerTask.Fortify)
        {
            txtTask.text = $"加固";
        }
        else if (currentWorker.currentTask == WorkerTask.Decorate)
        {
            txtTask.text = $"改良";
        }
        else
        {
            txtTask.text = $"空闲";
        }
    }
    public void UpdateHealthDisplay(Unit unit)
    {
        if (unit == null || unit.stats == null) return;
        txtHealth.text = $"{unit.stats.CurrentHealth} / {unit.stats.MaxHealth}";
        txtArmor.text = unit.stats.Armor.ToString();

        // 如果当前是工人，同时更新工人信息，保持面板同步
        if (currentWorker != null)
            UpdateWorkerDisplay();
    }
}