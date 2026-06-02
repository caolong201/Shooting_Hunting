using System;
using UnityEngine;
using DG.Tweening;
using Random = UnityEngine.Random;
using IE.RSB;

public class EnemyPatrol : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    private float moveDuration = 2f;
    private float idleDuration = 3f;

    private Animator animator;
    private bool movingToB = true;

    private Vector3 posA, posB;
    private Tween currentTween;
    [SerializeField] bool usePhysicMove = false;

    private bool isInit = false;
    private Enemy enemy = null;

    public void StartMove()
    {
        if (pointA == null || pointB == null)
            return;

        Debug.Log("StartMove");
        animator = GetComponent<Animator>();
        posA = pointA.position;
        posB = pointB.position;

        enemy = GetComponent<Enemy>();
        MoveToNextPoint();
    }

    public void StopMove()
    {
        Debug.Log("StopMove");
        if (currentTween != null && currentTween.IsActive())
        {
            currentTween.Kill();
        }

        if (enemy != null) enemy.IsRunningAnimal = false;
    }

    void MoveToNextPoint()
    {
        if (!usePhysicMove)
        {
            Vector3 target = movingToB ? posB : posA;
            moveDuration = Vector3.Distance(transform.position, target) / 2f;

            // Play walking animation
            idleDuration = Random.Range(3f, 5f);
            animator?.SetInteger("AnimationNo", 2);
            if (enemy != null) enemy.IsRunningAnimal = true;

            float delayDuration = 0f;
            if (isInit)
            {
                transform.DOLookAt(target, 2f);
                delayDuration = 1;
            }

            currentTween = DOVirtual.DelayedCall(delayDuration, () =>
            {
                currentTween = transform.DOMove(target, moveDuration)
                    .SetEase(Ease.Linear)
                    .OnComplete(() =>
                    {
                        isInit = true;
                        // Switch to idle animation
                        animator?.SetInteger("AnimationNo", -1);
                        if (enemy != null) enemy.IsRunningAnimal = false;

                        // Wait for idleDuration seconds before moving again
                        currentTween = DOVirtual.DelayedCall(idleDuration, () =>
                        {
                            movingToB = !movingToB; // Reverse direction
                            MoveToNextPoint();
                        });
                    });
            });
        }
        else
        {
            Vector3 target = movingToB ? posB : posA;
            moveDuration = Vector3.Distance(transform.position, target) / 2f;

            // Play walking animation
            idleDuration = Random.Range(3f, 5f);
            animator?.SetInteger("AnimationNo", 2);
            if (enemy != null) enemy.IsRunningAnimal = true;
            transform.DOLookAt(target, 2f);
            currentTween = DOVirtual.DelayedCall(1f, () =>
            {
                currentTween = DOVirtual.DelayedCall(moveDuration, () =>
                {
                    // Switch to idle animation
                    animator?.SetInteger("AnimationNo", -1);
                    if (enemy != null) enemy.IsRunningAnimal = false;
                    // Wait for idleDuration seconds before moving again
                    currentTween = DOVirtual.DelayedCall(idleDuration, () =>
                    {
                        movingToB = !movingToB; // Reverse direction
                        MoveToNextPoint();
                    });
                });
            });
        }
    }
}