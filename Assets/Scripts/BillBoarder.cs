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
        //transform.LookAt(Camera.current.transform);
        //Debug.Log("RectTarget: " + Camera.main.transform);
        //Debug.Log("RectRot: "+rect.rotation);
        //Vector3 relative = transform.InverseTransformPoint(mainCam.transform.position);
        //float angle = Mathf.Atan2(relative.x, relative.y) * Mathf.Rad2Deg;
        //transform.rotation = Quaternion.Euler(0, 0, -angle);
        //rect.LookAt(relative);
        //rect.LookAt(mainCam.transform.position);
        if (!useStandardTransform)
        {
            rect.rotation = mainCam.transform.rotation;
        }
        else
        {
            transform.rotation = mainCam.transform.rotation;
        }
    }
}
