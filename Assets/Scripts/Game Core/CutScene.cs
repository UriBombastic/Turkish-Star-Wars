using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;

public class CutScene : MonoBehaviour
{
    public string CutsceneName;
    public TextMeshProUGUI NarrationTextBox;
    int dialogueIndex = 0;
    public string[] dialogueTexts;
    public float[] dialogueDelays;
    int subsectionIndex = 0;
    public GameObject [] subsections;
    public float [] subsectionDelays;
    //public List<GameObject[]> subsectionObjects;
    //public List<float[]> subsectionObjectDelays;

    // Start is called before the first frame update
    void Start()
    {
        if (subsections.Length != subsectionDelays.Length)
            Debug.LogError("Uh oh! Stinky! Poopy! Subsections length isn't equal to subsection delays length!");
        if (dialogueTexts.Length != dialogueDelays.Length)
            Debug.LogError("Uh oh! Stinky! Poopy! Dialogues length isn't equal to dialogue delays length!");
        InitializeDialogueTexts();
        StartCoroutine(SectionsSequence());
        StartCoroutine(TextSequence());
    }

    public void InitializeDialogueTexts()
    {

    }

    public IEnumerator SectionsSequence()
    {
        for(int i = 0; i < subsectionDelays.Length; i++)
        {
            yield return new WaitForSeconds(subsectionDelays[i]);
        }
    }

    public IEnumerator TextSequence()
    {
        for (int i = 0; i < dialogueDelays.Length; i++)
        {
            yield return new WaitForSeconds(dialogueDelays[i]);
        }
    }

    
}
