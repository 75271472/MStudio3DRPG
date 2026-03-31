using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ABUpdatePanel : BasePanel
{
    [SerializeField] private Text tipTxt;
    [SerializeField] private Image progressImg;
    [SerializeField] private Button sureBtn;
    [SerializeField] private Button cancelBtn;
    [SerializeField] private Transform btnGroup;
    [SerializeField] private Transform progressGroup;
    [SerializeField] private Transform panelBk;

    public UnityAction<bool> OnUpdateSelectEvent;
    public UnityAction<bool> OnRetrySelectEvent;
    public UnityAction OnCompletedEvent;

    public override void ShowMe()
    {
        gameObject.SetActive(true);
        SetActiveBtnGroup(false);
        progressGroup.gameObject.SetActive(false);

        ExtensionTool.UpdateUI(panelBk);
    }

    public override void HideMe() 
    {
        gameObject.SetActive(false);
        SetActiveBtnGroup(false);
        progressGroup.gameObject.SetActive(false);
    }

    public void SetTipTxt(string tipStr)
    {
        tipTxt.text = tipStr;

        ExtensionTool.UpdateUI(panelBk);
    }

    public void ShowBtnGroupForSelectUpdate()
    {
        progressGroup.gameObject.SetActive(false);
        SetActiveBtnGroup(true);

        ExtensionTool.UpdateUI(panelBk);

        sureBtn.onClick.RemoveAllListeners();
        cancelBtn.onClick.RemoveAllListeners();

        sureBtn.onClick.AddListener(() => {
            OnUpdateSelectEvent?.Invoke(true);
            OnUpdateSelectEvent = null;
        });
        cancelBtn.onClick.AddListener(() => {
            OnUpdateSelectEvent?.Invoke(false);
            OnUpdateSelectEvent = null;
        });
    }

    public void ShowBtnGroupForRetry()
    {
        progressGroup.gameObject.SetActive(false);
        SetActiveBtnGroup(true);

        ExtensionTool.UpdateUI(panelBk);

        sureBtn.onClick.RemoveAllListeners();
        cancelBtn.onClick.RemoveAllListeners();

        sureBtn.onClick.AddListener(() => {
            OnRetrySelectEvent?.Invoke(true);
            OnRetrySelectEvent = null;
        });
        cancelBtn.onClick.AddListener(() => {
            OnRetrySelectEvent?.Invoke(false);
            OnRetrySelectEvent = null;
        });
    }

    public void ShowProgress()
    {
        SetActiveBtnGroup(false);
        progressGroup.gameObject.SetActive(true);

        ExtensionTool.UpdateUI(panelBk);

        UpdateProgress(0);
    }

    public void UpdateProgress(float progress)
    {
        progressImg.fillAmount = progress;
    }

    public void ShowBtnGroupForComplete()
    {
        progressGroup.gameObject.SetActive(false);
        SetActiveBtnGroup(true);

        cancelBtn.gameObject.SetActive(false);

        ExtensionTool.UpdateUI(panelBk);

        sureBtn.onClick.RemoveAllListeners();
        sureBtn.onClick.AddListener(() =>
        {
            OnCompletedEvent?.Invoke();
            OnCompletedEvent = null;
        });
    }

    private void SetActiveBtnGroup(bool isActive)
    {
        sureBtn.gameObject.SetActive(isActive);
        cancelBtn.gameObject.SetActive(isActive);
        btnGroup.gameObject.SetActive(isActive);
    }
}
