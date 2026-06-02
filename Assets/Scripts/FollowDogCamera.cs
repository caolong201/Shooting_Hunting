using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class FollowDogCamera : MonoBehaviour
{
    [Header("Follow")]
    private Transform target;
    public Vector3 offset = new Vector3(0f, 6f, -8f); // offset theo hướng của dog
    public float followSmooth = 10f;
    public bool lookAtTarget = true;
    public float lookSmooth = 12f;

    [Header("Return Home")]
    public float returnDuration = 1.2f;

    // home pose (vị trí ban đầu khi kích hoạt camera)
    private Vector3 homePos;
    private Quaternion homeRot;

    private bool following = false;
    private bool returning = false;


    public void ActivateFollow(Transform dog)
    {
        target = dog;
        following = true;
        returning = false;
        
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.Euler(Vector3.zero);
        
        homePos = transform.position;
        homeRot = transform.rotation;
    }

    public IEnumerator ReturnHome(System.Action onDone = null)
    {
        following = false;
        returning = true;

        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, returnDuration);
            transform.position = Vector3.Lerp(startPos, homePos, t);
            transform.rotation = Quaternion.Slerp(startRot, homeRot, t);
            yield return null;
        }

        returning = false;
        onDone?.Invoke();
    }

    void LateUpdate()
    {
        if (following && target != null)
        {
            // offset theo hướng của dog để luôn "đi sau lưng"
            Vector3 desiredPos = target.position + target.TransformDirection(offset);
            transform.position = Vector3.Lerp(transform.position, desiredPos, followSmooth * Time.deltaTime);

            if (lookAtTarget)
            {
                var desiredRot = Quaternion.LookRotation(target.position - transform.position, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, lookSmooth * Time.deltaTime);
            }
        }
    }
}
