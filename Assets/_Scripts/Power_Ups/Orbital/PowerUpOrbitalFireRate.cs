﻿using UnityEngine;
using CompositeValues;
using Lua.Managers;

namespace Lua.PowerUps
{
    [CreateAssetMenu(menuName = "Power Up/Orbital/Fire Rate")]
    public class PowerUpOrbitalFireRate : PowerUpModifier
    {
        protected override CompositeValue ValueToModify(GameManager gm)
        {
            return gm.Witch.OrbitalShootTime;
        }
    }
}
