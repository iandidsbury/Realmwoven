using UnityEngine;

public abstract class Hittable : MonoBehaviour
{
    public virtual void OnHoverEnter() { }
    public virtual void OnHoverExit() { }
    public virtual void OnInteract() { }
}
