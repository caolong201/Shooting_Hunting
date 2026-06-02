using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class TransitionWave : MonoBehaviour
{
    Action _callback;

    public void StartTransition(Vector3 targetMove, Vector3 rotation, Action callback)
    {
        this._callback = callback;
        Vector3 direction = (targetMove - transform.position).normalized;
        direction.y = 0;

        float targetY = Quaternion.LookRotation(direction).eulerAngles.y;
        Vector3 currentEuler = transform.eulerAngles;
        Vector3 finalEuler = new Vector3(0f, targetY, 0f);
        float angleDiff = Mathf.DeltaAngle(currentEuler.y, targetY);
        float adjustedY = currentEuler.y + angleDiff;
        finalEuler.y = adjustedY;


        transform.DORotate(finalEuler, 0.5f).SetEase(Ease.OutSine).OnComplete(() =>
        {
            var distance = Vector3.Distance(transform.position, targetMove);
            distance /= 6.5f;
            
            transform.DOMove(targetMove, distance).SetEase(Ease.Linear).OnComplete(() =>
            {
                transform.DOKill();
                transform.DORotate(rotation, .5f).SetEase(Ease.OutSine).OnComplete(() => { _callback?.Invoke(); });
            });
        });
    }
}