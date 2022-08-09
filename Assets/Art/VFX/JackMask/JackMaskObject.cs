using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JackMaskObject : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject[] MaskObject;

    void Start()
    {
        for (int i =0; i < MaskObject.Length; i++)
        {
            MaskObject[i].GetComponent<MeshRenderer>().material.renderQueue = 3002;
        }
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mouse = Input.mousePosition;
        Ray castRay = Camera.main.ScreenPointToRay(mouse);
        RaycastHit hit;
        if (Physics.Raycast(castRay, out hit, Mathf.Infinity))
        {
            transform.position = hit.point;
        }
    }
}
