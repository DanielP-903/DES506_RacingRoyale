using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangingTrack : MonoBehaviour
{
    [SerializeField]
    private GameObject Version1, Version2, Version3;
    // Start is called before the first frame update
    void Start()
    {
        if (Version1 != null && Version2 != null && Version3 != null)
        {
            
            Version1.SetActive(false);
            Version2.SetActive(false);
            Version2.SetActive(false);
            int version = Random.Range(0, 3);

            switch (version)
            {
                case 0:
                    Version1.SetActive(true);
                    break;
                case 1:
                    Version2.SetActive(true);
                    break;
                case 2:
                    Version3.SetActive(true);
                    break;
                default:
                    Version1.SetActive(true);
                    break;
            }
        }

    }

}
