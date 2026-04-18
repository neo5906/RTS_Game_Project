using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public List<ActionSO> Actions = new List<ActionSO>();//定义可建造建筑的列表容器

    protected GameManager m_GameManager;
    

    protected SpriteRenderer sr => GetComponentInChildren<SpriteRenderer>();
    public UnitStats stats => GetComponent<UnitStats>();

    [Header("CheckTime")]
    [SerializeField] protected float CheckFrequency;
    protected float CheckTimer = 0f;

    public bool IsDead = false;

    [Header("Health Bar")]
    [SerializeField] protected GameObject HealthBar;

    public System.Action onFlipped;

    protected virtual void Start()
    {
        m_GameManager = GameManager.Get();
        m_GameManager.RegisteredUnits.Add(this);
        //m_GameManager.RegisterUnit(this);

        if (HealthBar != null)
            HealthBar.SetActive(m_GameManager.ShowHealthBars);
    }

    protected virtual void Update()
    {
        UpdateBehaviour();
    }

    protected virtual void UpdateBehaviour()
    {

    }

    public virtual void Death()
    {
        if (IsDead) return;
        IsDead = true;
        //m_GameManager.RegisteredUnits.Remove(this);
        m_GameManager.RemoveUnit(this);
        if (HealthBar != null)
            HealthBar.SetActive(false);
    }

    public void DestroyUnit() => Destroy(gameObject);

    public void AddAction(ActionSO action)
    {
        if (action == null) return;
        if (!Actions.Contains(action))
        {
            Actions.Add(action);
            Debug.Log($"{gameObject.name} 获得了：{action.name}");
        }
    }

    //血条显隐方法
    public void SetHealthBarActive(bool active)
    {
        if (HealthBar != null)
            HealthBar.SetActive(active);
    }
}
