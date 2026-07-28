using System;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [field: Header("References (External)")]
    [field: SerializeField] public SoMMenuAssets MenuAssets { get; private set; } = null;

    [Header("References (Internal)")]
    [SerializeField] Canvas menuCanvas;
    [SerializeField] Camera menuCamera;
    [SerializeField] SystemMessage systemMessage;
    [field: SerializeField] public HUDController HUD { get; private set; }

    /// <summary>Stores avaliable menus</summary>
    Dictionary<string, MenuBase> menus;

    /// <summary>Used to accumulate a path of visited menus</summary>
    Stack<string> menuHistory;

    /// <summary>Stores the key of the current menu</summary>
    string currentMenuName     = string.Empty;
    MenuBase currentMenuObject = null;

    /// <summary>
    /// Initializes the menu manager
    /// </summary>
    public void Initialize()
    {
        // Initialize and load menu assets
        MenuAssets.Initialize();

        // Here we locate avaliable menus
        menus = new Dictionary<string, MenuBase>();

        foreach (Transform T in menuCanvas.transform)
        {
            // Find all menu implementations inside the canvas.  Skip objects which do not contain a menu.
            MenuBase menu = T.GetComponent<MenuBase>();

            if (menu == null)
                continue;

            // Menu Setup
            menu.Initialize();

            // Store menu in our list of menus
            menus.Add(menu.gameObject.name, menu);

            // All menu controllers should be disabled by default
            T.gameObject.SetActive(false);
        }

        // Set up menu stack related data
        menuHistory       = new Stack<string>();
        currentMenuName   = string.Empty;
        currentMenuObject = null;

        // Initialize system message and HUD too...
        systemMessage.Initialize();
    }

    public void Shutdown()
    {
        // Free menu assets
        foreach (MenuBase menu in menus.Values)
            menu.Shutdown();

        MenuAssets.Free();
    }

    public MenuBase OpenMenu(string name)
    {
        if (menus.TryGetValue(name, out MenuBase foundMenu))
        {
            // We found the menu. Lets store the current menu in our stack, and close the current menu if there is one.
            if (currentMenuName != string.Empty)
            {
                // Store the current menu on the stack...
                menuHistory.Push(currentMenuName);

                // Close the current menu...
                currentMenuObject.Close();
            }

            // Store current menu information
            currentMenuObject = foundMenu;
            currentMenuName   = name;

            // Now we can open the found menu
            currentMenuObject.Open();

            // And finally, return it's implementation
            return currentMenuObject;
        }
        else
            Logger.Warn($"Could not open menu: '{name}'");

        return null;
    }

    public void CloseMenu()
    {
        currentMenuObject.Close();
        currentMenuObject = null;
        currentMenuName   = string.Empty;

        if (menuHistory.Count > 0)
            OpenMenu(menuHistory.Pop());
        else
            Logger.Info("Menu Stack Depleted...");
    }

    /// <summary>
    /// Display a system message on the screen
    /// </summary>
    public void ShowSystemMessage(string message, Action callback = null) =>
        systemMessage.Show(message, callback);
}
