using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Android.Gradle;
using UnityEngine;
using UnityEngine.InputSystem;


public class NewsManager : MonoBehaviour
{
    public TextMeshProUGUI textComponent;
    public float textSpeed;

    private bool isTyping;
    void Start()
    {
        textComponent.text = string.Empty;
        StartDialogue();
    }


    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (RunState.Active.eventOutcomes.Count >0 && !isTyping)
            {
                NextLine();
            }
            else
            {
                StateManager.Instance.UpdateScene();
            }
        }
    }

    public void StartDialogue()
    {

        StartCoroutine(TypeLine(RunState.Active.eventOutcomes.Dequeue()));
    }

    // Coroutine that types out the text based on textSpeed float
    IEnumerator TypeLine(string sentence)
    {
        isTyping = true;
        textComponent.text = "";
        foreach (char c in sentence.ToCharArray())
        {
            textComponent.text += c;
            yield return new WaitForSeconds(textSpeed);
        }
        isTyping = false;
    }
    public void NextLine()
    {
        if (RunState.Active.eventOutcomes.Count > 0)
        {
            StartCoroutine(TypeLine(RunState.Active.eventOutcomes.Dequeue()));
        }
    }
}