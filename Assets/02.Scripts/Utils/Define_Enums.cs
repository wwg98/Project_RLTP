using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Define_Enums
{
    public enum ScenesKind
    {
        Title_Scene = 0,
        Lobby_Scene,
        Setting_Scene,
        Small_Map_Scene,
        Large_Map_Scene
    }

    public enum RSPKind 
    {
        Rock = 0,
        Scissors,
        Paper,
        NoChoise
    }

    public enum Setting_progress
    {
        RSP = 0,
        Role,
        SelectPosition,
    }

    public enum RSP_Result
    {
        None = 0,
        Win ,
        Defeat,
        draw
    }

    public enum Anistate
    {
        Idel= 0,
        Run,
        Attack,
        Roll,
        Death,
        Victory
    }

    public enum Role
    {
        None = 0,
        Fox,
        Rabbit
    }

    public enum GameProgress
    {
        Set = 0,
        Ready,
        Start,
    }

    public enum GameBattleProgress
    {
        OPEN = 0,
        BUFF_CHOICE,
        INGAME,
        VICTORY
    }

    public enum Round_Result
    {
        None = 0,
        Win,
        defeat
    }
    public enum TableName
    {
        CardTable
    }
}
