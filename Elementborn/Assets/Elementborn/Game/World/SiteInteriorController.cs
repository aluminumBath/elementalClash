using System.Reflection;
using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Lightweight compatibility controller for older site/interior scaffold scenes.
    /// It keeps site identity and simple enter/exit teleport points without depending on removed generator APIs.
    /// </summary>
    public sealed class SiteInteriorController : MonoBehaviour
    {
        public static SiteInteriorController Instance { get; private set; }

        [SerializeField] private string siteId = "";
        [SerializeField] private string displayName = "Site Interior";
        [TextArea]
        [SerializeField] private string summary = "";
        [SerializeField] private Transform entryPoint;
        [SerializeField] private Transform exitPoint;
        [SerializeField] private Vector3 fallbackExitPosition;

        public string SiteId => string.IsNullOrWhiteSpace(siteId) ? name : siteId;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? SiteId : displayName;
        public string Summary => summary ?? string.Empty;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                return;
            }

            Instance = this;
        }

        public static SiteInteriorController Ensure()
        {
            if (Instance != null)
            {
                return Instance;
            }

            var go = new GameObject(nameof(SiteInteriorController));
            return go.AddComponent<SiteInteriorController>();
        }

        public void Configure(string id, string title, string body)
        {
            siteId = id ?? siteId;
            displayName = string.IsNullOrWhiteSpace(title) ? displayName : title;
            summary = body ?? summary;
        }

        // v57 exact compatibility overload for WorldSpawnPlacer: Configure(SiteKind, string).
        public void Configure(Elementborn.Core.SiteKind kind, string idOrTitle)
        {
            siteId = string.IsNullOrWhiteSpace(idOrTitle) ? kind.ToString() : idOrTitle;
            displayName = string.IsNullOrWhiteSpace(idOrTitle) ? kind.ToString() : idOrTitle;
            if (string.IsNullOrWhiteSpace(summary))
            {
                summary = kind.ToString();
            }
        }

        // v57 broad compatibility overload for older local world setup scripts.
        public void Configure(object kindOrDefinition, string idOrTitle)
        {
            ApplySiteLikeObject(kindOrDefinition);

            if (!string.IsNullOrWhiteSpace(idOrTitle))
            {
                siteId = idOrTitle;
                if (string.IsNullOrWhiteSpace(displayName) || displayName == "Site Interior")
                {
                    displayName = idOrTitle;
                }
            }

            if (string.IsNullOrWhiteSpace(summary) && kindOrDefinition != null)
            {
                summary = kindOrDefinition.ToString();
            }
        }

        public void Configure(string id, string title, string body, Vector3 exitPosition)
        {
            Configure(id, title, body);
            fallbackExitPosition = exitPosition;
        }

        public void Configure(CapitalId capital, string title, string body, Vector3 exitPosition)
        {
            Configure(capital.ToString(), title, body, exitPosition);
        }

        public void Configure(MapMarkerType markerType, Vector3 worldPosition, string title, string body)
        {
            Configure(markerType.ToString(), title, body, worldPosition);
        }

        // v56 compatibility overload for older WorldSpawnPlacer scripts.
        public void Configure(object siteDefinition, Vector3 exitPosition)
        {
            ApplySiteLikeObject(siteDefinition);
            fallbackExitPosition = exitPosition;
        }

        // v56 compatibility overload for older WorldSpawnPlacer scripts.
        public void Configure(object siteDefinition, Transform exit)
        {
            ApplySiteLikeObject(siteDefinition);
            exitPoint = exit;
        }

        public void SetEntryPoint(Transform value)
        {
            entryPoint = value;
        }

        public void SetExitPoint(Transform value)
        {
            exitPoint = value;
        }

        public void Enter(GameObject actor)
        {
            MoveActor(actor, entryPoint != null ? entryPoint.position : transform.position);
        }

        public void Exit(GameObject actor)
        {
            Vector3 destination = exitPoint != null ? exitPoint.position : fallbackExitPosition;
            MoveActor(actor, destination);
        }

        // v55 compatibility overloads for older SiteEntrance/SiteExit scripts.
        public void Enter()
        {
            Enter((GameObject)null);
        }

        public void Enter(object request)
        {
            ApplySiteLikeObject(request);
            Enter((GameObject)null);
        }

        public void Enter(string id)
        {
            siteId = id ?? siteId;
            Enter((GameObject)null);
        }

        public void Enter(string id, GameObject actor)
        {
            siteId = id ?? siteId;
            Enter(actor);
        }

        public void Enter(object request, Vector3 fallbackExit)
        {
            ApplySiteLikeObject(request);
            fallbackExitPosition = fallbackExit;
            Enter((GameObject)null);
        }

        public void Enter(string id, Vector3 fallbackExit)
        {
            siteId = id ?? siteId;
            fallbackExitPosition = fallbackExit;
            Enter((GameObject)null);
        }

        public void Exit()
        {
            Exit((GameObject)null);
        }

        public void Exit(object request)
        {
            ApplySiteLikeObject(request);
            Exit((GameObject)null);
        }

        public void Exit(string id)
        {
            siteId = id ?? siteId;
            Exit((GameObject)null);
        }

        public void Exit(string id, GameObject actor)
        {
            siteId = id ?? siteId;
            Exit(actor);
        }

        public void Exit(object request, Vector3 fallbackExit)
        {
            ApplySiteLikeObject(request);
            fallbackExitPosition = fallbackExit;
            Exit((GameObject)null);
        }

        public void EnterSite(string id)
        {
            Enter(id);
        }

        public void ExitSite(string id)
        {
            Exit(id);
        }

        private void ApplySiteLikeObject(object value)
        {
            if (value == null)
            {
                return;
            }

            string id = ReadStringMember(value, "SiteId")
                ?? ReadStringMember(value, "Id")
                ?? ReadStringMember(value, "Key")
                ?? ReadStringMember(value, "Name");
            string title = ReadStringMember(value, "DisplayName")
                ?? ReadStringMember(value, "Title")
                ?? ReadStringMember(value, "Name");
            string body = ReadStringMember(value, "Summary")
                ?? ReadStringMember(value, "Description")
                ?? ReadStringMember(value, "Body");

            if (!string.IsNullOrWhiteSpace(id))
            {
                siteId = id;
            }

            if (!string.IsNullOrWhiteSpace(title))
            {
                displayName = title;
            }

            if (!string.IsNullOrWhiteSpace(body))
            {
                summary = body;
            }
        }

        private static string ReadStringMember(object value, string memberName)
        {
            if (value == null || string.IsNullOrWhiteSpace(memberName))
            {
                return null;
            }

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var type = value.GetType();

            var property = type.GetProperty(memberName, flags);
            if (property != null && property.PropertyType == typeof(string))
            {
                return property.GetValue(value) as string;
            }

            var field = type.GetField(memberName, flags);
            if (field != null && field.FieldType == typeof(string))
            {
                return field.GetValue(value) as string;
            }

            return null;
        }

        private void MoveActor(GameObject actor, Vector3 destination)
        {
            if (actor == null)
            {
                try
                {
                    actor = GameObject.FindGameObjectWithTag("Player");
                }
                catch
                {
                    actor = GameObject.Find("Player Test Rig");
                }
            }

            if (actor != null)
            {
                actor.transform.position = destination;
            }
        }
    }
}
