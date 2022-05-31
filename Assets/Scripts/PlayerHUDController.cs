using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHUDController : MonoBehaviour
{
     
    private Dan_CarController _carController;
    
    // Start is called before the first frame update
    void Start()
    {
        _carController = GetComponent<Dan_CarController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
