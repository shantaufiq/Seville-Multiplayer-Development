using Fusion;
using UnityEngine;

namespace Seville.Multiplayer.Launcer
{
    public class Hand : NetworkBehaviour
    {
        public LocalControllerXRI LocalController { get; set; }

        public void SetLocalController(LocalControllerXRI other)
        {
            LocalController = other;

            if (LocalController != null)
            {
                var nt = GetComponent<NetworkTransform>();
                nt.InterpolationDataSource = InterpolationDataSources.NoInterpolation;
            }
        }

        public void UpdateInput(InputDataController input)
        {
            UpdatePose(input.LocalPosition, input.LocalRotation);
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