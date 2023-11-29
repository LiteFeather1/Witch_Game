﻿using UnityEngine;

[CreateAssetMenu(menuName = "Power Up/Recycle/Refund Drawer")]
public class PowerUpRefundDrawerOnRecycle : PowerUpModifier
{
    protected override CompositeValue ValueToModify(GameManager gm)
    {
        return gm.OnRecycleRefund;
    }
}
