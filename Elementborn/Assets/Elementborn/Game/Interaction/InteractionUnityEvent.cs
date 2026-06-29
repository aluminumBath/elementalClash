using System;
using UnityEngine;
using UnityEngine.Events;

namespace Elementborn.Game
{
    [Serializable]
    public sealed class GameObjectUnityEvent : UnityEvent<GameObject>
    {
    }
}
