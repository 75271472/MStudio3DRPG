using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class IntInfo
{
    public int value;
}
public class CharacterTransInfo
{
    public bool IsEmpty => positionArray == null || rotationArray == null;
    public string SceneName;

    // 将要序列化的字段保持共有访问修饰符
    public float[] positionArray { get; private set; }
    public float[] rotationArray { get; private set; }

    // 将Vector3成员变量变为成员方法，避免被Json序列化，
    // 因为LitJson序列化Vector3类型变量会报错
    public Vector3 GetPosition() => new Vector3(
    positionArray[0], positionArray[1], positionArray[2]);
    public Quaternion GetRotation() => new Quaternion(
        rotationArray[0], rotationArray[1], rotationArray[2], rotationArray[3]);

    public void UpdateCharacterTransInfo(string SceneName,
        Vector3 position, Quaternion rotation)
    {
        this.SceneName = SceneName;
        this.positionArray = new float[3] {
            position.x, position.y, position.z};
        this.rotationArray = new float[4] {
            rotation.x, rotation.y, rotation.z, rotation.w};
    }
}

public class InventoryInfo
{
    public int size;
    public List<InventoryItemInfo> inventoryItemList;
}

//public class QuestIdInfo
//{
//    public List<CurrentQuest> 
//}

public class DataManager : BaseManager<DataManager>
{
    #region const string
    public const string SCENEROOTPATH = "Scenes/";
    // 开始场景常量名
    public const string STARTSCENE = "StartScene";
    // 第一加载场景常量名
    public const string FIRSTSCENE = "SceneOne";
    
    #region resources
    // UI文件夹根路径
    public const string UIROOTPATH = "Prefabs/UI/";
    public const string ITEMUIROOTPATH = UIROOTPATH + "ItemUI/";

    // Panel文件夹根路径
    public const string PANELROOTPATH = UIROOTPATH + "Panel/";
    public const string OVERLAYCANVAS = UIROOTPATH + "OverlayCanvas";
    public const string WORLDCANVAS = UIROOTPATH + "WorldCanvas";
    public const string EVENTSYSTEM = UIROOTPATH + "EventSystem";
    
    // 游戏对象预设体路径
    public const string ROCKBREAKPARTICAL = "Prefabs/Particle/RockBreakParticle";
    public const string ROCK = "Prefabs/Weapon/Rock";
    public const string SHIELD = "Prefabs/WordItem/Shield";
    public const string YELLOWPORTAL = "Prefabs/Portal/YellowPortal";

    // 普通UI路径
    public const string INPUTACTIONING = UIROOTPATH + "InputActionUI";
    public const string HEALTHBAR = UIROOTPATH + "HealthBar";
    public const string OPTIONBUTTONUI = UIROOTPATH + "OptionBtnUI";
    public const string QUESTBUTTON = UIROOTPATH + "QuestBtn";
    public const string PROGRESSUI = UIROOTPATH + "ProgressUI";
    public const string INFOTEXTUI = UIROOTPATH + "InfoTxtUI";

    public const string INVENTORYITEMUI = ITEMUIROOTPATH + "InventoryItemUI";
    public const string REWARDITEMUI = ITEMUIROOTPATH + "RewardItemUI";

    // ModifierSO文件路径
    public const string MODIFIER = "GameData/Modifier/";
    public const string QUESTFINISHER = "GameData/QuestFinisher/";
    #endregion

    #region persistentDataPath
    public const string PLAYERARCHIVE = "PlayerArchive/";
    public const string ARCHIVEINDEX = PLAYERARCHIVE + "ArchiveIndex";
    public const string PLAYERARCHIVEONE = PLAYERARCHIVE + "One/";
    public const string PLAYERARCHIVETWO = PLAYERARCHIVE + "Two/";
    public const string PLAYERARCHIVETHREE = PLAYERARCHIVE + "Three/";
    //public const string PLAYERTRANSFORM = "PlayerTransform";
    public const string INVENTORYINFO = "InventoryInfo";
    public const string PLAYERINFO = "PlayerInfo";
    public const string QUESTRECORD = "QuestRecord";
    public const string CONDITIONINFO = "ConditionInfo";
    #endregion

    #region streamingAssets
    //public const string DEFAULTPLAYERDEFAULTINFO = "PlayerStateInfo";
    public const string PLAYERIDINFO = "PlayerIdInfo";
    public const string PLAYERSTATEINFO = "PlayerStateInfo";
    public const string PLAYERMOVEINFO = "PlayerMoveInfo";

    public const string MONSTERIDINFO = "MonsterIdInfo";
    public const string MONSTERSTATEINFO = "MonsterStateInfo";
    public const string MONSTERMOVEINFO = "MonsterMoveInfo";

    public const string NPCIDINFO = "NPCIdInfo";
    public const string DIALOGUEPIECEINFO = "DialoguePieceInfo";
    public const string QUESTDIALOGUEBINDINGINFO = "QuestDialogueBindingInfo";
    public const string CONDITIONDIALOGUEPIECEINFO = "ConditionDialoguePieceInfo";
    public const string CONDITIONDIALOGUEBINDING = "ConditionDialogueBinding";

    public const string ATTACKINFO = "AttackInfo";
    public const string EDIBLEITEMINFO = "EdibleItemInfo";
    public const string EQUIPPABLEITEMINFO = "EquippableItemInfo";
    public const string QUESTINFO = "QuestInfo";
    public const string DEFAULTINVENTORYITEMINFO = "DefaultInventoryItemInfo";
    #endregion
    
    #endregion
    public int ArchiveIndex { get; private set; }
    public List<AttackInfo> AttackInfoList { get; private set; }

    public PlayerIdInfo PlayerIdInfo { get; private set; }
    public PlayerInfo PlayerInfo { get; private set; }
    public InventoryInfo InventoryInfo { get; private set; }
    public List<PlayerStateInfo> PlayerStateInfoList { get; private set; }
    public List<PlayerMoveInfo> PlayerMoveInfoList { get; private set; }

    public List<MonsterIdInfo> MonsterIdInfoList { get; private set; }
    public List<MonsterInfo> MonsterInfoList { get; private set; }
    public List<MonsterStateInfo> MonsterStateInfoList { get; private set; }
    public List<MonsterMoveInfo> MonsterMoveInfoList { get; private set; }

    public List<NPCIdInfo> NPCIdInfoList { get; private set; }
    public List<NPCInfo> NPCInfoList { get; private set; }

    public Dictionary<int, List<QuestDialogueBinding>> BindingMap { get; private set; }
    public Dictionary<int, DialoguePiece> DialoguePieceMap { get; private set; }

    public Dictionary<int, List<ConditionDialogueBinding>> ConditionBindingMap { get; private set; }
    public Dictionary<int, ConditionDialoguePiece> ConditionDialoguePieceMap { get; private set; }

    public List<EdibleItemInfo> EdibleItemInfoList { get; private set; }
    public List<EquippableItemInfo> EquippableItemInfoList { get; private set; }
    //public List<InventoryItemInfo> InventoryItemInfoList { get; private set; }

    public List<QuestInfo> QuestInfoList { get; private set; }
    public List<QuestRecord> QuestRecordList { get; private set; }
    public List<ConditionInfo> ConditionInfoList { get; private set; }

    public DataManager()
    {
        LoadAttackInfoList();
        LoadEdibleItemInfoList();
        LoadEquippableItemInfoList();
        LoadQuestInfoList();

        LoadPlayerStateInfoList();
        LoadPlayerMoveInfoList();

        LoadMonsterStateInfoList();
        LoadMonsterMoveInfoList();
        
        LoadMonsterIdInfoList();
        LoadMonsterInfoList();

        LoadNPCIdInfoList();
        LoadNPCInfoList();

        LoadBindingMap();
        LoadDialoguePieceMap();

        LoadConditionBindingMap();
        LoadConditionDialoguePieceMap();

        if (MonoManager.Instance.IsDebug)
        {
            LoadPlayerData(1);
        }
    }

    public void LoadPlayerData(int archiveIndex = 0)
    {
        if (archiveIndex >= 1 && archiveIndex <= 3)
            ArchiveIndex = archiveIndex;

        Debug.Log($"Log Player Archive: {ArchiveIndex}");

        LoadPlayerInfo();
        LoadInventoryItemInfoList();
        LoadQuestRecordList();
        LoadConditionInfoList();
    }

    // 如果不填入值，就是用DataManager中的ArchiveIndex加载存档
    private void LoadPlayerInfo()
    {
        // 先在PersistentData中查找已经存在的存档
        // PersistentData中的数据是单个的
        PlayerInfo = GetPlayerInfoByIndex(ArchiveIndex);
        if (PlayerInfo != null)
        {
            //Debug.Log($"Load Archive {ArchiveIndex}");
            // 如果PersistentData中有先前的存档，就更新ArchiveIndex
            SaveCurrentArchiveIndex();
            return;
        }

        // 如果没有就在StreamingAsstes中查找默认存档
        // StreamingAsstes中的数据是以集合形式呈现的

        LoadPlayerIdInfo();

        PlayerMoveInfo moveInfo = PlayerMoveInfoList[PlayerIdInfo.moveInfoId];
        PlayerStateInfo stateInfo = PlayerStateInfoList[PlayerIdInfo.stateInfoId];
        CharacterTransInfo transInfo = new CharacterTransInfo();
        AttackInfo attackInfo = AttackInfoList[PlayerIdInfo.attackInfoId];
        InventoryItemInfo defaultWeaponInfo = new InventoryItemInfo(
            EquippableItemInfoList[PlayerIdInfo.defaultWeaponInfoId], 1);

        PlayerInfo = new PlayerInfo(PlayerIdInfo.name, moveInfo, stateInfo, transInfo, 
            attackInfo, defaultWeaponInfo, PlayerIdInfo.characterDialogueId);
    }

    private void LoadInventoryItemInfoList()
    {
        // 先从PersistentData中加载资源
        // PersistentData中记录有物品的格子，和背包大小size
        InventoryInfo = GetInventoryItemInfoListByIndex(ArchiveIndex);
        if (InventoryInfo != null) return;

        InventoryInfo = new InventoryInfo();
        // 再从StreamingAssets中加载
        // StreamingAssets默认资源中，记录了所有格子，包括有物品的和没有物品的
        InventoryInfo.inventoryItemList = JsonManager.Instance.
            LoadDataFromStreamingAssets<List<InventoryItemInfo>>(
            DEFAULTINVENTORYITEMINFO);

        InventoryInfo.size = InventoryInfo.inventoryItemList.Count;
    } 

    private void LoadQuestRecordList()
    {
        QuestRecordList = GetQuestRecordListByIndex(ArchiveIndex);
        if (QuestRecordList != null) return;

        QuestRecordList = new List<QuestRecord>();
        foreach (var questInfo in QuestInfoList)
        {
            QuestRecordList.Add(new QuestRecord(questInfo));
        }
    }

    private void LoadConditionInfoList()
    {
        ConditionInfoList = GetConditionInfoListByIndex(ArchiveIndex);
        if (ConditionInfoList != null) return;

        ConditionInfoList = JsonManager.Instance.
            LoadDataFromStreamingAssets<List<ConditionInfo>>(CONDITIONINFO);
    }

    /// <summary>
    /// 加载上一次游戏存档下标同时检查该存档是否完整
    /// </summary>
    /// <returns></returns>
    public bool LoadArchiveIndex()
    {
        if (JsonManager.Instance.LoadDataFromPersistentData<IntInfo>(
            ARCHIVEINDEX, out var archiveIndex))
        {
            ArchiveIndex = archiveIndex.value;
            return CheckArchiveExist(ArchiveIndex);
        }

        return false;
    }

    private void LoadPlayerIdInfo()
    {
        PlayerIdInfo = JsonManager.Instance.
            LoadDataFromStreamingAssets<List<PlayerIdInfo>>(PLAYERIDINFO)[0];
    }

    private void LoadPlayerStateInfoList()
    {
        PlayerStateInfoList = JsonManager.Instance.
            LoadDataFromStreamingAssets<List<PlayerStateInfo>>(
            PLAYERSTATEINFO);
    }

    private void LoadPlayerMoveInfoList()
    {
        PlayerMoveInfoList = JsonManager.Instance.
            LoadDataFromStreamingAssets<List<PlayerMoveInfo>>(
            PLAYERMOVEINFO);
    }

    private void LoadMonsterIdInfoList()
    {
        MonsterIdInfoList = JsonManager.Instance.
            LoadDataFromStreamingAssets<List<MonsterIdInfo>>(
            MONSTERIDINFO);
    }

    private void LoadMonsterInfoList()
    {
        if (MonsterInfoList == null)
        {
            MonsterInfoList = new List<MonsterInfo>();
        }

        MonsterInfoList.Clear();

        foreach (var idInfo in MonsterIdInfoList)
        {
            MonsterMoveInfo moveInfo = MonsterMoveInfoList[idInfo.moveInfoId];
            MonsterStateInfo stateInfo = MonsterStateInfoList[idInfo.stateInfoId];
            AttackInfo attackInfo = AttackInfoList[idInfo.attackInfoId];

            MonsterInfoList.Add(new MonsterInfo(idInfo.name,
                moveInfo, stateInfo, attackInfo));
        }
    }

    private void LoadMonsterStateInfoList()
    {
        MonsterStateInfoList = JsonManager.Instance.
            LoadDataFromStreamingAssets<List<MonsterStateInfo>>(
            MONSTERSTATEINFO);
    }

    private void LoadMonsterMoveInfoList()
    {
        MonsterMoveInfoList = JsonManager.Instance.
            LoadDataFromStreamingAssets<List<MonsterMoveInfo>>(
            MONSTERMOVEINFO);
    }

    private void LoadNPCIdInfoList()
    {
        NPCIdInfoList = JsonManager.Instance.
            LoadDataFromStreamingAssets<List<NPCIdInfo>>(NPCIDINFO);
    }

    private void LoadNPCInfoList()
    {
        if (NPCInfoList == null)
        {
            NPCInfoList = new List<NPCInfo>();
        }

        NPCInfoList.Clear();

        foreach (var idInfo in NPCIdInfoList)
        {
            NPCInfoList.Add(new NPCInfo(idInfo.name, idInfo.characterDialogueId));
        }
    }

    private void LoadBindingMap()
    {
        BindingMap = new Dictionary<int, List<QuestDialogueBinding>>();

        List<QuestDialogueBinding> BindingList = JsonManager.Instance.
            LoadDataFromStreamingAssets<List<QuestDialogueBinding>>(
            QUESTDIALOGUEBINDINGINFO);

        foreach (var binding in BindingList)
        {
            if (BindingMap.ContainsKey(binding.characterId))
            {
                BindingMap[binding.characterId].Add(binding);
            }
            else
            {
                List<QuestDialogueBinding> bindingList = 
                    new List<QuestDialogueBinding>();

                bindingList.Add(binding);
                BindingMap.Add(binding.characterId, bindingList);
            }
        }
    }

    public void LoadDialoguePieceMap()
    {
        DialoguePieceMap = new Dictionary<int, DialoguePiece>();

        List<DialoguePiece> pieceList = JsonManager.Instance.
            LoadDataFromStreamingAssets<List<DialoguePiece>>(DIALOGUEPIECEINFO);

        foreach (var piece in pieceList)
        {
            DialoguePieceMap.Add(piece.id, piece);
        }
    }

    public void LoadConditionBindingMap()
    {
        ConditionBindingMap = new Dictionary<int, List<ConditionDialogueBinding>>();

        List<ConditionDialogueBinding> ConditionBindingList = JsonManager.Instance.
            LoadDataFromStreamingAssets<List<ConditionDialogueBinding>>(
            CONDITIONDIALOGUEBINDING);

        foreach (var binding in ConditionBindingList)
        {
            if (ConditionBindingMap.ContainsKey(binding.characterId))
            {
                ConditionBindingMap[binding.characterId].Add(binding);
            }
            else
            {
                List<ConditionDialogueBinding> bindingList =
                    new List<ConditionDialogueBinding>();

                bindingList.Add(binding);
                ConditionBindingMap.Add(binding.characterId, bindingList);
            }
        }
    }

    public void LoadConditionDialoguePieceMap()
    {
        ConditionDialoguePieceMap = new Dictionary<int, ConditionDialoguePiece>();

        List<ConditionDialoguePiece> pieceList = JsonManager.Instance.
            LoadDataFromStreamingAssets<List<ConditionDialoguePiece>>(
            CONDITIONDIALOGUEPIECEINFO);

        foreach (var piece in pieceList)
        {
            ConditionDialoguePieceMap.Add(piece.id, piece);
        }
    }

    private void LoadAttackInfoList()
    {
        AttackInfoList = JsonManager.Instance.
            LoadDataFromStreamingAssets<List<AttackInfo>>(ATTACKINFO);
    }

    private void LoadEdibleItemInfoList()
    {
        EdibleItemInfoList = JsonManager.Instance.
            LoadDataFromStreamingAssets<List<EdibleItemInfo>>(EDIBLEITEMINFO);

        foreach (var itemInfo in EdibleItemInfoList)
        {
            itemInfo.img = ResourceManager.Instance.Load<Sprite>(itemInfo.imgPath);
        }
    }

    private void LoadEquippableItemInfoList()
    {
        EquippableItemInfoList = JsonManager.Instance.
            LoadDataFromStreamingAssets<List<EquippableItemInfo>>(EQUIPPABLEITEMINFO);

        foreach (var itemInfo in EquippableItemInfoList)
        {
            itemInfo.img = ResourceManager.Instance.Load<Sprite>(itemInfo.imgPath);
            if (itemInfo.attackInfoId != -1)
                itemInfo.weaponAttackInfo = AttackInfoList[itemInfo.attackInfoId];
        }
    }

    private void LoadQuestInfoList()
    {
        QuestInfoList = JsonManager.Instance.
            LoadDataFromStreamingAssets<List<QuestInfo>>(QUESTINFO);
    }

    public DialoguePiece GetDialoguePieceById(int pieceId)
    {
        if (DialoguePieceMap.TryGetValue(pieceId, out var piece))
        {
            return piece;
        }

        return null;
    }

    public ConditionDialoguePiece GetConditionDialoguePieceById(int pieceId)
    {
        if (ConditionDialoguePieceMap.TryGetValue(pieceId, out var piece))
        {
            return piece;
        }

        return null;
    }

    public ItemInfo GetItemInfo(int itemInfoType, int itemId)
    {
        if (itemId < 0) return null;

        return (EItemInfo)itemInfoType switch
        {
            EItemInfo.Editable => itemId >=
            EdibleItemInfoList.Count ? null : EdibleItemInfoList[itemId],
            EItemInfo.Equippable => itemId >=
            EquippableItemInfoList.Count ? null : EquippableItemInfoList[itemId],
            _ => null
        };
    }

    public void SavePlayerInfo(Action saveOKAction = null, 
        Action saveErrorAction = null)
    {
        try
        {
            string playerInfoPath = GetSavePath(ArchiveIndex, PLAYERINFO);

            JsonManager.Instance.SaveData(PlayerInfo, playerInfoPath);

            // 更新最新记录的存档下标
            SaveCurrentArchiveIndex();

            saveOKAction?.Invoke();
        }
        catch (Exception ex) 
        {
            Debug.Log(ex.Message);
            saveErrorAction?.Invoke();
        }
    }

    public void SaveCurrentArchiveIndex()
    {
        JsonManager.Instance.SaveData(new IntInfo() {
            value = ArchiveIndex,
        }, ARCHIVEINDEX);
    }

    // 更新InventoryItemInfoList
    // 用于数据保存到本地，或切换场景时数据保存到内存
    // （因为DataManager为不继承Mono的单例模式），因此切换场景时数据不变
    public void UpdateInventoryItemInfoList(int size, 
        Dictionary<int, InventoryItem> itemDict)
    {
        InventoryInfo.inventoryItemList = new List<InventoryItemInfo>();
        foreach (var value in itemDict)
        {
            InventoryInfo.inventoryItemList.Add(
                new InventoryItemInfo(value.Value, value.Key));
        }

        InventoryInfo.size = size;
    }

    public void SaveInventoryItemList(int size, Dictionary<int, InventoryItem> itemDict)
    {
        string itemListPath = GetSavePath(ArchiveIndex, INVENTORYINFO);

        UpdateInventoryItemInfoList(size, itemDict);

        JsonManager.Instance.SaveData(InventoryInfo, itemListPath);
    }

    public void UpdateQuestRecordList(Dictionary<int, Quest> activeQuestDict, 
        Dictionary<int, Quest> finishedQuestDict)
    {
        foreach (var questPair in activeQuestDict)
        {
            QuestRecordList[questPair.Key] = new QuestRecord(questPair.Value);
            //Debug.Log("UpdateQuest: " + QuestRecordList[questPair.Key].questState);
        }

        foreach (var questPair in finishedQuestDict)
        {
            QuestRecordList[questPair.Key] = new QuestRecord(questPair.Value);
            //Debug.Log("UpdateQuest：" + QuestRecordList[questPair.Key].questState);
        }
    }

    public void SaveQuestRecordList(Dictionary<int, Quest> activeQuestDict,
        Dictionary<int, Quest> finishedQuestDict)
    {
        string questRequireListPath = GetSavePath(ArchiveIndex, QUESTRECORD);

        UpdateQuestRecordList(activeQuestDict, finishedQuestDict);

        JsonManager.Instance.SaveData(QuestRecordList, questRequireListPath);
    }

    public void SaveConditionInfoList()
    {
        string conditionInfoListPath = GetSavePath(ArchiveIndex, CONDITIONINFO);

        JsonManager.Instance.SaveData(ConditionInfoList, conditionInfoListPath);
    }

    private string GetSavePath(int archiveIndex, string suffixStr = "")
    {
        return archiveIndex switch
        {
            1 => PLAYERARCHIVEONE + suffixStr,
            2 => PLAYERARCHIVETWO + suffixStr,
            3 => PLAYERARCHIVETHREE + suffixStr,
            _ => string.Empty
        };
    }

    // 删除存档
    public void DeletePlayerDataInPersistentData(int archiveIndex)
    {
        if (!CheckArchiveExist(archiveIndex)) return;

        string archiveConstStr = GetSavePath(archiveIndex);

        string archiveFilePath =
            $"{Application.persistentDataPath}/{archiveConstStr}";

        Debug.Log(Path.GetDirectoryName(archiveFilePath));
        Directory.Delete(Path.GetDirectoryName(archiveFilePath), true);
    }

    /// <summary>
    /// 从1到3查找第一个空存档，查找指定下标的存档是否为空
    /// 找到空存档则更新ArchiveIndex
    /// </summary>
    /// <param name="archiveIndex">要判断的存档下标</param>
    /// <returns></returns>
    public bool UpdateEmptyArchiveIndex(int archiveIndex = 0)
    {
        bool isEmpty = false;

        if (archiveIndex >= 1 && archiveIndex <= 3)
        {
            if (!CheckArchiveExist(archiveIndex))
            {
                ArchiveIndex = archiveIndex;
                isEmpty = true;
            }
        }
        else
        {
            if (!CheckArchiveExist(1))
            {
                ArchiveIndex = 1;
                isEmpty = true;
            }
            else if (!CheckArchiveExist(2))
            {
                ArchiveIndex = 2;
                isEmpty = true;
            }
            else if (!CheckArchiveExist(3))
            {
                ArchiveIndex = 3;
                isEmpty = true;
            }
        }

        if (isEmpty)
            LoadPlayerData(ArchiveIndex);
        else
            ArchiveIndex = 0;

        return isEmpty;
    }

    /// <summary>
    /// 获取下标存档的PlayerStateInfo数据信息
    /// </summary>
    /// <param name="archiveIndex"></param>
    /// <returns></returns>
    public PlayerInfo GetPlayerInfoByIndex(int archiveIndex)
    {
        if (!CheckArchiveExist(archiveIndex)) return null;

        string playerInfoPath = GetSavePath(archiveIndex, PLAYERINFO);

        if (JsonManager.Instance.LoadDataFromPersistentData<PlayerInfo>(
            playerInfoPath, out var playerInfo))
        {
            return playerInfo;
        }

        return null;
    }

    public InventoryInfo GetInventoryItemInfoListByIndex(int archiveIndex)
    {
        if (!CheckArchiveExist(archiveIndex)) return null;

        string inventoryInfoPath = GetSavePath(archiveIndex, INVENTORYINFO);

        if (JsonManager.Instance.LoadDataFromPersistentData<InventoryInfo>(
            inventoryInfoPath, out var inventoryInfo))
        {
            return inventoryInfo;
        }

        return null;
    }

    public List<QuestRecord> GetQuestRecordListByIndex(int archiveIndex)
    {
        if (!CheckArchiveExist(archiveIndex)) return null;

        string questRecordListPath = GetSavePath(archiveIndex, QUESTRECORD);

        if (JsonManager.Instance.LoadDataFromPersistentData<List<QuestRecord>>(
            questRecordListPath, out var questRecordList))
        {
            return questRecordList;
        }

        return null;
    }

    public List<ConditionInfo> GetConditionInfoListByIndex(int archiveIndex)
    {
        if (!CheckArchiveExist(archiveIndex)) return null;

        string conditionListPath = GetSavePath(archiveIndex, CONDITIONINFO);

        if (JsonManager.Instance.LoadDataFromPersistentData<List<ConditionInfo>>(
            conditionListPath, out var conditionInfoList))
        {
            return conditionInfoList;
        }

        return null;
    }

    /// <summary>
    /// 检查执行下标存档是否存在并且完整
    /// </summary>
    /// <param name="archiveIndex"></param>
    /// <returns></returns>
    public bool CheckArchiveExist(int archiveIndex)
    {
        return JsonManager.Instance.FindFromPersistentData(
            GetSavePath(archiveIndex, PLAYERINFO)) && 
            JsonManager.Instance.FindFromPersistentData(
            GetSavePath(archiveIndex, INVENTORYINFO));
    }
}
