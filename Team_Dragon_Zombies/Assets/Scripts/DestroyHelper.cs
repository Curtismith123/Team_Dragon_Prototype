using UnityEngine;
using System;

public class DestroyHelper : MonoBehaviour
{
    public event Action OnDestroyed;

    private void OnDestroy()
    {
        OnDestroyed?.Invoke();
    }

    public void RegisterOnDestroyed(Action callback)
    {
        OnDestroyed += callback;
    }
}