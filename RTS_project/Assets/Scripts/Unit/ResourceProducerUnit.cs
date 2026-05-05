using System.Collections;
using UnityEngine;

public class ResourceProducerUnit : StructureUnit
{
    [Header("Production Settings")]
    [SerializeField] private Sprite productionImage;
    [SerializeField] private float productionInterval = 5f;
    [SerializeField] private float productionImageDuration = 1f;

    [Header("Base Resources Per Production")]
    [SerializeField] private int culturePerProduction = 0;
    [SerializeField] private int goldPerProduction = 0;
    [SerializeField] private int woodPerProduction = 0;
    [SerializeField] private int rockPerProduction = 0;

    [Header("Decorate Bonuses")]
    [Tooltip("每级装饰额外增加的金币产量")]
    [SerializeField] private int[] decorateGoldBonus = { 0, 0, 5, 5, 0, 10 };
    [Tooltip("每级装饰额外增加的文化产量")]
    [SerializeField] private int[] decorateCultureBonus = { 1, 2, 2, 5, 10, 10 };

    private float productionTimer;

    public void OnDecorateUpgraded(int level) { }
    protected override void UpdateBehaviour()
    {
        base.UpdateBehaviour();

        if (!IsUnderConstruction && CompareTag("BlueUnit"))
        {
            if (Time.time - productionTimer >= productionInterval)
            {
                productionTimer = Time.time;
                StartCoroutine(ProduceResources());
            }
        }
    }

    private IEnumerator ProduceResources()
    {
        Sprite originalSprite = sr.sprite;
        if (productionImage != null)
            sr.sprite = productionImage;

        // 计算装饰加成（基于当前装饰等级）
        int bonusGold = 0;
        int bonusCulture = 0;
        if (DecorateLevel > 0 && DecorateLevel <= decorateGoldBonus.Length)
            bonusGold = decorateGoldBonus[DecorateLevel - 1];
        if (DecorateLevel > 0 && DecorateLevel <= decorateCultureBonus.Length)
            bonusCulture = decorateCultureBonus[DecorateLevel - 1];

        if (culturePerProduction + bonusCulture != 0)
            m_GameManager.CultureAmount += culturePerProduction + bonusCulture;
        if (goldPerProduction + bonusGold != 0)
            m_GameManager.GoldAmount += goldPerProduction + bonusGold;
        if (woodPerProduction != 0)
            m_GameManager.WoodAmount += woodPerProduction;
        if (rockPerProduction != 0)
            m_GameManager.RockAmount += rockPerProduction;

        m_GameManager.onResourcesChanged?.Invoke();

        yield return new WaitForSeconds(productionImageDuration);
        sr.sprite = originalSprite;
    }
}