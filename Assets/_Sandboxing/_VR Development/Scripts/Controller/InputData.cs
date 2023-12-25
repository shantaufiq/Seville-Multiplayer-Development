using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace Seville.Multiplayer.Launcer
{
    public struct InputDataController : INetworkStruct
    {
        public Vector3 LocalPosition;
        public Quaternion LocalRotation;

        public float pitchValue;
        public float gripValue;
    }

    public struct InputDataEmoji : INetworkStruct
    {
        public int emojiIndex;
        public bool OnEmojiSelected;

        public void ResetEmojiData()
        {
            emojiIndex = -1;
            OnEmojiSelected = false;
        }
    }

    public struct InputData : INetworkInput
    {

        public Vector3 HeadLocalPosition;
        public Quaternion HeadLocalRotation;

        public InputDataController Left;
        public InputDataController Right;

        public InputDataEmoji emojiData;

        public void ResetEmojiData()
        {
            emojiData.emojiIndex = -1;
            emojiData.OnEmojiSelected = false;
        }
    }
}