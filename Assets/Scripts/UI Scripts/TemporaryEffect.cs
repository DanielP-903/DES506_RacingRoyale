using System.Collections;
using UnityEngine;

/// <summary>
/// A temporary effect which can destroys itself or disable itself based on vfx requirements
/// </summary>
public class TemporaryEffect : MonoBehaviour
{
    public bool doNotDelete;
    public bool isBillboard;
    
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

    /// <summary>
    /// Destroy this object after a period of 3 seconds has expired
    /// </summary>
    private IEnumerator DeleteOverTime()
    {
        yield return new WaitForSeconds(3);
        Destroy(this.gameObject);
    }
}
