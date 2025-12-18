using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialoguePanel : BasePanel
{
    [SerializeField] private Button quitBtn;
    private DialogueContentUI dialogueContentUI;
    private DialogueOptionUI dialogueOptionUI;

    public event Action OnEndDialogueEvent;
    public event Action OnUpdateDialogueContentEvent;
    public event Action<int> OnOptionSelectEvent;

    public override void ShowMe()
    {
        base.ShowMe();

        gameObject.SetActive(true);
    }

    public override void HideMe()
    {
        base.HideMe();

        gameObject.SetActive(false);
    }

    public void DialoguePanelInit()
    {
        quitBtn.onClick.AddListener(OnEndDialogue);

        dialogueContentUI = GetComponentInChildren<DialogueContentUI>();
        dialogueOptionUI = GetComponentInChildren<DialogueOptionUI>();

        dialogueContentUI.ResetEvent();
        dialogueOptionUI.ResetEvent();

        dialogueContentUI.OnUpdateContentEvent += OnUpdateDialogueContentHandler;
        dialogueOptionUI.OnOptionSelectEvent += OnOptionSelectHandler;

        dialogueContentUI.DialogueContentUIInit();
    }

    public void SetProfile(Texture profileTexture, string name)
    {
        dialogueContentUI.SetProfileTexture(profileTexture);
        dialogueContentUI.SetNameTxt(name);
    }

    public void SetDialogueContent(string dialogueContent, bool hasNext)
    {
        dialogueContentUI.SetContent(dialogueContent, hasNext);
    }

    public void ResetDialogueOption()
    {
        dialogueOptionUI.ResteOptionList();
    }

    public void SetDialogueOption(List<string> optionStringList)
    {
        dialogueOptionUI.UpdateOptionList(optionStringList);
    }

    private void OnUpdateDialogueContentHandler()
    {
        OnUpdateDialogueContentEvent?.Invoke();
    }

    private void OnOptionSelectHandler(int index)
    {
        //print("DialoguePanel: OnOptionSelectHandler");
        OnOptionSelectEvent?.Invoke(index);
    }

    private void OnEndDialogue()
    {
        OnEndDialogueEvent?.Invoke();
    }
}
