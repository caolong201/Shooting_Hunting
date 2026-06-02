using System.Collections;
using UnityEngine;
using System;

public class ArrowSpinner : MonoBehaviour
{
    public float spinSpeed = 100f; // Tốc độ quay
    private float minAngle = -90f;
    private float maxAngle = 90f;
    private bool rotatingRight = true; // Quay phải trước
    private float currentAngle = 0f;
    private bool isStarted = false;

    public void StartPin()
    {
        isStarted = true;
    }

    public int StoptPin()
    {
        isStarted = false;
        return GetMultiplier();
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            isStarted = true;
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            isStarted = false;
            int a = GetMultiplier();
            Debug.LogError("Stop: " + a);
        }
        
        if(!isStarted) return;
        
        float rotationStep = spinSpeed * Time.deltaTime;

        if (rotatingRight)
        {
            currentAngle += rotationStep;
            if (currentAngle >= maxAngle)
            {
                currentAngle = maxAngle;
                rotatingRight = false; // Đổi hướng quay
            }
        }
        else
        {
            currentAngle -= rotationStep;
            if (currentAngle <= minAngle)
            {
                currentAngle = minAngle;
                rotatingRight = true; // Đổi hướng quay
            }
        }

        transform.rotation = Quaternion.Euler(0, 0, currentAngle);
    }
    
    public int GetMultiplier()
    {
        // Normalize angle to be within 0 to 180 degrees
        float angle = Mathf.Repeat(currentAngle, 180);

        if (angle >= 60 && angle <= 120)
            return 2; // Green Zone (8X)
        else if ((angle >= 30 && angle < 60) || (angle > 120 && angle <= 150))
            return 4; // Yellow Zones (4X)
        else
            return 8; // Red Zones (2X)
    }
    
}
