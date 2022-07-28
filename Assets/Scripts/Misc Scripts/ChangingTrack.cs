using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangingTrack : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        int version = Random.Range(0, 3);
        int count = 0;
        foreach (Transform child in this.transform)
        {
            if(child.transform != null)
            {
                GameObject currOjbect = child.transform.gameObject;
                if (count == version)
                {
                    currOjbect.SetActive(true);
                }
                else
                {
                    currOjbect.SetActive(false);
                }
                
                count++;

            }
        }


    }

}
