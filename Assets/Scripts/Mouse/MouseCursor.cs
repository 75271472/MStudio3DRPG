using System;
using System.Collections.Generic;
using UnityEngine;

public enum EMouseCursor
{
    Point,
    Doorway,
    Attack,
    Target,
    Arrow,
}

[Serializable]
public class CursorInfo
{
    public EMouseCursor cursorType;
    public Texture2D texture;
    public Vector2 offsetVect;
}

public class MouseCursor : MonoBehaviour
{
    [SerializeField] private List<CursorInfo> textureList = new List<CursorInfo>();

    public void SwitchCursor(EMouseCursor cursorType)
    {
        //print(cursorType);
        CursorInfo cursorInfo = textureList.Find(
            cursorInfo => cursorInfo.cursorType == cursorType);
        if (cursorInfo == null) return;

        Cursor.SetCursor(cursorInfo.texture, cursorInfo.offsetVect, CursorMode.Auto);
    }
}
