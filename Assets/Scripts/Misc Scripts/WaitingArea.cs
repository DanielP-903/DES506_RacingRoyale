using System.Collections;
using UnityEngine;

/// <summary>
/// Enables the cursor after a small delay when entering the waiting area
/// </summary>
public class WaitingArea : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(DelayEnable());
    }

    private IEnumerator DelayEnable()
    {
        yield return new WaitForSeconds(0.1f);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
