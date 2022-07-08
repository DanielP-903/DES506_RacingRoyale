using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TemporaryEffect : MonoBehaviour
{
    public bool doNotDelete = false;
    public bool isBillboard = false;
    
    // Start is called before the first frame update
    void Start()
    {
        if (!doNotDelete)
        {
            StartCoroutine(DeleteOverTime());
        }

        if (!isBillboard)
        {
            GetComponent<BillBoarder>().enabled = false;
        }
    }

    private IEnumerator DeleteOverTime()
    {
        yield return new WaitForSeconds(3);
        Destroy(this.gameObject);
    }
}
