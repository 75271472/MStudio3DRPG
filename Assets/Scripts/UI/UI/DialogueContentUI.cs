using DG.Tweening;
using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class DialogueContentUI : MonoBehaviour
{
    [SerializeField] private RawImage profileRawImg;
    [SerializeField] private Text nameTxt;
    [SerializeField] private Text dialogueTxt;
    [SerializeField] private Text ghostTxt;
    [SerializeField] private Button nextBtn;
    [SerializeField] private RectTransform bk;

    public event Action OnUpdateContentEvent;

    public void DialogueContentUIInit()
    {
        nextBtn.onClick.AddListener(OnUpdateContent);

        ResetContent();
    }

    public void ResetEvent()
    {
        nextBtn.onClick.RemoveAllListeners();
        OnUpdateContentEvent = null;
    }

    public void SetProfileTexture(Texture profileTexture)
    {
        if (profileTexture == null)
        {
            this.profileRawImg.enabled = false;
            return;
        }

        this.profileRawImg.enabled = true;
        this.profileRawImg.texture = profileTexture;
    }

    public void SetNameTxt(string name)
    {
        this.nameTxt.text = name;
    }

    public void ResetContent()
    {
        dialogueTxt.text = string.Empty;
        nextBtn.gameObject.SetActive(false);
    }

    public void SetContent(string dialogueContent, bool hasNext)
    {
        ResetContent();

        // 1. 先把完整内容给 Ghost，让它瞬间把 UI 撑到最终大小
        ghostTxt.text = dialogueContent;

        // 2. 强制刷新一次所有层级，确保 UI 定格在最终大小
        // 因为只执行一次，所以多刷新几层也没性能问题
        // DialogueUp
        ExtensionTool.UpdateUI(ghostTxt.transform.parent); 
        // DialogueBK
        ExtensionTool.UpdateUI(bk); 
        // Content
        if (transform.parent != null) 
            ExtensionTool.UpdateUI(transform.parent); 

        // 3. 在固定好的框里播放打字动画 (不需要 OnUpdate 了)
        dialogueTxt.DOText(dialogueContent, 1f).SetEase(Ease.Linear);

        nextBtn.gameObject.SetActive(hasNext);
    }

    private void OnUpdateContent()
    {
        OnUpdateContentEvent?.Invoke();
    }
}
