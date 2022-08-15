using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class TutorialMenu : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> tutorialPages;

    [SerializeField]
    private GameObject previousArrow; 
    
    [SerializeField]
    private GameObject nextArrow;   
    
    [SerializeField]
    private GameObject previousArrowShadow; 
    
    [SerializeField]
    private GameObject nextArrowShadow;

    private int _currentTutPage;
    private bool _activateTutorial;

    private void Start()
    {
        previousArrow.SetActive(false);
        previousArrowShadow.SetActive(false);
        _activateTutorial = true;
    }

    // Update is called once per frame
    void Update()
    {
        // Toggle tutorial menu based on stage name and if player has not closed the tutorial window
        gameObject.SetActive(SceneManager.GetActiveScene().name == "WaitingArea" && _activateTutorial);
        
        // Change cursor state dependant on if tutorial menu is being displayed
        if (SceneManager.GetActiveScene().name == "WaitingArea" && _activateTutorial && Cursor.lockState != CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else if (SceneManager.GetActiveScene().name != "WaitingArea" || !_activateTutorial)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    // Button: Go to the next page
    public void NextPage()
    {
        _currentTutPage++;
        if (_currentTutPage >= tutorialPages.Count-1)
        {
            _currentTutPage = tutorialPages.Count-1;
            nextArrow.SetActive(false);
            nextArrowShadow.SetActive(false);
            EventSystem.current.SetSelectedGameObject(previousArrow);
        }
        else if (previousArrow.activeInHierarchy == false)
        {
            previousArrow.SetActive(true);
            previousArrowShadow.SetActive(true);
        }
        UpdatePages();
    }
    
    // Button: Go to the previous page
    public void PreviousPage()
    {
        _currentTutPage--;
        if (_currentTutPage <= 0)
        {
            _currentTutPage = 0;
            previousArrow.SetActive(false);
            previousArrowShadow.SetActive(false);
            EventSystem.current.SetSelectedGameObject(nextArrow);
        }
        else if (nextArrow.activeInHierarchy == false)
        {
            nextArrow.SetActive(true);
            nextArrowShadow.SetActive(true);
        }
        UpdatePages();
    }

    // Button: Quit the tutorial window
    public void ExitTutorial()
    {
        _activateTutorial = false;
    }
    
    // Update the current page
    private void UpdatePages()
    {
        for (int i = 0; i < tutorialPages.Count; i++)
        {
            tutorialPages[i].SetActive(i == _currentTutPage);
        }
    }
}