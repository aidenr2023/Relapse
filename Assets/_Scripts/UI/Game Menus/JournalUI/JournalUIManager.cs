using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class JournalUIManager : MonoBehaviour
{
    #region Serialized Fields

    // TODO: Delete
    [SerializeField] private Button backButton;

    [Header("Header Buttons")] [SerializeField]
    private Button objectivesButton;

    [SerializeField] private Button inventoryButton;
    [SerializeField] private Button powersButton;
    [SerializeField] private Button memoriesButton;

    [Header("Menus")] [SerializeField] private GameObject tasksMenu;
    [SerializeField] private GameObject inventoryMenu;
    [SerializeField] private GameObject powersMenu;
    [SerializeField] private GameObject memoriesMenu;

    [Header("Objectives Menu")] [SerializeField]
    private GameObject objectiveContentArea;

    [SerializeField] private JournalUIObjective objectiveUIPrefab;
    [SerializeField] private TMP_Text objectiveDescriptionArea;

    [Header("Inventory Menu")] [SerializeField]
    private GameObject inventoryContentArea;

    [SerializeField] private JournalUIInventoryItem inventoryItemPrefab;

    [Header("Powers Menu")] [SerializeField]
    private GameObject powersContentArea;

    [SerializeField] private JournalUIPowerItem powerItemPrefab;

    [SerializeField] private TMP_Text drugDescriptionText;

    [SerializeField] private Button medsButton;
    [SerializeField] private Button drugsButton;

    [Header("Memories Menu")] [SerializeField]
    private GameObject memoriesContentArea;

    [SerializeField] private TMP_Text memoryDescriptionText;
    [SerializeField] private JournalUIMemoryItem memoryItemPrefab;

    #endregion

    #region Private Fields

    private readonly List<GameObject> _menuList = new();

    private GameObject _currentMenu;

    // Private objective UI fields
    private int _currentObjectiveIndex;
    private readonly List<JournalUIObjective> _objectives = new();

    private PowerType _currentPowerType = PowerType.Drug;

    private PowerScriptableObject _selectedPower;

    private MemoryScriptableObject _selectedMemory;

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

        if (_currentMenu == powersMenu)
            UpdatePowersMenu();
    }

    private void OnEnable()
    {
        // Populate each menu
        PopulateObjectives();
        PopulateMemories();
        PopulateInventory();
        PopulatePowers(_currentPowerType);

        // Open the objectives menu
        OpenObjectivesMenu();
    }

    #region Objectives Menu

    private JournalUIObjective CreateObjectiveButton(JournalObjective objective)
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

        return objectiveUI;
    }

    private void PopulateObjectives()
    {
        // Clear the content area
        foreach (Transform child in objectiveContentArea.transform)
            Destroy(child.gameObject);

        // If the instance of the JournalObjectiveManager is null, return
        if (JournalObjectiveManager.Instance == null)
            return;

        // Get all the completed objectives from the JournalObjectiveManager
        var completeObjectives = JournalObjectiveManager.Instance.CompleteObjectives;

        // Get all the active objectives from the JournalObjectiveManager
        var activeObjectives = JournalObjectiveManager.Instance.ActiveObjectives;

        // Clear the objectives list
        _objectives.Clear();

        JournalUIObjective first = null;

        var objectivesList = new List<JournalUIObjective>();

        // Populate the content area with the objectives
        foreach (var objective in completeObjectives)
        {
            var button = CreateObjectiveButton(objective);

            // Add the button to the list of objectives
            objectivesList.Add(button);

            if (first == null)
                first = button;
        }

        foreach (var objective in activeObjectives)
        {
            var button = CreateObjectiveButton(objective);

            // Add the button to the list of objectives
            objectivesList.Add(button);
        }

        // Set the header navigation down
        if (first != null)
            SetHeaderNavDown(first.Button);
        else
            SetHeaderNavDown(backButton);

        // Set the navigation between objectives
        for (var i = 0; i < objectivesList.Count; i++)
        {
            var button = objectivesList[i].Button;

            var nav = button.navigation;

            // Up navigation
            nav.selectOnUp = i > 0
                ? objectivesList[i - 1].Button
                : objectivesButton;

            // Down navigation
            nav.selectOnDown = i < objectivesList.Count - 1
                ? objectivesList[i + 1].Button
                : backButton;

            // Set the navigation
            button.navigation = nav;
        }

        // Set the back button's up navigation to the last item in the objectives list
        if (objectivesList.Count > 0)
        {
            var lastItem = objectivesList[^1];

            var nav = backButton.navigation;
            nav.selectOnUp = lastItem.Button;
            backButton.navigation = nav;
        }
        else
        {
            var nav = backButton.navigation;
            nav.selectOnUp = objectivesButton;
            backButton.navigation = nav;
        }
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

    #endregion

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

        // Populate the inventory area
        PopulateInventory();
    }

    public void OpenPowersMenu() => OpenPowersMenu(_currentPowerType);

    private void OpenPowersMenu(PowerType powerType)
    {
        // Open the powers menu
        OpenMenu(powersMenu);

        // Populate the powers area
        PopulatePowers(powerType);
    }

    // Open the drug powers menu
    public void OpenDrugPowersMenu() => OpenPowersMenu(PowerType.Drug);

    // Open the medicine powers menu
    public void OpenMedicinePowersMenu() => OpenPowersMenu(PowerType.Medicine);

    public void OpenMemoriesMenu()
    {
        // Open the memories menu
        OpenMenu(memoriesMenu);

        // Populate the memories area
        PopulateMemories();
    }

    #endregion

    #region Inventory Menu

    private JournalUIInventoryItem CreateInventoryItem(InventoryEntry entry)
    {
        var inventoryItem = Instantiate(inventoryItemPrefab, inventoryContentArea.transform);
        inventoryItem.SetInventoryEntry(entry);

        return inventoryItem;
    }

    private void PopulateInventory()
    {
        // Clear the content area
        foreach (Transform child in inventoryContentArea.transform)
            Destroy(child.gameObject);

        // Return if the instance of the InventoryManager is null
        if (Player.Instance?.PlayerInventory == null)
            return;

        // Get the inventory entries
        var inventoryEntries = Player.Instance.PlayerInventory.InventoryEntries;

        JournalUIInventoryItem firstInventoryItem = null;

        // Create a list for the inventory UI items
        var inventoryItems = new List<JournalUIInventoryItem>();

        // Populate the content area with the inventory entries
        foreach (var entry in inventoryEntries)
        {
            var item = CreateInventoryItem(entry);

            // Add the item to the list of inventory items
            inventoryItems.Add(item);

            if (firstInventoryItem == null)
                firstInventoryItem = item;
        }

        // Set the header navigation down
        if (firstInventoryItem != null)
            SetHeaderNavDown(firstInventoryItem.Button);
        else
            SetHeaderNavDown(backButton);

        // Set up the navigation between inventory items
        for (var i = 0; i < inventoryItems.Count; i++)
        {
            var button = inventoryItems[i].Button;

            var nav = button.navigation;

            // Up navigation
            nav.selectOnUp = i > 0
                ? inventoryItems[i - 1].Button
                : inventoryButton;

            // Down navigation
            nav.selectOnDown = i < inventoryItems.Count - 1
                ? inventoryItems[i + 1].Button
                : backButton;

            // Set the navigation
            button.navigation = nav;
        }

        // Set the back button's up navigation to the last inventory item
        if (inventoryItems.Count > 0)
        {
            var lastItem = inventoryItems[^1];

            var nav = backButton.navigation;
            nav.selectOnUp = lastItem.Button;
            backButton.navigation = nav;
        }
        else
        {
            SetHeaderNavDown(backButton);

            var nav = backButton.navigation;
            nav.selectOnUp = inventoryButton;
            backButton.navigation = nav;
        }
    }

    #endregion

    #region Powers Menu

    private JournalUIPowerItem CreatePowerItem(PowerScriptableObject power)
    {
        var powerItem = Instantiate(powerItemPrefab, powersContentArea.transform);
        powerItem.SetPower(power);

        // Add the button click event
        powerItem.Button.onClick.AddListener(() => SetSelectedPower(power));

        // Add the event trigger
        var entry = new EventTrigger.Entry { eventID = EventTriggerType.Select };

        // Add the event trigger callback
        entry.callback.AddListener(_ => SetSelectedPower(power));

        return powerItem;
    }

    private void PopulatePowers(PowerType powerType)
    {
        // Set the current power type
        _currentPowerType = powerType;

        // Clear the content area
        foreach (Transform child in powersContentArea.transform)
            Destroy(child.gameObject);

        // If the instance of the PowerManager is null, return
        if (Player.Instance?.PlayerPowerManager == null)
            return;

        // Get the powers
        var powers = Player.Instance.PlayerPowerManager.Powers;

        // Filter the powers by power type
        var filteredPowers = powers.Where(power => power.PowerType == powerType);

        // Create a list for the power UI items
        var powerItems = new List<JournalUIPowerItem>();

        // Populate the content area with the powers
        foreach (var power in filteredPowers)
        {
            var powerItem = CreatePowerItem(power);

            // Add the power item to the list of power items
            powerItems.Add(powerItem);
        }

        var subheaderButton = powerType == PowerType.Medicine
            ? medsButton
            : drugsButton;

        // Debug.Log($"Setting Nav down to {subheaderButton.name}");

        // Set the header navigation down based on the powertype
        SetHeaderNavDown(subheaderButton);

        // Set the navigation between power items
        for (var i = 0; i < powerItems.Count; i++)
        {
            var button = powerItems[i].Button;

            var nav = button.navigation;

            // Up navigation
            nav.selectOnUp = i > 0
                ? powerItems[i - 1].Button
                : subheaderButton;

            // Down navigation
            nav.selectOnDown = i < powerItems.Count - 1
                ? powerItems[i + 1].Button
                : backButton;

            // Set the navigation
            button.navigation = nav;
        }

        // Set the down navigation of the subheader buttons
        var subHeaders = new[] { medsButton, drugsButton };

        foreach (var subHeader in subHeaders)
        {
            var nav = subHeader.navigation;
            nav.selectOnDown = powerItems.Count > 0
                ? powerItems[0].Button
                : backButton;
            subHeader.navigation = nav;
        }

        // Set the back button's up navigation to the last power item
        if (powerItems.Count > 0)
        {
            var lastItem = powerItems[^1];

            var nav = backButton.navigation;
            nav.selectOnUp = lastItem.Button;
            backButton.navigation = nav;
        }
        else
        {
            var nav = backButton.navigation;
            nav.selectOnUp = subheaderButton;
            backButton.navigation = nav;
        }
    }

    private void UpdatePowersMenu()
    {
        // Return if the selected power is null
        if (_selectedPower == null)
            return;

        // Set the drug description text
        drugDescriptionText.text = _selectedPower.Description;
    }

    public void SetSelectedPower(PowerScriptableObject power)
    {
        _selectedPower = power;
        drugDescriptionText.text = power.Description;
    }

    #endregion

    #region Memories Menu

    private JournalUIMemoryItem CreateMemoryItem(MemoryScriptableObject memory)
    {
        // Return if the memory is null
        if (memory == null)
            return null;

        var memoryItem = Instantiate(memoryItemPrefab, memoriesContentArea.transform);
        memoryItem.SetMemory(memory);

        // Add the button click event
        memoryItem.Button.onClick.AddListener(() => SetSelectedMemory(memory));

        // Add the event trigger
        var entry = new EventTrigger.Entry { eventID = EventTriggerType.Select };

        // Add the event trigger callback
        entry.callback.AddListener(_ => SetSelectedMemory(memory));

        return memoryItem;
    }

    private void PopulateMemories()
    {
        // Clear the content area
        foreach (Transform child in memoriesContentArea.transform)
            Destroy(child.gameObject);

        // Get the list of memories
        var memories = MemoryManager.Instance.Memories;

        // Create a list for the memory UI items
        var memoryItems = new List<JournalUIMemoryItem>();

        JournalUIMemoryItem firstMemoryItem = null;

        foreach (var memory in memories)
        {
            var powerItem = CreateMemoryItem(memory);

            if (firstMemoryItem == null)
                firstMemoryItem = powerItem;
        }

        // Set the header navigation down
        if (firstMemoryItem != null)
            SetHeaderNavDown(firstMemoryItem.Button);
        else
            SetHeaderNavDown(backButton);

        var selectableMemoryItems = memoryItems.Select(item => item.Button).ToList();

        // Add the back button to the list of selectable memory items
        selectableMemoryItems.Add(backButton);

        // Set the navigation between memory items
        for (var i = 0; i < memoryItems.Count; i++)
        {
            var button = memoryItems[i].Button;

            var nav = button.navigation;

            // // Up navigation
            // nav.selectOnUp = i > 0
            //     ? GetClosestElement(button, Vector2.up, selectableMemoryItems)
            //     : backButton;
            //
            // // Down navigation
            // nav.selectOnDown = i < memoryItems.Count - 1
            //     ? GetClosestElement(button, Vector2.down, selectableMemoryItems)
            //     : backButton;
            //
            // // Left navigation
            // nav.selectOnLeft = i > 0
            //     ? GetClosestElement(button, Vector2.left, selectableMemoryItems)
            //     : backButton;
            //
            // // Right navigation
            // nav.selectOnRight = i < memoryItems.Count - 1
            //     ? GetClosestElement(button, Vector2.right, selectableMemoryItems)
            //     : backButton;

            // Up navigation
            nav.selectOnUp = i > 0
                ? memoryItems[i - 1].Button
                : memoriesButton;

            // Down navigation
            nav.selectOnDown = i < memoryItems.Count - 1
                ? memoryItems[i + 1].Button
                : backButton;
            
            // Set the navigation
            button.navigation = nav;
        }

        // Set the back button's up navigation to the last memory item
        if (memoryItems.Count > 0)
        {
            var lastItem = memoryItems[^1];

            var nav = backButton.navigation;
            nav.selectOnUp = lastItem.Button;
            backButton.navigation = nav;
        }
        else
        {
            var nav = backButton.navigation;
            nav.selectOnUp = memoriesButton;
            backButton.navigation = nav;
        }

        return;

        Selectable GetClosestElement(Selectable origin, Vector2 direction, List<Button> elements)
        {
            var closest = origin;

            // Map the elements and their dot products in relation to the direction
            var elementDistances = elements.ToDictionary(
                element => element,
                element =>
                {
                    // Get the dot product
                    var dot = Vector2.Dot(
                        direction,
                        element.transform.position - origin.transform.position
                    );

                    return dot;
                });

            // Sort the elements by their dot product
            var sortedElements = elementDistances.OrderBy(kvp => kvp.Value).ToList();

            // Get the closest element
            closest = sortedElements.First().Key;

            // If the first element has the same dot product as the origin, return the origin
            if (sortedElements.First().Value == 0)
                return origin;

            return closest;
        }
    }

    private void SetSelectedMemory(MemoryScriptableObject memory)
    {
        _selectedMemory = memory;
        memoryDescriptionText.text = memory.LongDescription;
    }

    #endregion

    private void SetHeaderNavDown(Selectable obj)
    {
        // Set up an array of all the header buttons
        var headerButtons = new[] { objectivesButton, inventoryButton, powersButton, memoriesButton };

        // Debug.Log($"Setting header nav down to {obj.name}");

        foreach (var button in headerButtons)
        {
            // If the button is null, continue
            if (button == null)
            {
                // Log error
                Debug.LogError("Button is null");
                continue;
            }

            // If the button is the same as the object, continue
            if (button == obj)
                continue;

            // Get the navigation object
            var nav = button.navigation;

            // Set the down navigation object
            nav.selectOnDown = obj;

            // Set the navigation object
            button.navigation = nav;
        }
    }
}