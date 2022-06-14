using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Powerup", menuName = "Powerup", order = 1)]
public class SO_Powerup : ScriptableObject
{
    public PowerupType powerupType;
    public float chanceOfSpawning = 100.0f;
    public Sprite powerupUIImage;
}
