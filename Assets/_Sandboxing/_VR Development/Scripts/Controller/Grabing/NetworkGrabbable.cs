using UnityEngine;
using UnityEngine.Events;
using Fusion;
using Fusion.XR.Shared;
using System.Threading.Tasks;

public class NetworkGrabbable : NetworkBehaviour
{
    public NetworkObject myNetworkObject;

    [HideInInspector]
    public NetworkTransform networkTransform;
    public NetworkRigidbody networkRigidbody;

    [Networked(OnChanged = nameof(OnGrabberChanged))]
    public NetworkGrabber CurrentGrabber { get; set; }

    [Networked]
    public Vector3 LocalPositionOffset { get; set; }

    [Networked]
    public Quaternion LocalRotationOffset { get; set; }

    public bool IsGrabbed => Object != null && CurrentGrabber != null; // We make sure that we are online before accessing [Networked] var


    [Header("Events")]
    public UnityEvent onDidUngrab = new UnityEvent();
    public UnityEvent<NetworkGrabber> onDidGrab = new UnityEvent<NetworkGrabber>();

    [Header("Advanced options")]
    public bool extrapolateWhileTakingAuthority = true;
    public bool isTakingAuthority = false;

    [HideInInspector]
    public LocalGrabbable grabbable;

    // Callback that will be called on all clients on grabber change (grabbing/ungrabbing)
    public static void OnGrabberChanged(Changed<NetworkGrabbable> changed)
    {
        // We load the previous state to find what was the grabber before
        changed.LoadOld();
        NetworkGrabber previousGrabber = null;
        if (changed.Behaviour.CurrentGrabber != null)
        {
            previousGrabber = changed.Behaviour.CurrentGrabber;
        }
        // We reload the current state to see the current grabber
        changed.LoadNew();

        changed.Behaviour.HandleGrabberChange(previousGrabber);
    }

    protected virtual void HandleGrabberChange(NetworkGrabber previousGrabber)
    {
        if (previousGrabber)
        {
            DidUngrab();
        }
        if (CurrentGrabber)
        {
            DidGrab();
        }
    }

    protected virtual void Awake()
    {
        networkTransform = GetComponent<NetworkTransform>();
        networkRigidbody = GetComponent<NetworkRigidbody>();
        grabbable = GetComponent<LocalGrabbable>();
        if (grabbable == null)
        {
            Debug.LogError("NetworkGrabbable requires a Grabbable");// We do not use requireComponent as this classes can be subclassed
            gameObject.AddComponent<LocalGrabbable>();
        }
    }

    #region Interface for local Grabbable (when the local user grab/ungrab this object)
    public virtual void LocalUngrab()
    {
        CurrentGrabber = null;
    }

    // public async virtual void LocalGrab()
    // {
    //     //? Debug.Log($"possition on network {grabbable.localPositionOffset}"); udah sampe sini

    //     // Ask and wait to receive the stateAuthority to move the object
    //     isTakingAuthority = true;
    //     var c = await WaitForStateAuthority(myNetworkObject);
    //     isTakingAuthority = false;

    //     // We waited to have the state authority before setting Networked vars
    //     LocalPositionOffset = grabbable.localPositionOffset;
    //     LocalRotationOffset = grabbable.localRotationOffset;

    //     if (c) Debug.Log($"possition on network: {LocalPositionOffset}");

    //     // Update the CurrentGrabber in order to start following position in the FixedUpdateNetwork
    //     if (grabbable.currentGrabber == null)
    //     {
    //         return;
    //     }
    //     CurrentGrabber = grabbable.currentGrabber.networkGrabber;
    // }

    // public async Task<bool> WaitForStateAuthority(NetworkObject o, float maxWaitTime = 8f)
    // {
    //     Debug.Log($"object hasn't state authority");

    //     float waitStartTime = Time.time;
    //     o.RequestStateAuthority();
    //     while (!o.HasStateAuthority && (Time.time - waitStartTime) < maxWaitTime)
    //     {

    //         await AsyncTask.Delay(1);
    //     }
    //     return o.HasStateAuthority;
    // }

    public async virtual void LocalGrab()
    {
        isTakingAuthority = true;
        bool hasAuthority = await WaitForStateAuthority(myNetworkObject);
        isTakingAuthority = false;

        if (!hasAuthority)
        {
            Debug.Log("Failed to get state authority. Aborting grab operation.");
            return; // Keluar dari fungsi jika gagal mendapatkan otoritas
        }

        // Lanjutkan dengan operasi setelah mendapatkan otoritas
        LocalPositionOffset = grabbable.localPositionOffset;
        LocalRotationOffset = grabbable.localRotationOffset;

        if (hasAuthority) Debug.Log($"Position on network: {LocalPositionOffset}");

        if (grabbable.currentGrabber == null)
        {
            return;
        }
        CurrentGrabber = grabbable.currentGrabber.networkGrabber;
    }

    public async Task<bool> WaitForStateAuthority(NetworkObject o, float maxWaitTime = 8f)
    {
        Debug.Log("Requesting state authority for the object.");

        float waitStartTime = Time.time;
        o.RequestStateAuthority();

        while (!o.HasStateAuthority && (Time.time - waitStartTime) < maxWaitTime)
        {
            // Tunggu selama 100ms sebelum cek kondisi lagi.
            // Ini memberikan waktu yang cukup untuk operasi jaringan dan mengurangi beban CPU.
            await Task.Delay(100);

            // Opsional: Tambahkan log untuk debugging.
            Debug.Log($"Waiting for state authority... {(Time.time - waitStartTime)} seconds elapsed.");
        }

        if (o.HasStateAuthority)
        {
            Debug.Log("State authority acquired.");
        }
        else
        {
            Debug.Log("Failed to acquire state authority within the maximum wait time.");
        }

        return o.HasStateAuthority;
    }

    #endregion

    public override void Spawned()
    {
        base.Spawned();
        if (networkRigidbody)
        {
            // We store the default kinematic state, while it is not affected by NetworkRigidbody logic
            grabbable.expectedIsKinematic = (networkRigidbody.ReadNetworkRigidbodyFlags() & NetworkRigidbodyBase.NetworkRigidbodyFlags.IsKinematic) != 0;
        }
    }

    protected virtual void DidGrab()
    {
        grabbable.DidUngrab();
        if (onDidUngrab != null) onDidUngrab.Invoke();
    }

    protected virtual void DidUngrab()
    {
        grabbable.DidUngrab();
        if (onDidUngrab != null) onDidUngrab.Invoke();
    }

    public override void FixedUpdateNetwork()
    {
        // We only update the object position if we have the state authority
        if (!Object.HasStateAuthority) return;

        if (!IsGrabbed) return;
        // Follow grabber, adding position/rotation offsets
        grabbable.Follow(followingtransform: transform, followedTransform: CurrentGrabber.transform, LocalPositionOffset, LocalRotationOffset);
    }

    public override void Render()
    {
        if (isTakingAuthority && extrapolateWhileTakingAuthority)
        {
            // If we are currently taking the authority on the object due to a grab, the network info are still not set
            //  but we will extrapolate anyway (if the option extrapolateWhileTakingAuthority is true) to avoid having the grabbed object staying still until we receive the authority
            ExtrapolateWhileTakingAuthority();
            return;
        }

        // No need to extrapolate if the object is not grabbed
        if (!IsGrabbed) return;

        // Extrapolation: Make visual representation follow grabber, adding position/rotation offsets
        // We extrapolate for all users: we know that the grabbed object should follow accuratly the grabber, even if the network position might be a bit out of sync
        grabbable.Follow(followingtransform: networkTransform.InterpolationTarget.transform, followedTransform: CurrentGrabber.hand.networkTransform.InterpolationTarget.transform, LocalPositionOffset, LocalRotationOffset);
    }

    protected virtual void ExtrapolateWhileTakingAuthority()
    {
        // No need to extrapolate if the object is not really grabbed
        if (grabbable.currentGrabber == null) return;
        NetworkGrabber networkGrabber = grabbable.currentGrabber.networkGrabber;

        // Extrapolation: Make visual representation follow grabber, adding position/rotation offsets
        // We use grabberWhileTakingAuthority instead of CurrentGrabber as we are currently waiting for the authority transfer: the network vars are not already set, so we use the temporary versions
        grabbable.Follow(followingtransform: networkTransform.InterpolationTarget.transform, followedTransform: networkGrabber.hand.networkTransform.InterpolationTarget.transform, grabbable.localPositionOffset, grabbable.localRotationOffset);
    }
}
