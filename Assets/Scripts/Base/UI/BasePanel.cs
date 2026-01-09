using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class BasePanel : MonoBehaviour
{
    // 控件管理字典，字典键名：控件依附对象名，字典键值：依附对象挂载的所有控件，通过UIBehaviour控件基类存储
    private Dictionary<string, List<UIBehaviour>> uiDic = new Dictionary<string, List<UIBehaviour>>();

    /// <summary>
    /// Awake函数，将当前面板下所有类型控件添加到字典中
    /// </summary>
    protected virtual void Awake()
    {
        AddControl<Image>();
        AddControl<Text>();
        AddControl<RawImage>();
        AddControl<Button>();
        AddControl<Toggle>();
        AddControl<InputField>();
        AddControl<Slider>();
        AddControl<Scrollbar>();
        AddControl<ScrollRect>();
        AddControl<Dropdown>();
    }

    /// <summary>
    /// 控件添加函数
    /// </summary>
    /// <typeparam name="T">要添加的控件类型</typeparam>
    private void AddControl<T>() where T : UIBehaviour
    {
        T[] controls = GetComponentsInChildren<T>();
        foreach (T control in controls)
        {
            string name = control.name;
            if (uiDic.ContainsKey(name))
                uiDic[name].Add(control);
            else
                uiDic.Add(name, new List<UIBehaviour>() { control });

            if (control is Button)
            {
                (control as Button).onClick.AddListener(() =>
                {
                    ButtonOnClicked(name);
                });
            }

            else if (control is InputField)
            {
                (control as InputField).onValueChanged.AddListener((inputStr) =>
                {
                    InputFieldOnValueChanged(inputStr, name);
                });

                (control as InputField).onEndEdit.AddListener((inputStr) =>
                {
                    InputFieldOnEndEdit(inputStr, name);
                });
            }

            else if (control is Toggle)
            {
                (control as Toggle).onValueChanged.AddListener((value) =>
                {
                    ToggleOnValueChanged(value, name);
                });
            }

            else if (control is Slider)
            {
                (control as Slider).onValueChanged.AddListener((value) =>
                {
                    SliderOnValueChanged(value, name);
                });
            }

            else if (control is Scrollbar)
            {
                (control as Scrollbar).onValueChanged.AddListener((value) =>
                {
                    SBOnValueChanged(value, name);
                });
            }

            else if (control is ScrollRect)
            {
                (control as ScrollRect).onValueChanged.AddListener((vec) =>
                {
                    SROnValueChanged(vec, name);
                });
            }

            else if (control is Dropdown)
            {
                (control as Dropdown).onValueChanged.AddListener((index) =>
                {
                    DDOnValueChanged(index, name);
                });
            }
        }
    }

    /// <summary>
    /// 从控件字典中获取指定对象所挂在的指定类型控件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <returns></returns>
    public T GetControl<T>(string name) where T : UIBehaviour
    {
        if (uiDic.ContainsKey(name))
        {
            foreach (UIBehaviour control in uiDic[name])
            {
                if (control is T)
                    return control as T;
            }
        }

        return null;
    }

    public virtual void HideMe() { }

    public virtual void ShowMe() { }

    protected virtual void ButtonOnClicked(string btnName) { }

    protected virtual void InputFieldOnValueChanged(string inputStr, string inputName) { }
              
    protected virtual void InputFieldOnEndEdit(string inputStr, string inputName) { }
              
    protected virtual void ToggleOnValueChanged(bool value, string togName) { }
              
    protected virtual void SliderOnValueChanged(float value, string sliderName) { }
              
    protected virtual void SBOnValueChanged(float value, string SBName) { }
              
    protected virtual void SROnValueChanged(Vector2 vec, string SRName) { }
              
    protected  void DDOnValueChanged(int index, string DDName) { }
}
