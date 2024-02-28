using Fusion;
using UnityEngine;

namespace Seville.Multiplayer.Launcer
{

    [RequireComponent(typeof(NetworkTransform))]
    [OrderAfter(typeof(NetworkTransform), typeof(NetworkRigidbody))]
    public class Hand : NetworkBehaviour
    {
        public NetworkTransform networkTransform;

        NetworkPlayer rig;
        public LocalControllerXRI LocalController { get; set; }

        public Animator handAnimator;

        public bool IsLocalNetworkRig => rig.IsLocalNetworkRig;

        public LocalControllerXRI LocalHardwareHand => IsLocalNetworkRig ? LocalController : null;

        private void Awake()
        {
            rig = GetComponentInParent<NetworkPlayer>();
            // networkTransform = GetComponent<NetworkTransform>();
        }

        public void SetLocalController(LocalControllerXRI other)
        {
            LocalController = other;

            if (LocalController != null)
            {
                var nt = GetComponent<NetworkTransform>();
                nt.InterpolationDataSource = InterpolationDataSources.NoInterpolation;
            }
        }

        // update value from NetworkPlayer
        public void UpdateInput(InputDataController input)
        {
            UpdatePose(input.LocalPosition, input.LocalRotation);

            SetAnimation(input.pitchValue, input.gripValue);
        }

        void SetAnimation(float pitch, float grip)
        {
            handAnimator.SetFloat("Trigger", pitch);
            handAnimator.SetFloat("Grip", grip);
        }

        void UpdatePose(Vector3 localPosition, Quaternion localRotation)
        {
            transform.localPosition = localPosition;
            transform.localRotation = localRotation;
        }

        public void UpdateLocalPose(Vector3 localPosition, Quaternion localRotation)
        {
            transform.localPosition = localPosition;
            transform.localRotation = localRotation;
        }

        public Vector3 GetWorldPosition()
        {
            if (LocalController != null)
            {
                return transform.parent.TransformPoint(LocalController.GetLocalPosition());
            }
            return transform.position;
        }

        public Quaternion GetWorldRotation()
        {
            if (LocalController != null)
            {
                return transform.parent.rotation * LocalController.GetLocalRotation();
            }
            return transform.rotation;
        }

        public override void Render()
        {
            if (LocalController != null)
            {
                UpdateLocalPose(LocalController.GetLocalPosition(), LocalController.GetLocalRotation());
            }
        }
    }
}