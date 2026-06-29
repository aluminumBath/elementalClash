using System;

namespace Elementborn.Game
{
    [Serializable]
    public class NotificationRecord
    {
        public string Message = "";
        public NotificationType Type = NotificationType.Info;
        public float CreatedAtUnscaledTime;
        public float DurationSeconds = 4f;
    }
}
