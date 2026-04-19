using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public enum TrainingUnitType
{
    Warrior, Archer, Worker,
    YSZS,
    GR,
    ZZSB,
    GJS,
    PJGJS,
    PJSB,
    DJJ,
    SGS
}

public class TrainingUI : MonoBehaviour
{
    [SerializeField] private RectTransform PartForm;
    [Header("UI Elements")]
    [SerializeField] private GameObject WarriorSlot;
    [SerializeField] private GameObject ArcherSlot;
    [SerializeField] private GameObject WorkerSlot;
    [SerializeField] private GameObject YSZSSlot;
    [SerializeField] private GameObject GRSlot;
    [SerializeField] private GameObject ZZSBSlot;
    [SerializeField] private GameObject GJSSlot;
    [SerializeField] private GameObject PJGJSSlot;
    [SerializeField] private GameObject PJSBSlot;
    [SerializeField] private GameObject DJJSlot;
    [SerializeField] private GameObject SGSSlot;

    private Queue<GameObject> TrainingUnits;
    private Queue<GameObject> TrainingSlots;
    private Queue<float> TrainingTime;
    private Queue<StructureUnit> TrainingBarracks;

    private GameObject newSlot;
    private float TrainingTimer;
    private bool IsTraining = false;

    private Vector3[] Destination = new Vector3[] { Vector3.down };

    private void Start()
    {
        TrainingTime = new();
        TrainingSlots = new();
        TrainingBarracks = new();
        TrainingUnits = new();
    }

    private void Update()
    {
        if (!IsQueueVaild() || IsTraining)
        {
            return;
        }

        StartCoroutine(StartTrainingProcess());
    }

    private IEnumerator StartTrainingProcess()
    {
        IsTraining = true;

        TrainingTimer = TrainingTime.Dequeue();
        yield return new WaitForSeconds(TrainingTimer);

        var slot = TrainingSlots.Dequeue();
        Destroy(slot);

        var barrack = TrainingBarracks.Dequeue();
        if (barrack == null || barrack.IsDead)
        {
            IsTraining = false;
            yield break;
        }

        var unitPrefab = TrainingUnits.Dequeue();

        Vector3 spawnPosition = FindSpawnPositionAroundBuilding(barrack.transform.position, barrack.GetComponent<Collider2D>());

        GameObject newUnit = Instantiate(unitPrefab, spawnPosition, Quaternion.identity);

        IsTraining = false;
    }

    // 辅助方法：在建筑周围寻找无碰撞体的位置
    private Vector3 FindSpawnPositionAroundBuilding(Vector3 center, Collider2D buildingCollider)
    {
        // 优先尝试四个主方向
        Vector3[] directions = { Vector3.down, Vector3.up, Vector3.left, Vector3.right };
        //float checkRadius = 2.5f;
        float spacing = 1.5f;

        foreach (var dir in directions)
        {
            for (float dist = spacing; dist <= 5f; dist += spacing)
            {
                Vector3 testPos = center + dir * dist;
                Collider2D hit = Physics2D.OverlapCircle(testPos, 0.3f);
                if (hit == null || hit == buildingCollider)
                {
                    return testPos;
                }
            }
        }

        // 降级：返回建筑下方 3 单位处
        return center + Vector3.down * 3f;
    }

    private bool IsQueueVaild() => TrainingSlots.Count > 0 && TrainingTime.Count > 0 && TrainingBarracks.Count > 0 && TrainingUnits.Count > 0;

    public void RegisterTrainingUnit(TrainingUnitType _unitType, float _trainingTime, StructureUnit _barrack, GameObject _unit)
    {
        switch (_unitType)
        {
            case TrainingUnitType.Warrior:
                newSlot = Instantiate(WarriorSlot, PartForm);
                break;
            case TrainingUnitType.Archer:
                newSlot = Instantiate(ArcherSlot, PartForm);
                break;
            case TrainingUnitType.Worker:
                newSlot = Instantiate(WorkerSlot, PartForm);
                break;
            case TrainingUnitType.YSZS:
                newSlot = Instantiate(YSZSSlot, PartForm);
                break;
            case TrainingUnitType.GR:
                newSlot = Instantiate(GRSlot, PartForm);
                break;
            case TrainingUnitType.ZZSB:
                newSlot = Instantiate(ZZSBSlot, PartForm);
                break;
            case TrainingUnitType.GJS:
                newSlot = Instantiate(GJSSlot, PartForm);
                break;
            case TrainingUnitType.PJGJS:
                newSlot = Instantiate(PJGJSSlot, PartForm);
                break;
            case TrainingUnitType.PJSB:
                newSlot = Instantiate(PJSBSlot, PartForm);
                break;
            case TrainingUnitType.DJJ:
                newSlot = Instantiate(DJJSlot, PartForm);
                break;
            case TrainingUnitType.SGS:
                newSlot = Instantiate(SGSSlot, PartForm);
                break;
            default:
                return;
        }
        TrainingSlots.Enqueue(newSlot);
        TrainingUnits.Enqueue(_unit);
        TrainingTime.Enqueue(_trainingTime);
        TrainingBarracks.Enqueue(_barrack);
    }
}
