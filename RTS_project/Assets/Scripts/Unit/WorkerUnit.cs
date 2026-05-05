using System.Collections;
using UnityEngine;

public enum WorkerTask
{
    None,
    Building,
    Chopping,
    Repair,
    Fortify,
    Decorate
}

public class WorkerUnit : HumanoidUnit
{
    [Header("Labor")]
    [SerializeField] private int maxLabor = 3;
    public int MaxLabor => maxLabor;
    public int CurrentLabor { get; private set; }

    [Header("Improvement")]
    [SerializeField] private float improvementDuration = 3f;

    public WorkerTask currentTask = WorkerTask.None;

    private bool isImproving = false;
    private float improvementTimer = 0f;
    private StructureUnit improvementTarget;

    protected override void Start()
    {
        base.Start();
        CurrentLabor = maxLabor;
    }

    protected override void Update()
    {
        base.Update();

        if (isImproving)
        {
            improvementTimer += Time.deltaTime;
            if (improvementTimer >= improvementDuration)
                CompleteImprovement();
        }
    }

    protected override void UpdateBehaviour()
    {
        if (isImproving) return;

        if (Time.time - CheckTimer >= CheckFrequency)
        {
            CheckTimer = Time.time;

            // 移动至目标
            if (HasRegisteredTarget)
            {
                MoveToDestination(Target.transform.position);
            }

            // 检测是否到达目标建筑
            if (HasRegisteredTarget && Target is StructureUnit targetStructure)
            {
                if (IsTargetDetected())
                {
                    // 检查当前任务是否可以执行
                    bool canImprove = false;
                    if (currentTask == WorkerTask.Repair)
                        canImprove = true;
                    else if (currentTask == WorkerTask.Fortify && targetStructure.CanFortifyFurther)
                        canImprove = true;
                    else if (currentTask == WorkerTask.Decorate && targetStructure.CanDecorateFurther)
                        canImprove = true;

                    if (!canImprove)
                    {
                        ClearTask();
                        return;
                    }

                    StartImprovement(targetStructure);
                }
            }
            // 伐木逻辑保留
            else if (HasRegisteredTarget && currentTask == WorkerTask.Chopping && Target is TreeUnit)
            {
                if (IsTargetDetected())
                    anim.SetBool("Chop", true);
                else
                    anim.SetBool("Chop", false);
            }
        }
    }

    private void StartImprovement(StructureUnit target)
    {
        if (CurrentLabor <= 0) return;

        ai.ClearPath();
        anim.SetBool("Move", false);
        isImproving = true;
        improvementTimer = 0f;
        improvementTarget = target;
        anim.SetBool("Build", true);

        if (improvementTarget.BuildingEffect != null)
            improvementTarget.BuildingEffect.Play();
    }

    private void CompleteImprovement()
    {
        isImproving = false;
        improvementTimer = 0f;

        anim.SetBool("Build", false);

        if (improvementTarget != null && !improvementTarget.IsDead)
        {
            if (improvementTarget.BuildingEffect != null)
                improvementTarget.BuildingEffect.Stop();

            switch (currentTask)
            {
                case WorkerTask.Repair:
                    improvementTarget.Repair();
                    break;
                case WorkerTask.Fortify:
                    improvementTarget.ApplyFortifyUpgrade();
                    break;
                case WorkerTask.Decorate:
                    improvementTarget.ApplyDecorateUpgrade();
                    break;
            }
        }

        CurrentLabor--;
        GameManager.Get().RefreshStatusPanel();

        UnassignTarget();
        ai.ClearPath();
        anim.SetBool("Move", false);
        improvementTarget = null;

        if (CurrentLabor <= 0)
        {
            Death();
        }
        else
        {
            currentTask = WorkerTask.None;
            GameManager.Get().RefreshStatusPanel();
        }
    }

    private void CancelImprovement()
    {
        isImproving = false;
        improvementTimer = 0f;
        anim.SetBool("Build", false);
        if (improvementTarget != null && !improvementTarget.IsDead)
        {
            if (improvementTarget.BuildingEffect != null)
                improvementTarget.BuildingEffect.Stop();
        }
        improvementTarget = null;
    }

    public void ClearTask()
    {
        if (isImproving) CancelImprovement();
        currentTask = WorkerTask.None;
        UnassignTarget();
        improvementTarget = null;
        anim.SetBool("Build", false);
        ai.ClearPath();
        GameManager.Get().RefreshStatusPanel();
    }

    public void SetTask(WorkerTask newTask)
    {
        if (CurrentLabor <= 0) return;
        currentTask = newTask;
        GameManager.Get().RefreshStatusPanel();
    }

    public void ChopTree()
    {
        if (HasRegisteredTarget)
            (Target as TreeUnit).Shake();
    }
}