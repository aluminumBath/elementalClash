using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Elementborn.Game
{
    public sealed class NotificationFeed : MonoBehaviour
    {
        public static NotificationFeed Instance { get; private set; }

        [SerializeField] private int maxStored = 25;
        [SerializeField] private List<NotificationRecord> notifications = new List<NotificationRecord>();
        [SerializeField] private NotificationUnityEvent onNotification;

        public IReadOnlyList<NotificationRecord> Notifications => notifications;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            notifications.RemoveAll(n => n.CreatedAtUnscaledTime + n.DurationSeconds < Time.unscaledTime);
        }

        public static NotificationFeed Ensure()
        {
            if (Instance != null)
            {
                return Instance;
            }

            var go = new GameObject(nameof(NotificationFeed));
            return go.AddComponent<NotificationFeed>();
        }

        public static void Post(string message, NotificationType type = NotificationType.Info, float durationSeconds = 4f)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            var feed = Ensure();
            var record = new NotificationRecord
            {
                Message = message,
                Type = type,
                DurationSeconds = Mathf.Max(0.5f, durationSeconds),
                CreatedAtUnscaledTime = Time.unscaledTime
            };

            feed.notifications.Add(record);
            while (feed.notifications.Count > feed.maxStored)
            {
                feed.notifications.RemoveAt(0);
            }

            feed.onNotification?.Invoke(record);
            Debug.Log($"[{type}] {message}");
        }
    }

    [System.Serializable]
    public sealed class NotificationUnityEvent : UnityEvent<NotificationRecord>
    {
    }
}
