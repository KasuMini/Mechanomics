using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class LoadText : MonoBehaviour
{
    [Header("Keywords")]
    [SerializeField] TextAsset list1;
    [SerializeField] TextAsset list2;
    [SerializeField] TextAsset mechNameList;
    [SerializeField] TextAsset pilotNameList;

    public string[] keyword1;
    public string[] keyword2;
    public string[] mechNames;
    public string[] pilotNames;

    void OnValidate()
    {
        keyword1 = list1 ? list1.text.Split(',') : null;
        keyword2 = list2 ? list2.text.Split(',') : null;
        mechNames = mechNameList ? mechNameList.text.Split(",") : null;
        pilotNames = pilotNameList ? pilotNameList.text.Split(",") : null;
    }
}
