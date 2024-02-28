using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Seville.Multiplayer.Launcer;

public class NetworkGrabber : NetworkBehaviour
{
    // [HideInInspector]
    public Hand hand;
    public override void Spawned()
    {
        base.Spawned();
        // hand = GetComponentInParent<Hand>();

        if (hand.IsLocalNetworkRig)
        {
            Debug.Log($"is local network rig");
            // References itself in its local counterpart, to simplify the lookup during local grabbing
            if (hand.LocalHardwareHand)
            {
                LocalGrabInteractor grabber = hand.LocalHardwareHand.GetComponentInChildren<LocalGrabInteractor>();
                grabber.networkGrabber = this;
            }
        }
    }
}
