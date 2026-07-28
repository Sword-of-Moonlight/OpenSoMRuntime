using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;

using System.Collections.Generic;
using UnityEngine.UI;

public class ButtonList : MonoBehaviour
{
    [field: Header("Configuration")]
    [field: SerializeField] public ButtonListOrientation Orientation { get; private set; }
    [field: SerializeField] public bool WrapSelection { get; private set; }

    [field: Header("References (External)")]
    [field: SerializeField] public GameObject TextButtonPrefab;
    [field: SerializeField] public GameObject ImageButtonPrefab;

    [field: Header("Debugging / Read Only")]
    [field: SerializeField, ReadOnly] public List<ButtonListItem> Items { get; private set; } = new List<ButtonListItem>();

    /// <summary>
    /// MonoBehaviour Implementation.<br/>
    /// Used to find pre-existing buttons inside the list, and store them in the button items list.
    /// </summary>
    void Awake()
    {
        // Find all buttons which are under the game object in the hirachey
        foreach (Transform T in transform)
        {
            // Get the button list item...
            ButtonListItem item = T.GetComponent<ButtonListItem>();

            if (item == null)
                continue;

            // Add the item to this button list
            item.AddToButtonList(this);
        }

        RefreshNavigation();

        Select(0);
    }

    /// <summary>
    /// Rebuild navigation mapping between button items
    /// </summary>
    public void RefreshNavigation()
    {
        for (int i = 0; i < Items.Count; i++)
        {
            Navigation nav = Items[i].navigation;
            nav.mode = Navigation.Mode.Explicit;

            if (Orientation == ButtonListOrientation.Vertical)
            {
                nav.selectOnUp   = i == 0 ? (WrapSelection ? Items[^1] : null) : Items[i - 1];
                nav.selectOnDown = i == Items.Count - 1 ? (WrapSelection ? Items[0] : null) : Items[i + 1];
            }
            else
            {
                nav.selectOnLeft = i == 0 ? (WrapSelection ? Items[^1] : null) : Items[i - 1];
                nav.selectOnRight = i == Items.Count - 1 ? (WrapSelection ? Items[0] : null) : Items[i + 1];
            }

            Items[i].navigation = nav;
        }
    }

    /// <summary>
    /// Selects an item in the list using an index
    /// </summary>
    public void Select(int index)
    {
        if (index < 0 || index >= Items.Count)
            return;

        // Select the item in the event system
        EventSystem.current.SetSelectedGameObject(Items[index].gameObject);
    }

    /// <summary>
    /// Selects an item in the list using the object reference directly
    /// </summary>
    public void Select(ButtonListItem item)
    {
        // Find the index of the item to select
        int index = Items.IndexOf(item);

        if (index < 0)
            return;

        Select(index);
    }

    /// <summary>
    /// Creates a new text button
    /// </summary>
    public ButtonListItem CreateButton(string label, TextAlignmentOptions labelAlignment)
    {
        // Create a new instance of a text button...
        GameObject button = Instantiate(TextButtonPrefab);

        // We must get the text button component
        ButtonListTextItem textItem = button.GetComponent<ButtonListTextItem>();
        textItem.SetLabel(label, labelAlignment);

        // Add the item to this button list
        textItem.AddToButtonList(this);

        return textItem;
    }
}