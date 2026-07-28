using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using System;

public abstract class ButtonListItem : Selectable, ISubmitHandler, IPointerClickHandler
{
    // Events for binding...
    public event Action Pressed;

    // Properties
    public bool Disabled = false;

    /// <summary>
    /// The parent list of the item
    /// </summary>
    public ButtonList List { get; protected set; }

    /// <summary>
    /// Initialize the button list item for a list
    /// </summary>
    public virtual void AddToButtonList(ButtonList parentList)
    {
        List      = parentList;

        parentList.Items.Add(this);

        // We should also set the transform parent to that of the button list
        transform.SetParent(parentList.transform, false);

        parentList.RefreshNavigation();
    }

    /// <summary>
    /// Event System Callback.<br/>
    /// Called when the mouse pointer enters the area of the button
    /// </summary>
    public override void OnPointerEnter(PointerEventData eventData)
    {
        // Handle base selectable event
        base.OnPointerEnter(eventData);

        // Select this item in the list
        List.Select(this);
    }
        
    /// <summary>
    /// Event System Callback.<br/>
    /// Called when the object is selected in the event system
    /// </summary>
    public override void OnSelect(BaseEventData eventData)
    {
        base.OnSelect(eventData);
    }

    /// <summary>
    /// Event System Callback.<br/>
    /// Called when the object is unselected in the event system
    /// </summary>
    public override void OnDeselect(BaseEventData eventData)
    {
        base.OnDeselect(eventData);
    }

    /// <summary>
    /// Event System Callback.<br/>
    /// Called when the button is activated
    /// </summary>
    public void OnSubmit(BaseEventData eventData)
    {
        OnPressed();

        eventData.Use();
    }


    /// <summary>
    /// Event System Callback.<br/>
    /// Called when the button is clicked
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        OnPressed();

        eventData.Use();
    }
        
    /// <summary>
    /// Called internally when the button is pressed by either click or key
    /// </summary>
    public virtual void OnPressed()
    {
        if (!Disabled)
            Pressed?.Invoke();
    }
}
