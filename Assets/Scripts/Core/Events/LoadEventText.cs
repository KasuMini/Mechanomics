using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class LoadEventText : MonoBehaviour
{
    [Header("Keywords")]
    [SerializeField] TextAsset list1;
    [SerializeField] TextAsset list2;
    public string[] keyword1;
    public string[] keyword2;

    void OnValidate()
    {
        keyword1 = list1 ? list1.text.Split(',') : null;
        keyword2 = list2 ? list2.text.Split(',') : null;
    }
}
