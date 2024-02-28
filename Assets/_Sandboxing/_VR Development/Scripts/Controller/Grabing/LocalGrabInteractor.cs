
using System.Collections.Generic;
using UnityEngine;
using Seville.Multiplayer.Launcer;

public class LocalGrabInteractor : MonoBehaviour
{
    [SerializeField] LocalControllerXRI hand;

    Collider lastCheckedCollider;
    LocalGrabbable lastCheckColliderGrabbable;
    public LocalGrabbable grabbedObject;
    // Will be set by the NetworkGrabber for the local user itself, when it spawns
    public NetworkGrabber networkGrabber;

    private void Awake()
    {
        // hand = GetComponentInParent<LocalControllerXRI>();
    }

    void OnTriggerEnter(Collider other)
    {
        //? Debug.Log("Object touched by hand...");
    }

    private void OnTriggerStay(Collider other)
    {
        // Exit if an object is already grabbed
        if (grabbedObject != null)
        {
            // It is already the grabbed object or another, but we don't allow shared grabbing here
            return;
        }

        LocalGrabbable grabbable;

        if (lastCheckedCollider == other)
        {
            grabbable = lastCheckColliderGrabbable;
        }
        else
        {
            grabbable = other.GetComponentInParent<LocalGrabbable>();
        }
        // To limit the number of GetComponent calls, we cache the latest checked collider grabbable result
        lastCheckedCollider = other;
        lastCheckColliderGrabbable = grabbable;
        if (grabbable != null)
        {
            //? Debug.Log($"LocalGrabbable stay on hand");

            bool wasHovered = hoveredGrabbables.Contains(grabbable);

            if (hand.isGrabbing)
            {
                if (wasHovered || grabbable.allowedClosedHandGrabing)
                {
                    Grab(grabbable);
                }
            }
            else
            {
                if (!hoveredGrabbables.Contains(grabbable))
                {
                    hoveredGrabbableByColliders[other] = grabbable;
                    hoveredGrabbables.Add(grabbable);
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (hoveredGrabbableByColliders.ContainsKey(other))
        {
            if (hoveredGrabbables.Contains(hoveredGrabbableByColliders[other]))
            {
                //? Debug.Log("Object released");

                hoveredGrabbables.Remove(hoveredGrabbableByColliders[other]);
            }
            hoveredGrabbableByColliders.Remove(other);
        }
    }

    Dictionary<Collider, LocalGrabbable> hoveredGrabbableByColliders = new Dictionary<Collider, LocalGrabbable>();
    public List<LocalGrabbable> hoveredGrabbables = new List<LocalGrabbable>();

    public void Grab(LocalGrabbable grabbable)
    {
        grabbable.Grab(this);
        grabbedObject = grabbable;
    }

    public void Ungrab(LocalGrabbable grabbable)
    {
        grabbedObject.Ungrab();
        grabbedObject = null;
    }

    private void Update()
    {
        // Check if the local hand is still grabbing the object
        if (grabbedObject != null && !hand.isGrabbing)
        {
            // Object released by this hand
            Ungrab(grabbedObject);
        }
        CheckHovered();
    }

    void CheckHovered()
    {
        // Hovered object may have been destroyed while being hovered. Destroyed gameobjects respond to "== null" while staying in collections
        foreach (var key in hoveredGrabbableByColliders.Keys)
        {
            if (key == null)
            {
                hoveredGrabbableByColliders.Remove(key);
                break;
            }
        }
        hoveredGrabbables.RemoveAll(g => g == null);
    }
}