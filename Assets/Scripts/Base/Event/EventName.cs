using System.Dynamic;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.CompilerServices;
// 类名_注册事件函数名
public enum Events
{
    LoadSceneManager_LoadSceneAnsyc,

    Snake_SnakePartRemove,
    Snake_CreateDropItem,
    Snake_UpdatePosition,
    Snake_SnakePartInjured,

    SnakeUI_UpdatePosition,
    SnakeUI_UpdateSnakePartUI,
    SnakeUI_InitSnakePartUI,
    SnakeUI_SnakePartUIRemove,

    SnakeSO_SnakePartInjured,



    TurretAttack_BulletFire,
    Bullet_BulletBounce,
    Bullet_BulletHit,
    TurretManager_BulletNumAdd,
    TurretManager_TurretNumberChange,
    TurretManager_MoneyAdd,
    TurretControl_TurretDestroyed,

    MusicManager_SnakeHitted,
    MusicManager_SnakeDead,
    TurretManager_TurretNumberAdd,
    TurretManager_TurretNumberReduce,

    GameStateManager_GameStart,
    GameStateManager_GamePause,
    GameStateManager_GameOver,
    
}