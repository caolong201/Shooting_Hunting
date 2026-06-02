using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using IE.RSB;

public class EnemyIndicator : MonoBehaviour
{
    [SerializeField] private GameObject Indicator;
    [SerializeField] private Canvas canvas;
    [SerializeField] private Camera mainCamera;
    private List<Enemy> enemies = new List<Enemy>();
    private List<GameObject> arrowss = new List<GameObject>();
    [SerializeField] GameObject IndicatorEnemy;

    private float padding = 50f;
    private bool isGameClear = false;
    private bool isOpen = true;
    private bool isPaused = false;
    private void Start()
    {
        mainCamera = Camera.main;
    }

    public void closeIndicator()
    {
        if (!isOpen) return;
        IndicatorEnemy.SetActive(false);

        foreach (var arrow in arrowss)
        {
            if (arrow != null) arrow.SetActive(false); 
        }

        isOpen = false;
    }

    public void OpenIndicator()
    {
        if (isOpen) return;
        IndicatorEnemy.SetActive(true);
        isOpen = true;
    }


    public void SetGameClear(bool isClear)
    {
        isGameClear = isClear;
    }


    public void SetSize(float Size, bool isAnim)
    {
        foreach (var arrow in arrowss)
        {
            if (isAnim)
            {
                arrow.transform.DOScale(Vector3.one * Size, 0.8f);
            }
            else
            {
                arrow.transform.localScale = Vector3.one * Size;
            }
        }
    }

    public void SetEnemies(List<Enemy> enemyList)
    {
        foreach (var arrow in arrowss)
        {
            Destroy(arrow);
        }

        arrowss.Clear();
        enemies = enemyList;

        foreach (var enemy in enemies)
        {
            GameObject arrow = Instantiate(Indicator, canvas.transform);
            arrow.SetActive(false);
            Image img = arrow.GetComponent<Image>();
            if (img != null)
            {
                img.color = Color.white;
                img.DOColor(enemy.IsGoldRabbit ? Color.yellow : Color.red, 0.5f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.Linear);
            }


            arrowss.Add(arrow);
        }
    }


    public void PauseIndicator(bool pause)
    {
        isPaused = pause;
        IndicatorEnemy.SetActive(!pause);

        foreach (var arrow in arrowss)
        {
            if (arrow != null) arrow.SetActive(!pause);
        }


    }
    private void Update()
    {
        if (isPaused) return;

        for (int i = 0; i < enemies.Count; i++)
        {
            UpdateArrow(enemies[i], arrowss[i]);
        }
    }

    private void UpdateArrow(Enemy enemy, GameObject arrow)
    {
        //if (enemy == null || enemy.isDead)
        //{
        //    arrow.SetActive(false);
        //    return;
        //}
        if (enemy == null || enemy.isDead || isGameClear)
        {
            arrow.SetActive(false);
            return;
        }
        Vector3 screenPoint = mainCamera.WorldToScreenPoint(enemy.pointer.position);
        bool isOffScreen = screenPoint.z < 0 ||
                           screenPoint.x < 0 || screenPoint.x > Screen.width ||
                           screenPoint.y < 0 || screenPoint.y > Screen.height;

        if (isOffScreen)
        {
            arrow.SetActive(false);
            return;
        }

        arrow.SetActive(true);


        arrow.transform.position = screenPoint;
    }





}