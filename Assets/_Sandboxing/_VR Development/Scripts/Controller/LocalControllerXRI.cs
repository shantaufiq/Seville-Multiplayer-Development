using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine;

namespace Seville.Multiplayer.Launcer
{
    public class LocalControllerXRI : ActionBasedController
    {
        [Header("Network Needs")]
        [SerializeField] Transform _relativeTo;

        protected override void Awake()
        {
            if (_relativeTo == null)
            {
                _relativeTo = transform.parent;
            }
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