using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BillBoarder : MonoBehaviour
{
    private Component component;
    private RectTransform rect;
    private Camera mainCam;

    private bool useStandardTransform = false; 
    // Start is called before the first frame update
    void Start()
    {
        if (TryGetComponent(typeof(RectTransform), out component))
        {
            rect = GetComponent<RectTransform>();
            useStandardTransform = false;
        }
        else
        {
            useStandardTransform = true;
        }
        mainCam = Camera.main;
        //SceneManager.sceneLoaded += LoadPlayerInLevel;
        //Debug.Log("Rect: "+rect);
    }

    /*private void LoadPlayerInLevel(Scene scene, LoadSceneMode loadSceneMode)
    {
        mainCam = Camera.main;
    }*/

    // Update is called once per frame
    void Update()
    {
        if (!useStandardTransform)
        {
            if (!mainCam)
            {
                GameObject mainCamObject = GameObject.FindGameObjectWithTag("MainCamera");
                if (mainCamObject)
                {
                    mainCam = mainCamObject.GetComponent<Camera>();
                }
                else
                {
                    return;
                }
            }
            
            rect.rotation = mainCam.transform.rotation;
        }
        else
        {
            transform.rotation = mainCam.transform.rotation;
        }
    }
}
