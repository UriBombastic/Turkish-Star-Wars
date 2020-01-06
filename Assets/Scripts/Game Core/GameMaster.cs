﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class GameMaster : MonoBehaviour
{
    //instance
    private static GameMaster _instance;
    public static GameMaster Instance
    {
        get
        {
           if(_instance==null) _instance = FindObjectOfType<GameMaster>();
            return _instance;
        }
    }

    //enums
    public enum Language
    {
        EN,
        SP,
        TUR,
        FUK
    }

    public static Language language = Language.EN;

    public enum LevelObjective
    {
        None,
        ReachGoal,
        KillAllEnemies,
//        KillAllEnemiesSet,
        KillBoss
    }

    public LevelObjective CurrentObjective = LevelObjective.ReachGoal;

    //statics
    public static bool AllowTerribleNoises;
    private static HeroController savedPlayer;
    public static bool isPaused;

    //player
    private HeroController _player;
    private LookDirectionController LDC;
    //Enemies to Kill
    [SerializeField]
    private int EnemiestToKill;
    [SerializeField]
    private int EnemyKillCount;


    //Objectives Display
    public string ObjectiveExplanation;
    public GameObject LevelEndGoal;
    public GameObject LevelGoalDisplay;
    public TextMeshProUGUI GoalText;
    public float GoalDisplayStartTime = 1f;
    public float GoalDisplayDuration = 3f;

    //level complete UI
    public bool ShowLevelComplete;
    public GameObject LevelCompleteUI;

    //upgrade UI
    public UpgradePlayer upgradeUI;

    //pause menu
    public GameObject PauseMenu;
    public bool CanPause = true;

    public void Awake()
    {
        _player = FindObjectOfType<HeroController>();
        LDC = FindObjectOfType<LookDirectionController>();
    }

    public void IncrementEnemyKillCount()
    {
        EnemyKillCount++;
        if(CurrentObjective == LevelObjective.KillAllEnemies)
        {
            if (EnemyKillCount == EnemiestToKill)
            {
                LevelEndGoal.SetActive(true);
                if (ShowLevelComplete) LevelCompleteUI.SetActive(true);
            }
        }
    }

    public void StartLevel()
    {
        if (CurrentObjective != LevelObjective.None)
            StartCoroutine(ObjectivesFlash());
    }

   IEnumerator ObjectivesFlash()
    {
        LevelGoalDisplay.SetActive(true);
        yield return new WaitForSeconds(GoalDisplayStartTime);
        GoalText.text = ObjectiveExplanation;
        yield return new WaitForSeconds(GoalDisplayDuration);
        LevelGoalDisplay.SetActive(false);

    }

    public void EndLevel()
    {
        Debug.Log("Level Ended!");
        Pause(true);
        CanPause = false;
        upgradeUI.gameObject.SetActive(true);
       // LoadNextLevel();
    }

    public static void TogglePause()
    {
        Instance.Pause(!isPaused, true);
    }

    public void Pause(bool tf, bool showPauseMenu = false) //true for pause, false for unpause
    {
        if (!CanPause) return;
        if (LDC == null) LDC = FindObjectOfType<LookDirectionController>();
        if(LDC!=null)LDC.enabled = !tf;
        Time.timeScale = (tf ? 0f : 1f);
        Cursor.lockState = (tf ? CursorLockMode.None : CursorLockMode.Locked);
        isPaused = tf;
        LevelGoalDisplay.SetActive(tf);
        if (showPauseMenu) PauseMenu.SetActive(tf);
    }

    public void HealPlayer(float amount = 0.5f)
    {
        _player.health = Mathf.Max(_player.maxHealth, _player.health + _player.maxHealth * amount);
    }

    public static void LoadNextLevel()
    {

    }

    public static void UploadPlayer()
    {
        savedPlayer = Instance._player;
    }

    public static void DownloadPlayer()
    {
        Instance._player = savedPlayer;
    }

    public void Save()
    {

    }

    public void ExitGame()
    {
        Save();
        Application.Quit();
    }

}