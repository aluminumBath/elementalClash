using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Safe site entrance bridge. This replaces older scaffold versions that depended on removed SiteInfo signatures.
    /// </summary>
    public sealed class SiteEntrance : MonoBehaviour
    {
        [SerializeField] private SiteInteriorController targetInterior;
        [SerializeField] private string siteId = "";
        [SerializeField] private string displayName = "Site";
        [TextArea]
        [SerializeField] private string summary = "";
        [SerializeField] private bool useThisPositionAsFallbackExit = true;
        [SerializeField] private bool enterOnPlayerTrigger = false;

        public void Configure(string id, string title, string body)
        {
            siteId = id ?? siteId;
            displayName = string.IsNullOrWhiteSpace(title) ? displayName : title;
            summary = body ?? summary;
        }

        public void Configure(string id, string title, string body, SiteInteriorController target)
        {
            Configure(id, title, body);
            targetInterior = target;
        }

        // v56 compatibility overload for older WorldSpawnPlacer scripts.
        public void Configure(string id, SiteInteriorController target)
        {
            siteId = id ?? siteId;
            targetInterior = target;
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
                if (string.IsNullOrWhiteSpace(displayName) || displayName == "Site")
                {
                    displayName = idOrTitle;
                }
            }

            if (string.IsNullOrWhiteSpace(summary) && kindOrDefinition != null)
            {
                summary = kindOrDefinition.ToString();
            }
        }

        // v56 compatibility overload for older WorldSpawnPlacer scripts that pass a site definition object.
        public void Configure(object siteDefinition, SiteInteriorController target)
        {
            ApplySiteLikeObject(siteDefinition);
            targetInterior = target;
        }

        // v56 compatibility overload for older generators that pass an exit position.
        public void Configure(object siteDefinition, Vector3 fallbackExit)
        {
            ApplySiteLikeObject(siteDefinition);
            useThisPositionAsFallbackExit = false;
            if (targetInterior == null)
            {
                targetInterior = SiteInteriorController.Ensure();
            }

            targetInterior.Configure(siteId, displayName, summary, fallbackExit);
        }

        public void Enter(GameObject actor)
        {
            var interior = targetInterior != null ? targetInterior : SiteInteriorController.Ensure();
            Vector3 fallbackExit = useThisPositionAsFallbackExit ? transform.position : Vector3.zero;
            interior.Configure(siteId, displayName, summary, fallbackExit);
            interior.Enter(actor);
        }

        public void Enter()
        {
            Enter((GameObject)null);
        }

        public void Interact(GameObject actor)
        {
            Enter(actor);
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

            const System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
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

        private void OnTriggerEnter(Collider other)
        {
            if (!enterOnPlayerTrigger || other == null)
            {
                return;
            }

            if (other.CompareTag("Player"))
            {
                Enter(other.gameObject);
            }
        }
    }
}
