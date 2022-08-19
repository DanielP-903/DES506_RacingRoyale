using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Point for bots to target
/// Order is the position on a path
/// Choice is the number of the path
/// Choices is an array of multiple possible choices after this point
/// </summary>
/// <returns></returns>
public class BotPoint : MonoBehaviour
{
    public int order;
    public int choice;
    public int[] choices;

}