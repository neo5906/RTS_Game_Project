using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AttackWaveTimerUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private EnemyAI enemyAI;

    private void Update()
    {
        if (enemyAI == null) return;

        float remaining = enemyAI.GetRemainingTimeToNextWave();
        if (remaining > 0)
        {
            int minutes = Mathf.FloorToInt(remaining / 60);
            int seconds = Mathf.FloorToInt(remaining % 60);
            timerText.text = $"{minutes:00}:{seconds:00}";
        }
        else
        {
            timerText.text = "Attack£¡";
        }

        if (waveText != null)
        {
            int wave = enemyAI.currentWaveCount;
            waveText.text = $"{wave}";
        }
    }
}
