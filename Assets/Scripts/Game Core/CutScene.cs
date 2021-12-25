using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CutScene : MonoBehaviour
{
    public string CutsceneName;
    public TextMeshProUGUI NarrationTextBox;
    public Image CharacterImage;
    int dialogueIndex = 0;
    public string[] dialogueTexts = { };
    public float[] dialogueDelays;
    int subsectionIndex = 0;
    public GameObject [] subsections;
    public GameObject[] objectsToDeactivate;
    public float [] subsectionDelays;
    public GameObject[] StartSequenceDeactivations;
    public GameObject[] EndSequenceActivations;
    public bool doRollingDeactivate = true; // If cutscene should simply deactivate sections when moving on to the next
    public bool doFreezeEnemies = false; // If cutscene should pause enemies
    public bool doFreezePlayer = false; // If cutscene should pause player
    public bool doRecursiveCutscenes = false; // If cutscene should consider lengths of sub-cutscenes within it
    [SerializeField]
    private HeroController player;
    private Enemy[] allEnemies;
    public EndMode endMode = EndMode.None;
    public enum EndMode
    {
        None,          // Does nothing upon completion
        SelfDestruct, // Destroys the cutscene after completion
        BeginLevel, // Cutscenes which initialize the level
        EndLevel,   // Automatically proceeds to the next level
        AdvanceLevel    // Continues the level
    }
    //public List<GameObject[]> subsectionObjects;
    //public List<float[]> subsectionObjectDelays;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;
        if (NarrationTextBox == null) NarrationTextBox = GameMaster.Instance.NarrationTextBox;
        if (CharacterImage == null) CharacterImage = GameMaster.Instance.CharacterImage;
        if(StartSequenceDeactivations!=null && StartSequenceDeactivations.Length > 0)
        {
            for (int i = 0; i < StartSequenceDeactivations.Length; i++)
                StartSequenceDeactivations[i].SetActive(false);
        }
        InitializeDialogueTexts();
        if (subsections.Length != subsectionDelays.Length)
            Debug.LogError("Uh oh! Stinky! Poopy! Subsections length isn't equal to subsection delays length!");
        if (dialogueTexts.Length != dialogueDelays.Length)
            Debug.LogError("Uh oh! Stinky! Poopy! Dialogues length isn't equal to dialogue delays length!");
        if (doRollingDeactivate) objectsToDeactivate = subsections;

        // For each subsection, if they are a cutscene, make sure the subsection delay is equal to the length of that segment.
        if(doRecursiveCutscenes)
        {
            for(int i = 0; i < subsections.Length; i++)
            {
                CutScene subScene = subsections[i].GetComponent<CutScene>();
                if(subScene != null)
                {
                    subsectionDelays[i] = subScene.getTotalLength();
                }
            }
        }
        StartCoroutine(SectionsSequence());
        StartCoroutine(TextSequence());

        if (player == null)
            player = FindObjectOfType<HeroController>();

        if (doFreezePlayer)
            TogglePlayer(false);

        if (doFreezeEnemies)
            ToggleEnemies(false);

    }

    public float getTotalLength()
    {
        float totalLength = 0;
        for(int i = 0; i < subsectionDelays.Length; i++)
        {
            totalLength += subsectionDelays[i];
        }
        return totalLength;
    }

    public void InitializeDialogueTexts()
    {
        if (CutsceneName == "") return;
        //string filename = CutsceneName + GameMaster.language;
        TextAsset fileText = Resources.Load(string.Format("Dialogue/{0}/{1}", GameMaster.language, CutsceneName)) as TextAsset;
        dialogueTexts = fileText.text.Split('\n');
    }
  
    public IEnumerator TextSequence()
    {
        if (dialogueTexts.Length == 0 || NarrationTextBox == null) yield break;
        NarrationTextBox.gameObject.SetActive(true);
        for (int i = 0; i < dialogueTexts.Length; i++)
        {
            HandleTokens(i);
            NarrationTextBox.text = dialogueTexts[i];
            yield return new WaitForSeconds(dialogueDelays[i]);
        }
        NarrationTextBox.gameObject.SetActive(false);
        CharacterImage.gameObject.SetActive(false);
    }

    private void HandleTokens(int i)
    {
        //this could be broken down with a method but I'm lazy
        if (!dialogueTexts[i].Contains("&")) //if includes a character name for image setting
        {
            CharacterImage.gameObject.SetActive(false);
            NarrationTextBox.color = Color.white;
            return;
        }


            string[] tokens = dialogueTexts[i].Split('&');
            dialogueTexts[i] = tokens[0]; //the actual dialogue
            string name = tokens[1];
            //Debug.Log(string.Format("Sprites/{0}", name));
            Sprite profile = Resources.Load<Sprite>(string.Format("Sprites/{0}", name));
            CharacterImage.sprite = profile;
            CharacterImage.gameObject.SetActive(true);

        if (tokens.Length < 4)
        {
            NarrationTextBox.color = Color.white;
            return; //avoid outofindex exceptions
        }

        string color = tokens[2];
        Color col = new Color();
        //what the actual fuck Unity? Could you not have, you know, made this method return a Color?
        if(!ColorUtility.TryParseHtmlString(color, out col))
            Debug.LogError("Cut! Cut! Something went wrong.");

        NarrationTextBox.color = col;
        
    }

    public IEnumerator SectionsSequence()
    {
        if(subsections.Length == 0) yield break;
        for(int i = 0; i < subsections.Length; i++)
        {
         //   Debug.Log("Handling subsection " + i);
            if (i != 0)
                if(objectsToDeactivate[i-1]!=null)
                    objectsToDeactivate[i - 1].SetActive(false);//deactivate previous

            if (subsections[i] != null)
                subsections[i].SetActive(true); //activate current
            yield return new WaitForSeconds(subsectionDelays[i]);
        }
        //  subsections[subsections.Length - 1].SetActive(false); //deactivate last subsection
        End();
    }

    public void Update()
    {
        if (endMode == EndMode.BeginLevel || endMode == EndMode.EndLevel || endMode == EndMode.AdvanceLevel) //allow breaking cutscenes to begin level
        {
            if (Input.GetKeyDown(KeyCode.Escape)) 
            {
                StopAllCoroutines(); //stop the sequence
                for (int i = 0; i < subsections.Length; i++) //deactivate all subsections
                    if(subsections[i])subsections[i].SetActive(false);
                End();
            }
        }
    }

    public void End()
    {
        NarrationTextBox.gameObject.SetActive(false);
        CharacterImage.gameObject.SetActive(false);

        switch (endMode)
        {
            case EndMode.SelfDestruct:
                Destroy(this);
                break;
            case EndMode.BeginLevel:
                BeginLevel();
                break;
            case EndMode.EndLevel:
                GameMaster.UploadPlayer();
                GameMaster.Instance.LoadNextLevel();
                break;
            case EndMode.AdvanceLevel:
                AdvanceLevel();
                break;
        }

    }

    private void BeginLevel()
    {
        if(doRollingDeactivate)
            subsections[subsections.Length - 1].SetActive(false); //how is this not getting set to false before??
        //turns out, it is critical to unfreeze the player BEFORE enemies become initialized
        if (doFreezePlayer)
            TogglePlayer(true);

        for (int i = 0; i < EndSequenceActivations.Length; i++)
            EndSequenceActivations[i].SetActive(true);
        GameMaster.Instance.StartLevel();
        gameObject.SetActive(false);
        if (doFreezeEnemies)
            ToggleEnemies(true);
 
    }

    void AdvanceLevel()
    {
        if (doFreezePlayer)
            TogglePlayer(true);

        for (int i = 0; i < EndSequenceActivations.Length; i++)
            EndSequenceActivations[i].SetActive(true);
        gameObject.SetActive(false);
    }

    public float TotalSubsectLength()
    {
        float total = 0;
        for (int i = 0; i < subsectionDelays.Length; i++)
            total += subsectionDelays[i];

        return total;
    }

    private void ToggleEnemies(bool On)
    {
            if(allEnemies==null || allEnemies.Length ==0)allEnemies = FindObjectsOfType<Enemy>();
            for (int i = 0; i < allEnemies.Length; i++)
                allEnemies[i].enabled = On;
    }

    private void TogglePlayer(bool On)
    {
        player.gameObject.SetActive(On);
        //player.enabled = On;
        //player.Blind(On);
    }
    
}
