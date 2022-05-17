using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyCondition
{
    NearPlayer      = (1 << 0),
    IsDamaged       = (1 << 1),
    HasLowHealth    = (1 << 2)
}

public enum MusicType
{
    MENU,
    TUTORIAL,
    BOSS
}
