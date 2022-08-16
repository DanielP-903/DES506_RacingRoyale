using UnityEngine;

[CreateAssetMenu(fileName = "Connection", menuName = "Connection", order = 1)]
public class SO_Connection : ScriptableObject
{
   // Cause for last disconnection - if it is not a successful "left" it is assumed a "timeout"
   public string cause;
}