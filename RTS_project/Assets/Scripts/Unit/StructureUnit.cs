using System.Collections;
using UnityEngine;

public class StructureUnit : Unit
{
    [Header("Building Effect")]
    [SerializeField] public ParticleSystem BuildingEffect;

    [Header("Structure Unit")]
    [SerializeField] private GameObject TowerUnit;

    [Header("Death Info")]
    [SerializeField] private ParticleSystem DeathEffect;
    [SerializeField] private Sprite DeathIcon;

    [Header("Fortify Settings")]
    [SerializeField] private int fortifyLevel = 0;
    [SerializeField] private int[] fortifyArmorPerLevel = { 10, 0, 15, 0, 25, 0 };
    [SerializeField] private int[] fortifyHealthPerLevel = { 0, 100, 0, 500, 0, 1000 };

    [Header("Decorate Settings")]
    [SerializeField] private int decorateLevel = 0;

    protected BuildingProcess m_BuildingProcess;
    public bool IsUnderConstruction => m_BuildingProcess != null;

    private float processValue = 0f;

    public int FortifyLevel => fortifyLevel;
    public int DecorateLevel => decorateLevel;

    private int MaxAvailableFortifyLevel => GameManager.Get().GetMaxUnlockedFortifyLevel();
    private int MaxAvailableDecorateLevel => GameManager.Get().GetMaxUnlockedDecorateLevel();

    public int MaxFortifyLevelAvailable => MaxAvailableFortifyLevel;   // 已在内部计算
    public int MaxDecorateLevelAvailable => MaxAvailableDecorateLevel;

    public bool CanFortifyFurther => fortifyLevel < MaxAvailableFortifyLevel && MaxAvailableFortifyLevel > 0;
    public bool CanDecorateFurther => decorateLevel < MaxAvailableDecorateLevel && MaxAvailableDecorateLevel > 0;

    protected override void UpdateBehaviour()
    {
        if (Time.time - CheckTimer > CheckFrequency)
        {
            CheckTimer = Time.time;

            if (IsUnderConstruction)
            {
                processValue += 0.05f;
                if (processValue >= 1f)
                {
                    CompleteConstruction();
                }
            }
        }
    }

    public void AssignBuildingProcess(BuildingProcess _buildingProcess)
    {
        m_BuildingProcess = _buildingProcess;
        if (BuildingEffect != null)
            BuildingEffect.Play();
    }

    private void CompleteConstruction()
    {
        sr.sprite = m_BuildingProcess.BuildingAction.CompletionSprite;
        if (BuildingEffect != null)
            BuildingEffect.Stop();
        m_BuildingProcess = null;

        if (TowerUnit != null)
            TowerUnit.SetActive(true);
    }

    public override void Death()
    {
        base.Death();
        sr.sprite = DeathIcon;
        DeathEffect.Play();
        if (TowerUnit != null)
            Destroy(TowerUnit);
        StartCoroutine(AfterDeath());
    }

    private IEnumerator AfterDeath()
    {
        yield return new WaitForSeconds(1.5f);
        Destroy(gameObject);
    }

    // ----- 改良方法 -----
    public void Repair()
    {
        if (stats != null)
            stats.HealToFull();
    }

    public void ApplyFortifyUpgrade()
    {
        if (!CanFortifyFurther) return;
        int nextLevel = fortifyLevel + 1;

        // 增加护甲
        if (nextLevel - 1 < fortifyArmorPerLevel.Length)
            stats.Armor += fortifyArmorPerLevel[nextLevel - 1];

        // 增加最大生命值
        if (nextLevel - 1 < fortifyHealthPerLevel.Length)
            stats.AddMaxHealth(fortifyHealthPerLevel[nextLevel - 1]);

        fortifyLevel = nextLevel;
        GameManager.Get().RefreshStatusPanel(this);
    }

    public void ApplyDecorateUpgrade()
    {
        if (!CanDecorateFurther) return;
        decorateLevel++;
        var producer = GetComponent<ResourceProducerUnit>();
        if (producer != null)
            producer.OnDecorateUpgraded(decorateLevel);
        else
        {
            // 即使没有 ResourceProducerUnit 也提升等级，避免工人卡死
            Debug.LogWarning($"{name} 无 ResourceProducerUnit，但装饰等级已提升。");
        }
        GameManager.Get().RefreshStatusPanel(this);
    }
}