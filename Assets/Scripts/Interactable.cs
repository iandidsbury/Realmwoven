using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    public virtual void OnHoverEnter() { }
    public virtual void OnHoverExit() { }
    public virtual void OnInteract() { }
}
