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
    public GameObject[] EndSequenceActivations;
    public bool doRollingDeactivate = true;
    public EndMode endMode = EndMode.None;
    public enum EndMode
    {
        None,
        SelfDestruct,
        BeginLevel,
        EndLevel
    }
    //public List<GameObject[]> subsectionObjects;
    //public List<float[]> subsectionObjectDelays;

    // Start is called before the first frame update
    void Start()
    {
        InitializeDialogueTexts();
        if (subsections.Length != subsectionDelays.Length)
            Debug.LogError("Uh oh! Stinky! Poopy! Subsections length isn't equal to subsection delays length!");
        if (dialogueTexts.Length != dialogueDelays.Length)
            Debug.LogError("Uh oh! Stinky! Poopy! Dialogues length isn't equal to dialogue delays length!");
        if (doRollingDeactivate) objectsToDeactivate = subsections;
        StartCoroutine(SectionsSequence());
        StartCoroutine(TextSequence());
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
        for (int i = 0; i < dialogueTexts.Length; i++)
        {
            if (dialogueTexts[i].Contains("&")) //if includes a character name for image setting
            {
                string[] tokens = dialogueTexts[i].Split('&');
                dialogueTexts[i] = tokens[0]; //the actual dialogue
                string name = tokens[1];
                Sprite profile = Resources.Load<Sprite>(string.Format("Sprites/{0}",name));
                CharacterImage.sprite = profile;
                CharacterImage.gameObject.SetActive(true);
            }
            else
            {
                CharacterImage.gameObject.SetActive(false);
            }
            NarrationTextBox.text = dialogueTexts[i];
            yield return new WaitForSeconds(dialogueDelays[i]);
        }
        NarrationTextBox.gameObject.SetActive(false);
    }

    public IEnumerator SectionsSequence()
    {
        if(subsections.Length == 0) yield break;
        for(int i = 0; i < subsections.Length; i++)
        {
         //   Debug.Log("Handling subsection " + i);
            if (i != 0)
                if(objectsToDeactivate[i-1]!=null)objectsToDeactivate[i - 1].SetActive(false);//deactivate previous
            if (subsections[i] != null)
            {
                subsections[i].SetActive(true); //activate current
            }
            yield return new WaitForSeconds(subsectionDelays[i]);
        }
        //  subsections[subsections.Length - 1].SetActive(false); //deactivate last subsection
        End();
    }

    public void Update()
    {
        if (endMode == EndMode.BeginLevel) //allow breaking cutscenes to begin level
        {
            if (Input.GetKeyDown(KeyCode.Escape)) 
            {
                StopAllCoroutines(); //stop the sequence
                for (int i = 0; i < subsections.Length; i++) //deactivate all subsections
                    subsections[i].SetActive(false);
                End();
            }
        }
    }

    public void End()
    {
        NarrationTextBox.gameObject.SetActive(false);
        switch (endMode)
        {
            case EndMode.SelfDestruct:
                Destroy(this);
                break;
            case EndMode.BeginLevel:
                for (int i = 0; i < EndSequenceActivations.Length; i++)
                    EndSequenceActivations[i].SetActive(true);
                CharacterImage.gameObject.SetActive(false);
                GameMaster.Instance.StartLevel();
                gameObject.SetActive(false);
                break;
        }
    }
    public float TotalSubsectLength()
    {
        float total = 0;
        for (int i = 0; i < subsectionDelays.Length; i++)
            total += subsectionDelays[i];

        return total;
    }

    
}
