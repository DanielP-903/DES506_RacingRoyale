using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class TutorialMenu : MonoBehaviour
{
    public List<GameObject> tutorialPages;

    private int _currentTutPage = 0;

    private bool _activateTutorial;

    public GameObject previousArrow; 
    public GameObject nextArrow;   
    public GameObject previousArrowShadow; 
    public GameObject nextArrowShadow;

    private void Start()
    {
        previousArrow.SetActive(false);
        previousArrowShadow.SetActive(false);
        _activateTutorial = true;
    }

    // Update is called once per frame
    void Update()
    {
        gameObject.SetActive(SceneManager.GetActiveScene().name == "WaitingArea" && _activateTutorial);
        if (SceneManager.GetActiveScene().name == "WaitingArea" && _activateTutorial && Cursor.lockState != CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else if (SceneManager.GetActiveScene().name != "WaitingArea" || !_activateTutorial)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

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

    public void ExitTutorial()
    {
        _activateTutorial = false;
    }
    
    private void UpdatePages()
    {
        for (int i = 0; i < tutorialPages.Count; i++)
        {
            tutorialPages[i].SetActive(i == _currentTutPage);
        }
    }
}