﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
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

    //level handling
    private static string[] levels = { "01.DesertFight", "02.Captured", "03.Arena", "04.Cave"};
    private static int LevelIndex = 0;

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
    public static bool isPaused;

    //static player
    //private static HeroController savedPlayer; //TODO: replace this with variables for each individual field modified by UpgradePlayer
    protected static bool hasUploaded = false;
    private static float playerAtkMult=0f;
    private static float playerMaxHealth=0f;
    private static float playerCurrentHealth=0f;
 //   private static float playerMaxShieldTime=0f;
    private static float playerShieldDegradeFactor=0f;
 //   private static float playerShieldCoolDown=0f;
    private static float playerMaxShieldPower = 0f;
    private static float playershieldRegenFactor = 0f;
    private static int playerShieldLevel=0;

    //player
    [SerializeField]
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

    //Deaths 
    public GameObject DeathScreen;

    //upgrade UI
    public UpgradePlayer upgradeUI;

    //pause menu
    public GameObject PauseMenu;
    public bool CanPause = true;

    //Cutscenes
    public TextMeshProUGUI NarrationTextBox;
    public Image CharacterImage;



    //starting functions
    public void Awake()
    {
        if(_player==null)_player = FindObjectOfType<HeroController>();
        LDC = FindObjectOfType<LookDirectionController>();
        if (hasUploaded)
            DownloadPlayer();
        else
            Debug.Log("No player saved; not downloading player");

}

    public void StartLevel()
    {
        if (CurrentObjective != LevelObjective.None)
            StartCoroutine(ObjectivesFlash());
    }

    //objectives funcitons
    public void IncrementEnemyKillCount()
    {
        EnemyKillCount++;
        if (CurrentObjective == LevelObjective.KillAllEnemies)
        {
            if (EnemyKillCount == EnemiestToKill)
            {
                LevelEndGoal.SetActive(true);
                if (ShowLevelComplete) LevelCompleteUI.SetActive(true);
            }
        }
    }


    IEnumerator ObjectivesFlash()
    {
        LevelGoalDisplay.SetActive(true);
        yield return new WaitForSeconds(GoalDisplayStartTime);
        GoalText.text = ObjectiveExplanation;
        yield return new WaitForSeconds(GoalDisplayDuration);
        LevelGoalDisplay.SetActive(false);

    }

    //pause
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
        Cursor.visible = tf;
        isPaused = tf;
        if(showPauseMenu)LevelGoalDisplay.SetActive(tf);
        if (showPauseMenu) PauseMenu.SetActive(tf);
    }

    public void HealPlayer(float amount = 0.5f)
    {
        _player.health = Mathf.Min(_player.maxHealth, _player.health + _player.maxHealth * amount);
    }

    //level transitions
    public void EndLevel()
    {
        Debug.Log("Level Ended!");
        Pause(true);
        CanPause = false;
        upgradeUI.gameObject.SetActive(true);
    }

    public void StartGame()
    {
        SceneManager.LoadScene("01.DesertFight");
    }

    public static void LoadNextLevel()
    {
        LevelIndex++;
        LoadCurrentLevel();
    }

    public static void LoadCurrentLevel()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(levels[LevelIndex]); 
    }

    //restart level after death; potentially heal player, should be accessible via button rather than statically
    public void RestartLevel()
    {
        HealPlayer(0.1f); //heal player for 10% of their max health
        LoadCurrentLevel();
    }

    public static void UploadPlayer()
    {
        // savedPlayer = Instance._player;
        if(Instance._player==null)Instance._player = FindObjectOfType<HeroController>();
        if (Instance._player == null) return;

        playerAtkMult =Instance._player.AttackDamageMultiplier;
        playerMaxHealth = Instance._player.maxHealth;
        playerCurrentHealth = Instance._player.health;
        playerShieldDegradeFactor = Instance._player.shieldDegradeFactor;
        playerShieldLevel = Instance._player.ShieldLevel;
        playerMaxShieldPower = Instance._player.maxShieldPower;
        playershieldRegenFactor = Instance._player.shieldRegenFactor;
        // playerMaxShieldTime = Instance._player.maxShieldTime;
        //  playerShieldCoolDown = Instance._player.ShieldCoolDown;
        hasUploaded = true;
    }

    public static void DownloadPlayer()
    {
        if (Instance._player == null) Instance._player = FindObjectOfType<HeroController>();
        if (Instance._player == null) return; //just stop lol
        //  Instance._player = savedPlayer;
        Instance._player.AttackDamageMultiplier = playerAtkMult;
        Instance._player.maxHealth = playerMaxHealth;
        Instance._player.health = playerCurrentHealth;
        Instance._player.shieldDegradeFactor = playerShieldDegradeFactor;
        Instance._player.maxShieldPower = playerMaxShieldPower;
        Instance._player.shieldRegenFactor = playershieldRegenFactor;
        Instance._player.ShieldLevel = playerShieldLevel;
        //  Instance._player.maxShieldTime = playerMaxShieldTime;
        //   Instance._player.ShieldCoolDown = playerShieldCoolDown;
    }

    //death
    public void HandleDeath()
    {
        Pause(true);
        Instance.DeathScreen.SetActive(true);
        CanPause = false;
    }

    //termination functions
    public void Save()
    {
 
    }

    public void ExitGame()
    {
        Save();
        Application.Quit();
    }

}
