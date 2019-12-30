using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class GameMaster : MonoBehaviour
{
    private static GameMaster _instance;
    public static GameMaster Instance
    {
        get
        {
           if(_instance==null) _instance = FindObjectOfType<GameMaster>();
            return _instance;
        }
    }

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


    public void IncrementEnemyKillCount()
    {
        EnemyKillCount++;
        if(CurrentObjective == LevelObjective.KillAllEnemies)
        {
            if (EnemyKillCount == EnemiestToKill)
                LevelEndGoal.SetActive(true);
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
        Time.timeScale = 0f;
        LoadNextLevel();
    }

    public static void LoadNextLevel()
    {

    }
}
