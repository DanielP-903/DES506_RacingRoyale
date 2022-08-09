using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
