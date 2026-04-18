using System.Collections;
using UnityEngine;

public class ResourceProducerUnit : StructureUnit
{
    [Header("Production Settings")]
    [SerializeField] private Sprite productionImage;          // 生产时显示的图片
    [SerializeField] private float productionInterval = 5f;   // 生产间隔（秒）
    [SerializeField] private float productionImageDuration = 1f; // 生产图片显示时长

    [Header("Resources Per Production")]
    [SerializeField] private int culturePerProduction = 0;
    [SerializeField] private int goldPerProduction = 0;
    [SerializeField] private int woodPerProduction = 0;
    [SerializeField] private int rockPerProduction = 0;

    private float productionTimer;

    protected override void UpdateBehaviour()
    {
        base.UpdateBehaviour();

        // 仅当建筑已完成建造且属于玩家阵营时才生产资源
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
        // 切换为生产图片（如果有）
        Sprite originalSprite = sr.sprite;
        if (productionImage != null)
        {
            sr.sprite = productionImage;
        }

        // 增加各项资源（增量为 0 的资源实际不增加）
        if (culturePerProduction != 0)
        {
            m_GameManager.CultureAmount += culturePerProduction;
        }
        if (goldPerProduction != 0)
        {
            m_GameManager.GoldAmount += goldPerProduction;
        }
        if (woodPerProduction != 0)
        {
            m_GameManager.WoodAmount += woodPerProduction;
        }
        if (rockPerProduction != 0)
        {
            m_GameManager.RockAmount += rockPerProduction;
        }

        // 触发资源更新事件（只需调用一次）
        m_GameManager.onResourcesChanged?.Invoke();

        // 等待图片显示时间后恢复原图
        yield return new WaitForSeconds(productionImageDuration);
        sr.sprite = originalSprite;
    }
}