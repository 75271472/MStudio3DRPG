using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CharacterStateData
{
    public int health, maxHealth;
    public int level, maxLevel;
    public int exp, maxExp;
    public float levelBuff;
    public int attack;
    public float criticalRate;

    public CharacterStateData(int health, int maxHealth, int level, int maxLevel, 
        int exp, int maxExp, float levelBuff, int attack, float criticalRate)
    {
        this.health = health;
        this.maxHealth = maxHealth;
        this.level = level;
        this.maxLevel = maxLevel;
        this.exp = exp;
        this.maxExp = maxExp;
        this.levelBuff = levelBuff;
        this.attack = attack;
        this.criticalRate = criticalRate;
    }
}

public class PlayerStatePanel : BaseInventoryPanel
{
    [SerializeField] private StateItemUI weaponItemUI;
    [SerializeField] private StateItemUI shieldItemUI;

    [SerializeField] private StateTxtUI healthStateTxtUI;
    [SerializeField] private StateTxtUI levelStateTxtUI;
    [SerializeField] private StateTxtUI expStateTxtUI;
    [SerializeField] private StateTxtUI levelBuffStateTxtUI;
    [SerializeField] private StateTxtUI attackStateTxtUI;
    [SerializeField] private StateTxtUI criticalRateStateTxtUI;

    private InventoryBK inventoryBK;

    //public event Action<EEquippableItem, IndexInfo> OnEquipEvent, OnUnloadEvent;
    public event Func<CharacterStateData> OnGetCharacterStateDataEvent;
    public override EInventoryPanel PanelType => EInventoryPanel.PlayerState;

    public override void ShowMe()
    {
        if (inventoryBK == null)
        {
            inventoryBK = GetComponentInChildren<InventoryBK>();
            inventoryBK.OnPointerClickEvent += OnResetSelectHandler;
        }

        gameObject.SetActive(true);
        ResetSelection();

        UpdateStateUI(OnGetCharacterStateDataEvent?.Invoke());
    }

    public override void HideMe()
    {
        UIManager.Instance.HidePanel<InputActionPanel>();
        gameObject.SetActive(false);
        ResetDraggedItem();
    }

    public void PlayerStatePanelInit()
    {
        BaseInventoryInit();

        itemUIList.Add(weaponItemUI);
        itemUIList.Add(shieldItemUI);

        foreach (StateItemUI itemUI in itemUIList)
        {
            itemUI.OnLeftClickedEvent += OnItemSelectedHandler;
            itemUI.OnRightClickedEvent += OnItemAcionShowHandler;
            itemUI.OnItemBeginDragEvent += OnItemBeginDragHandler;
            itemUI.OnItemEndDragEvent += OnItemEndDragHandler;
            itemUI.OnItemDroppedEvent += OnItemSwapHandler;

            //itemUI.OnEquipEvent += OnEquipHandler;
            //itemUI.OnUnloadEvent += OnUnloadHandler;

            itemUI.ItemUIInit();
        }
    }

    public void UpdateStateUI(CharacterStateData playerStateData)
    {
        if (playerStateData == null) return;

        healthStateTxtUI.UpdateTxt(playerStateData.health, playerStateData.maxHealth);
        levelStateTxtUI.UpdateTxt(playerStateData.level, playerStateData.maxLevel);
        expStateTxtUI.UpdateTxt(playerStateData.exp, playerStateData.maxExp);
        levelBuffStateTxtUI.UpdateTxt(playerStateData.levelBuff);
        attackStateTxtUI.UpdateTxt(playerStateData.attack);
        criticalRateStateTxtUI.UpdateTxt(playerStateData.criticalRate);
    }

    public int GetEquippableItemIndex(EEquippableItem equippableItemType)
    {
        for (int i = 0; i < itemUIList.Count; i++)
        {
            if ((itemUIList[i] as StateItemUI).EquippableItemType == 
                equippableItemType)
            {
                return i;
            }
        }

        return -1;
    }

    public EEquippableItem GetEquippableItemType(int index)
    {
        return (itemUIList[index] as StateItemUI).EquippableItemType;
    }

    protected override void SetDescriptionPosition(int index)
    {
        Transform itemUITrans = itemUIList[index].transform;
        Vector3 targetPos = itemUITrans.position;
        //targetPos.x += (itemUITrans as RectTransform).rect.width / 2 +
        //    (itemDescription.transform as RectTransform).rect.width / 2;
        targetPos.x += (itemUITrans as RectTransform).rect.width / 2;
        itemDescription.SetPosition(targetPos, ItemDescription.EPos.Right);
    }

    protected override void SetItemActionPosition(InputActionPanel panel, int index)
    {
        // 这里不知道为什么要等两帧，但等两帧才能将面板显示在正确位置
        //yield return null;
        //yield return null;

        Transform itemUITrans = itemUIList[index].transform;
        Vector3 targetPos = itemUITrans.position;
        //targetPos.x += (itemUITrans as RectTransform).rect.width / 2 +
        //    (panel.inputActionBG as RectTransform).rect.width / 2;
        targetPos.x += (itemUITrans as RectTransform).rect.width / 2;
        panel.SetPosition(targetPos, InputActionPanel.EPos.Right);
    }
}
