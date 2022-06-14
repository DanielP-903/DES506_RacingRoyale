using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BillBoarder : MonoBehaviour
{
    private RectTransform rect;
    private Camera mainCam;
    
    // Start is called before the first frame update
    void Start()
    {
        rect = GetComponent<RectTransform>();
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
        rect.rotation = mainCam.transform.rotation;
    }
}
