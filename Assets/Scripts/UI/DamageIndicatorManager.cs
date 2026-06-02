using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using IE.RSB;

public class DamageIndicatorManager : MonoBehaviour
{
    [SerializeField] private Canvas canvas; 
    [SerializeField] private Camera mainCamera;
    [SerializeField] private GameObject damageTextPrefab;

    private List<Enemy> enemies = new List<Enemy>();
    private List<TextMeshProUGUI> damageTexts = new List<TextMeshProUGUI>();

    private void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    public void SetEnemies(List<Enemy> enemyList)
    {
        foreach (var txt in damageTexts)
            Destroy(txt.gameObject);

        damageTexts.Clear();
        enemies = enemyList;

        foreach (var enemy in enemies)
        {
            GameObject go = Instantiate(damageTextPrefab, canvas.transform);
            go.SetActive(false);
            damageTexts.Add(go.GetComponent<TextMeshProUGUI>());
        }
    }

    public void ShowDamage(Enemy enemy, int percentLost)
    {
        int index = enemies.IndexOf(enemy);
        if (index < 0) return;

        TextMeshProUGUI txt = damageTexts[index];
        if (txt == null) return;

        txt.text = $"-{percentLost}";
        txt.color = Color.red;
        txt.alpha = 1f;
        txt.transform.localScale = Vector3.one * 0.5f;
        txt.rectTransform.anchoredPosition = Vector2.zero;
        txt.gameObject.SetActive(true);

        Sequence seq = DOTween.Sequence();
        seq.Append(txt.transform.DOScale(1.2f, 0.1f).SetEase(Ease.OutBack))
           .Join(txt.rectTransform.DOAnchorPosY(50f, 0.5f))
           .Append(txt.DOFade(0, 0.5f))
           .OnComplete(() => txt.gameObject.SetActive(false));
    }

    private void Update()
    {
      
        for (int i = 0; i < enemies.Count; i++)
        {
            var enemy = enemies[i];
            var txt = damageTexts[i];
            if (enemy == null || enemy.isDead || txt == null) continue;

            Vector3 screenPoint = mainCamera.WorldToScreenPoint(enemy.pointer.position);
            txt.transform.position = screenPoint;
        }
    }
}
