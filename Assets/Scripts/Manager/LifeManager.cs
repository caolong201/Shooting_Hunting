using UnityEngine;
using System;
using System.Collections;

public class LifeManager : SingletonMonoAwake<LifeManager>
{
    private const int MAX_LIFES = 5;
    private const int RESTORE_TIME_MINUTES = 30;
    private const string LIFE_KEY = "PlayerLifes";
    private const string LAST_RESTORE_TIME_KEY = "LastRestoreTime";

    public int CurrentLifes { get; private set; }
    private DateTime lastRestoreTime;

    public override void OnAwake()
    {
        base.OnAwake();
        LoadLifes();
        StartCoroutine(AutoRestoreLifes());
    }

    private void LoadLifes()
    {
        CurrentLifes = PlayerPrefs.GetInt(LIFE_KEY, MAX_LIFES);
        string lastTimeString = PlayerPrefs.GetString(LAST_RESTORE_TIME_KEY, "");

        if (!string.IsNullOrEmpty(lastTimeString))
            lastRestoreTime = DateTime.Parse(lastTimeString);
        else
            lastRestoreTime = DateTime.Now;
    }

    private IEnumerator AutoRestoreLifes()
    {
        while (true)
        {
            if (CurrentLifes == MAX_LIFES) lastRestoreTime = DateTime.Now;
            else if (CurrentLifes < MAX_LIFES)
            {
                TimeSpan timePassed = DateTime.Now - lastRestoreTime;
                int lifesToRestore = (int)(timePassed.TotalMinutes / 30);

                if (lifesToRestore > 0)
                {
                    CurrentLifes = Mathf.Min(CurrentLifes + lifesToRestore, MAX_LIFES);
                    lastRestoreTime = DateTime.Now;
                    SaveLifes();
                }
            }

            yield return new WaitForSeconds(60); // Check every 60 seconds
        }
    }

    public void LoseLife()
    {
        if (CurrentLifes > 0)
        {
            if(CurrentLifes == MAX_LIFES) lastRestoreTime = DateTime.Now;
            CurrentLifes--;

            SaveLifes();
        }
    }

    public void AddLife(bool isFullLife = false)
    {
        if (isFullLife)
        {
            CurrentLifes = MAX_LIFES;
        }
        else
        {
            if (CurrentLifes < MAX_LIFES)
            {
                CurrentLifes++;
            }
        }

        // if (CurrentLifes == MAX_LIFES) lastRestoreTime = DateTime.Now;

        SaveLifes();
    }

    public void SaveLifes()
    {
        PlayerPrefs.SetInt(LIFE_KEY, CurrentLifes);
        PlayerPrefs.SetString(LAST_RESTORE_TIME_KEY, lastRestoreTime.ToString());
        PlayerPrefs.Save();
    }

    public string GetTimeUntilNextLife()
    {
        if (CurrentLifes >= MAX_LIFES)
            return "FULL";

        TimeSpan timeSinceLastRestore = DateTime.Now - lastRestoreTime;
        int elapsedSeconds = (int)timeSinceLastRestore.TotalSeconds;

        int remainingSeconds = (RESTORE_TIME_MINUTES * 60) - elapsedSeconds;
        if (remainingSeconds <= 0)
            return "00:00";

        int minutes = remainingSeconds / 60;
        int seconds = remainingSeconds % 60;

        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public bool IsFullLife()
    {
        return CurrentLifes >= MAX_LIFES;
    }

    private void Update()
    {
        //cheat
        if (Input.GetKeyDown(KeyCode.A))
        {
            CurrentLifes = 0;
        }
    }
}