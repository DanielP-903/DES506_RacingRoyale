using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dissolve : MonoBehaviour
{
    private MeshRenderer _meshRenderer;
    private static readonly int FadeOutSlider = Shader.PropertyToID("FadeOutSlider");
    private bool _dissolve = false;
    private float _dissolveTimer;

    private bool _canDetect = true;
    // Start is called before the first frame update
    void Start()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        _dissolveTimer = 0.0f;
        _canDetect = true;
        _dissolve = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (_dissolve)
        {
            _dissolveTimer += Time.deltaTime;
            _dissolveTimer = Mathf.Clamp(_dissolveTimer, 0, 3);
            float dissolveFloat = Mathf.Lerp(1, 0, (3 - _dissolveTimer) / 3);
            _meshRenderer.materials[0].SetFloat(FadeOutSlider, dissolveFloat);
            _meshRenderer.materials[1].SetFloat(FadeOutSlider, dissolveFloat);
            if (dissolveFloat >= 0.8f)
            {
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("EliminationZone") && _canDetect)
        {
            StartCoroutine(DelayDissolve());
        }
    }

    private IEnumerator DelayDissolve()
    {
        _canDetect = false;
        yield return new WaitForSeconds(3);
        _dissolve = true;
        _dissolveTimer = 0.0f;
    }
}
