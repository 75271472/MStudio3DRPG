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
}

public class MouseCursor : MonoBehaviour
{
    [SerializeField] private List<CursorInfo> textureList = new List<CursorInfo>();
    [SerializeField] private Vector2 offsetVect = new Vector2(16, 16);

    public void SwitchCursor(EMouseCursor cursorType)
    {
        CursorInfo cursorInfo = textureList.Find(
            cursorInfo => cursorInfo.cursorType == cursorType);
        if (cursorInfo == null) return;

        Cursor.SetCursor(cursorInfo.texture, offsetVect, CursorMode.Auto);
    }
}
