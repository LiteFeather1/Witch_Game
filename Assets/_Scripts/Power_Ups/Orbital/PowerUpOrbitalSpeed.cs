using UnityEngine;

[CreateAssetMenu(menuName = "Power Up/Orbital/Rotation Speed")]
public class PowerUpOrbitalSpeed : PowerUpModifier
{
    protected override CompositeValue ValueToModify(GameManager gm)
    {
        return gm.Witch.OrbitalGun.RotationSpeed;
    }
}