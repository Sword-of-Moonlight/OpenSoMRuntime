using System;
using UnityEngine;

public class MenuBase : MonoBehaviour
{
    [SerializeField] protected CanvasGroup canvasGroup;

    public event Action Opened;
    public event Action Closed;

    /// <summary>Used to store if this menu has been opened before</summary>
    protected bool IsFirstOpen   { get; private set; } = true;
    protected bool IsInitialized { get; private set; } = false;

    /// <summary>
    /// Called to open the menu. When overriding, call the base LAST!!
    /// </summary>
    public virtual void Open()
    {
        IsFirstOpen = false;
        Opened?.Invoke();

        gameObject.SetActive(true);
    }

    /// <summary>
    /// Called to close the menu. When overriding, call the base LAST!!
    /// </summary>
    public virtual void Close()
    {
        Closed?.Invoke();

        gameObject.SetActive(false);
    }

    /// <summary>
    /// Called to initialize menu data.
    /// </summary>
    public virtual void Initialize()
    {
        IsInitialized = true;
    }

    /// <summary>
    /// Called at the end of game execution to clean up any data.
    /// </summary>
    public virtual void Shutdown()
    {
        IsInitialized = false;
    }
}
