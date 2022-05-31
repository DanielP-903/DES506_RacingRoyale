using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialRotator : MonoBehaviour
{
    private float _rotationalValue = 0;
    public PlayerHUDController _hudRef;
    private RectTransform _rectTransform;

    private void Start()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        _rotationalValue = Mathf.Clamp(_hudRef.currentSpeed, 0, 99);
        transform.rotation = new Quaternion(0,0,0,0);
        //transform.RotateAround(transform.position - new Vector3(0,40,0),Vector3.forward, _rotationalValue);
    }
}