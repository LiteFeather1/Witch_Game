﻿using UnityEngine;
using LTF.CompositeValue;

namespace Lua.PowerUps
{
    [CreateAssetMenu(menuName = "Power Up/Bullet/Range")]
    public class PowerUpBulletRange : PowerUpModifier
    {
        protected override CompositeValue ValueToModify(Cards.CardManager cm)
        {
            return cm.GameManager.Witch.Gun.BulletDuration;
        }
    }
}
