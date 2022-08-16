using UnityEngine;

/// <summary>
/// Powerup scriptable object stores the type and icon for a new powerup
/// </summary>
[CreateAssetMenu(fileName = "Powerup", menuName = "Powerup", order = 1)]
public class SO_Powerup : ScriptableObject
{
    public PowerupType powerupType;
    public Sprite powerupUIImage;
}
