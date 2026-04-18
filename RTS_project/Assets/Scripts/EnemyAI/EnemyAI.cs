using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public StructureUnit MainCastle;
    public WorkerUnit Worker;
    public List<HumanoidUnit> ActiveUnits;

    private GameManager m_GameManager;
    private List<Vector3> m_PlacementGrid;

    private Queue<(TrainingActionSO action, BarrackUnit barrack)> trainingQueue = new();

    [Header("Training Settings")]
    [SerializeField] private float trainingCheckInterval = 2f;
    [SerializeField] private int maxTrainingQueueSize = 3;

    private float trainingCheckTimer;

    [Header("Attack Wave Settings")]
    [SerializeField] private float attackWaveInterval = 180f;
    [SerializeField] private bool enableWaveAttack = true;
    [SerializeField] private int minUnitsForWave = 1;

    private float attackWaveTimer;

    [Header("EnemyAIStage")]
    public List<EnemyAIStageSO> EnemyAIStages;
    [SerializeField] private float enemyCheckFrequency = 1f;
    [SerializeField] private float enemyRefillFrequency = 5f;

    private float enemyCheckTimer;
    private float enemyRefillTimer;
    private float stageUpdateTimer;
    private int currentStageIndex = 0;
    private float stageUpdateFrequency;
    public int currentWaveCount;

    private bool isTraining = false;

    private void Start()
    {
        m_GameManager = GameManager.Get();
        if (EnemyAIStages.Count > 0)
            stageUpdateFrequency = EnemyAIStages[0].StageExistWindow;

        attackWaveTimer = attackWaveInterval;
        currentWaveCount = 0;
    }

    private void Update()
    {
        // 进攻波次计时器
        if (enableWaveAttack)
        {
            attackWaveTimer -= Time.deltaTime;
            if (attackWaveTimer <= 0f)
            {
                attackWaveTimer = attackWaveInterval;
                currentWaveCount++;
                LaunchAttackWave();
            }
        }

        // 全局检查计时
        if (Time.time - enemyCheckTimer >= enemyCheckFrequency)
        {
            enemyCheckTimer = Time.time;

            if (currentStageIndex < EnemyAIStages.Count)
            {
                if (Time.time - stageUpdateTimer >= stageUpdateFrequency)
                {
                    var currentStage = EnemyAIStages[currentStageIndex];
                    // 检查波次要求是否满足
                    if (currentWaveCount >= currentStage.minWaveRequired)
                    {
                        ExecuteCurrentStage();
                        currentStageIndex++;

                        if (currentStageIndex < EnemyAIStages.Count)
                        {
                            stageUpdateFrequency = EnemyAIStages[currentStageIndex].StageExistWindow;
                            stageUpdateTimer = Time.time;
                        }
                    }
                    else
                    {
                        Debug.Log($"EnemyAI: 阶段 {currentStageIndex} 需要波次 {currentStage.minWaveRequired}，当前波次 {currentWaveCount}，等待中...");
                    }
                }
            }

            // 刷新活跃单位列表
            if (Time.time - enemyRefillTimer >= enemyRefillFrequency)
            {
                enemyRefillTimer = Time.time;
                RefillActiveUnits();
            }
        }

        // 持续训练逻辑
        if (Time.time - trainingCheckTimer >= trainingCheckInterval)
        {
            trainingCheckTimer = Time.time;
            TryAddTrainingToQueue();
        }

        // 启动训协程
        if (trainingQueue.Count > 0 && !isTraining)
        {
            StartCoroutine(ProcessTrainingQueue());
        }
    }

    private void ExecuteCurrentStage()
    {
        var stage = EnemyAIStages[currentStageIndex];
        if (stage.BuildingAction != null)
            EnemyPlaceBuilding(stage.BuildingAction);
    }

    private void TryAddTrainingToQueue()
    {
        if (trainingQueue.Count >= maxTrainingQueueSize) return;

        var barracks = FindObjectsOfType<BarrackUnit>()
            .Where(b => b != null && !b.IsDead && b.CompareTag("RedUnit") && !b.IsUnderConstruction)
            .ToList();

        if (barracks.Count == 0) return;

        // 只收集已满足波次要求的阶段的训练单位
        List<TrainingActionSO> availableTrainings = new();
        foreach (var stage in EnemyAIStages)
        {
            if (currentWaveCount >= stage.minWaveRequired)
            {
                if (stage.TrainingActions != null)
                    availableTrainings.AddRange(stage.TrainingActions);
            }
        }
        if (availableTrainings.Count == 0) return;

        var barrack = barracks[Random.Range(0, barracks.Count)];
        var action = availableTrainings[Random.Range(0, availableTrainings.Count)];

        trainingQueue.Enqueue((action, barrack));
        Debug.Log($"EnemyAI: 添加训练任务 - {action.name} @ {barrack.name}");
    }

    private IEnumerator ProcessTrainingQueue()
    {
        isTraining = true;
        while (trainingQueue.Count > 0)
        {
            var (action, barrack) = trainingQueue.Dequeue();
            if (barrack == null || barrack.IsDead)
            {
                continue;
            }

            yield return new WaitForSeconds(action.TrainingTime);

            if (barrack != null && !barrack.IsDead)
            {
                GameObject newUnit = Instantiate(action.UnitPrefab, barrack.transform.position, Quaternion.identity);
                var humanoid = newUnit.GetComponent<HumanoidUnit>();
                humanoid.MoveToDestination(barrack.transform.position + Vector3.down * 3);
                ActiveUnits.Add(humanoid);
            }
        }
        isTraining = false;
    }

    private void RefillActiveUnits()
    {
        ActiveUnits = FindObjectsOfType<HumanoidUnit>()
            .Where(u => u != null && !u.IsDead && u.CompareTag("RedUnit") && u is not WorkerUnit)
            .ToList();
    }

    private void LaunchAttackWave()
    {
        // 刷新单位列表确保最新
        RefillActiveUnits();

        var combatUnits = ActiveUnits.Where(u => u != null && !u.IsDead).ToList();
        if (combatUnits.Count < minUnitsForWave)
        {
            Debug.Log($"EnemyAI: 兵力不足 ({combatUnits.Count}/{minUnitsForWave})，推迟进攻");
            return;
        }

        // 优先寻找玩家市中心
        MainUnit playerMain = FindPlayerMainBuilding();

        foreach (var unit in combatUnits)
        {
            if (playerMain != null && !playerMain.IsDead)
            {
                unit.AssignTarget(playerMain);
            }
            else
            {
                unit.FindClosestEnemyWithoutRange();
            }
        }

        Debug.Log($"EnemyAI: 发动波次进攻！参战单位数={combatUnits.Count}");
    }

    private MainUnit FindPlayerMainBuilding()
    {
        return m_GameManager.RegisteredUnits
            .FirstOrDefault(u => u != null && !u.IsDead && u.CompareTag("BlueUnit") && u is MainUnit) as MainUnit;
    }


    public float GetRemainingTimeToNextWave()
    {
        return Mathf.Max(0, attackWaveTimer);
    }

    private void EnemyPlaceBuilding(BuildingActionSO _buildingAction)
    {
        m_PlacementGrid = new();

        for (int i = -7; i <= 7; i++)
        {
            for (int j = -7; j <= 7; j++)
            {
                var placePosition = MainCastle.transform.position + new Vector3(i, j, 0);

                if (m_GameManager.CanEnemyPlaceBuilding(_buildingAction, placePosition))
                {
                    m_PlacementGrid.Add(placePosition);
                }
            }
        }
        if (m_PlacementGrid.Count == 0)
        {
            return;
        }

        var finalPosition = m_PlacementGrid[Random.Range(0, m_PlacementGrid.Count - 1)];

        new BuildingProcess(_buildingAction, finalPosition, out var structure);
        Worker.AssignTarget(structure);
        Worker.currentTask = WorkerTask.Building;
    }
}