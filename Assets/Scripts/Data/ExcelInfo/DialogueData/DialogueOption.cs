using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueOption
{
    [TextArea] public string text;
    public int targetId;

    public bool isTakeTask;

    //public event Action<bool> OnSelectEvent;

    //public void OptionSelect()
    //{
    //    OnSelectEvent?.Invoke(isTakeTask);
    //}
}
