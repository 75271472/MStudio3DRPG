using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TestText
{
    [TextArea] public string text;
}

public class Test : MonoBehaviour
{
    public DialoguePanel panel;
    public List<TestText> stringList = new List<TestText>();
    public int index = 0;

    private void Start()
    {
        panel.DialoguePanelInit();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && index < stringList.Count)
        {
            panel.SetDialogueContent(stringList[index].text, false); 
            index++;
            index %= stringList.Count;

            //isChanged = true;
        }
    }
}
