using UnityEngine;
using Fusion;

namespace Seville.Multiplayer.Launcer
{
    public class LocalRigSpawner : SimulationBehaviour, ISpawned
    {
        [SerializeField] PlayerInputHandler _XRIRig;

        public void Spawned()
        {
            // if (Object.HasInputAuthority)
            // {
            //     SpawnPlayer(_XRIRig);
            // }
        }

        public void SpawnPlayer(Transform transformTarget)
        {
            NetworkObject _networkedParent = GetComponentInParent<NetworkObject>();

            var rig = Instantiate(_XRIRig, transform);
            rig.transform.localPosition = Vector3.zero;
            rig.transform.localRotation = Quaternion.identity;
            rig.networkedParent = _networkedParent;

            this.enabled = false;
        }
    }
}