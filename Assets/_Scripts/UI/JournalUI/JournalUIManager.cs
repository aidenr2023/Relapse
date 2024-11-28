using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class JournalUIManager : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] private JournalObjective testObjective;

    [Header("Menus")] [SerializeField] private GameObject tasksMenu;
    [SerializeField] private GameObject inventoryMenu;
    [SerializeField] private GameObject powersMenu;
    [SerializeField] private GameObject memoriesMenu;

    [Header("Objectives Menu")] [SerializeField]
    private GameObject objectiveContentArea;

    [SerializeField] private JournalUIObjective objectiveUIPrefab;
    [SerializeField] private TMP_Text objectiveDescriptionArea;

    #endregion

    #region Private Fields

    private readonly List<GameObject> _menuList = new();

    private GameObject _currentMenu;

    // Private objective UI fields
    private int _currentObjectiveIndex;
    private readonly List<JournalUIObjective> _objectives = new();

    #endregion

    private void Awake()
    {
        // Add the menus to the menu list
        _menuList.Add(tasksMenu);
        _menuList.Add(inventoryMenu);
        _menuList.Add(powersMenu);
        _menuList.Add(memoriesMenu);
    }

    private void Start()
    {
        // Open the objectives menu by default
        OpenObjectivesMenu();
    }

    private void Update()
    {
        if (_currentMenu == null)
            return;

        if (_currentMenu == tasksMenu)
            UpdateObjectivesMenu();
    }

    #region Objectives Menu

    private void CreateObjectiveButton(JournalObjective objective)
    {
        var objectiveUI = Instantiate(objectiveUIPrefab, objectiveContentArea.transform);
        objectiveUI.SetObjective(objective);

        // Add the objective to the list of objectives
        _objectives.Add(objectiveUI);

        // Add the button click event
        objectiveUI.Button.onClick.AddListener(() => ChangeJournalObjectiveIndex(_objectives.IndexOf(objectiveUI)));

        // Add the event trigger
        var entry = new EventTrigger.Entry { eventID = EventTriggerType.Select };

        // Add the event trigger callback
        entry.callback.AddListener(_ => ChangeJournalObjectiveIndex(_objectives.IndexOf(objectiveUI)));

        // Add the event trigger entry
        objectiveUI.EventTrigger.triggers.Add(entry);
    }

    private void PopulateObjectives()
    {
        // Clear the content area
        foreach (Transform child in objectiveContentArea.transform)
            Destroy(child.gameObject);

        // // TODO: Delete
        // for (var i = 0; i < 10; i++)
        //     CreateObjectiveButton(testObjective);

        // If the instance of the JournalObjectiveManager is null, return
        if (JournalObjectiveManager.Instance == null)
            return;

        // Get all the completed objectives from the JournalObjectiveManager
        var completeObjectives = JournalObjectiveManager.Instance.CompleteObjectives;

        // Get all the active objectives from the JournalObjectiveManager
        var activeObjectives = JournalObjectiveManager.Instance.ActiveObjectives;

        // Clear the objectives list
        _objectives.Clear();

        // Populate the content area with the objectives
        foreach (var objective in completeObjectives)
            CreateObjectiveButton(objective);

        foreach (var objective in activeObjectives)
            CreateObjectiveButton(objective);
    }

    private void ChangeJournalObjectiveIndex(int index)
    {
        // If the list of objectives is null, return
        if (_objectives == null)
            return;

        // If the list of objectives is empty, return
        if (_objectives.Count == 0)
            return;

        // Set the current objective index
        _currentObjectiveIndex = index;

        // Set the objective description area text
        objectiveDescriptionArea.text = _objectives[_currentObjectiveIndex].Objective.ShortDescription;
    }

    public void NextJournalObjective()
    {
        ChangeJournalObjectiveIndex(_currentObjectiveIndex + 1);
    }

    public void PreviousJournalObjective()
    {
        ChangeJournalObjectiveIndex(_currentObjectiveIndex - 1);
    }

    private void UpdateObjectivesMenu()
    {
        // Return if the list of objectives is null
        if (_objectives == null)
            return;

        // Return if the list of objectives is empty
        if (_objectives.Count == 0)
            return;

        // Return if the current objective index is out of range
        if (_currentObjectiveIndex < 0 || _currentObjectiveIndex >= _objectives.Count)
            return;

        // Set the objective description area text
        objectiveDescriptionArea.text = _objectives[_currentObjectiveIndex].Objective.LongDescription;
    }

    #region Open Menus

    private void OpenMenu(GameObject menu)
    {
        // If the menu is not in the menuList, return
        if (!_menuList.Contains(menu))
            throw new Exception("Menu not found in the menu list");

        // Close all menus
        foreach (var m in _menuList)
            m.SetActive(false);

        // Open the selected menu
        menu.SetActive(true);

        // Set the current menu
        _currentMenu = menu;
    }

    public void OpenObjectivesMenu()
    {
        // Open the tasks menu
        OpenMenu(tasksMenu);

        // Populate the objectives area
        PopulateObjectives();
    }

    public void OpenInventoryMenu()
    {
        // Open the inventory menu
        OpenMenu(inventoryMenu);
    }

    public void OpenPowersMenu()
    {
        // Open the powers menu
        OpenMenu(powersMenu);
    }

    public void OpenMemoriesMenu()
    {
        // Open the memories menu
        OpenMenu(memoriesMenu);
    }

    #endregion

    #endregion
}