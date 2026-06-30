using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem;

public class TutorialScript : MonoBehaviour
{
    public TextMeshProUGUI textComponent;
    public GameObject eventPanel;
    public GameObject dispatchButton;
    public string[] lines;
    public float textSpeed;

    private int index;

    void Start()
    {
        if (StateManager.Instance.currentState != StateManager.GameplayState.Tutorial)
        {
            gameObject.SetActive(false);
            dispatchButton.SetActive(true);
            eventPanel.SetActive(true);
        }
        else
        {
            textComponent.text = string.Empty;
            StartDialogue();
        }
    }


    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (textComponent.text == lines[index])
            {
                NextLine();
            }
            else
            {
                StopAllCoroutines();
                textComponent.text = lines[index];
            }
        }
    }

    public void StartDialogue()
    {
        index = 0;
        StartCoroutine(TypeLine());
    }

    // Coroutine that types out the text based on textSpeed float
    IEnumerator TypeLine()
    {
        foreach (char c in lines[index].ToCharArray())
        {
            textComponent.text += c;
            yield return new WaitForSeconds(textSpeed);
        }
    }

    public void NextLine()
    {
        if (index < lines.Length - 1)
        {
            index++;
            textComponent.text = string.Empty;
            StartCoroutine(TypeLine());
        }
        else
        {
            if (StateManager.Instance != null) StateManager.Instance.EndTutorial();
            gameObject.SetActive(false);
            eventPanel.SetActive(true);
            dispatchButton.SetActive(true);
        }
    }
}
