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
    public float [] subsectionDelays;
    public GameObject[] EndSequenceActivations;
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
        StartCoroutine(SectionsSequence());
        StartCoroutine(TextSequence());
    }

    public void InitializeDialogueTexts()
    {
        if (CutsceneName == "") return;
        string filename = CutsceneName + GameMaster.language;
        TextAsset fileText = Resources.Load("Dialogue/" + filename) as TextAsset;
        dialogueTexts = fileText.text.Split('\n');

       // for (int i = 0; i < dialogueTexts.Length; i++)
         //   Debug.Log(dialogueTexts[i]);
    }

    public IEnumerator TextSequence()
    {
        if (dialogueTexts.Length == 0 || NarrationTextBox == null) yield break;
        for (int i = 0; i < dialogueTexts.Length; i++)
        {
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
                if(subsections[i-1]!=null)subsections[i - 1].SetActive(false);//deactivate previous
            if(subsections[i]!=null)subsections[i].SetActive(true); //activate current
            yield return new WaitForSeconds(subsectionDelays[i]);
        }
      //  subsections[subsections.Length - 1].SetActive(false); //deactivate last subsection

    }

    
}
