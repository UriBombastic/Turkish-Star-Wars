using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMaster : MonoBehaviour
{
    public enum Language
    {
        EN,
        SP,
        TUR,
        FUK
    }

    public static Language language = Language.EN;
}
