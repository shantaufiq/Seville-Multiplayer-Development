using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine;

namespace Seville.Multiplayer.Launcer
{
    public class LocalControllerXRI : ActionBasedController
    {
        [Header("Network Needs")]
        [SerializeField] Transform _relativeTo;

        [Header("Grab Components")]
        public bool isGrabbing = false;
        public float grabThreshold = 0.5f;
        //False for Desktop mode, true for VR mode: when the hand grab is triggered by other scripts (MouseTeleport in desktop mode), we do not want to update the isGrabbing. It should only be done in VR mode
        public bool updateGrabWithAction = true;

        protected override void Awake()
        {
            base.Awake();

            if (_relativeTo == null)
            {
                _relativeTo = transform.parent;
            }
        }

        protected override void UpdateController()
        {
            base.UpdateController();

            if (updateGrabWithAction)
            {
                isGrabbing = selectActionValue.action.ReadValue<float>() > grabThreshold;
            }

            // Debug.Log($"Grabbing: {isGrabbing}, LocalPosition: {GetLocalPosition()}, LocalRotation: {GetLocalRotation()}");
        }

        public Vector3 GetLocalPosition()
        {
            return _relativeTo.InverseTransformPoint(transform.position);
        }
        public Quaternion GetLocalRotation()
        {
            return Quaternion.Inverse(_relativeTo.rotation) * transform.rotation;
        }

        public void UpdateInputFixed(ref InputDataController container)
        {
            container.LocalPosition = GetLocalPosition();
            container.LocalRotation = GetLocalRotation();

            container.pitchValue = activateActionValue.action.ReadValue<float>();
            container.gripValue = selectActionValue.action.ReadValue<float>();
        }
    }
}