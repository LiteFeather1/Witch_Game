﻿using UnityEngine;
using CompositeValues;
using Lua.Managers;

namespace Lua.PowerUps
{
    [CreateAssetMenu(menuName = "Power Up/Drawer/Drawing Speed")]
    public class PowerUpDrawingSpeed : PowerUpModifier
    {
        protected override CompositeValue ValueToModify(GameManager gm)
        {
            return gm.CardManager.TimeToDrawCard;
        }
    }
}
