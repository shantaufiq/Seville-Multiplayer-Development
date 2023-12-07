using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace Seville.Multiplayer.Launcer
{
    public struct InputDataController : INetworkStruct
    {
        public Vector3 LocalPosition;
        public Quaternion LocalRotation;
    }

    public struct InputData : INetworkInput
    {

        public Vector3 HeadLocalPosition;
        public Quaternion HeadLocalRotation;

        public InputDataController Left;
        public InputDataController Right;
    }
}