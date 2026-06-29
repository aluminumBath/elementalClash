using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Safe site exit bridge for prototype interiors.
    /// </summary>
    public sealed class SiteExit : MonoBehaviour
    {
        [SerializeField] private SiteInteriorController targetInterior;
        [SerializeField] private string siteId = "";
        [SerializeField] private bool exitOnPlayerTrigger = false;

        public void Configure(string id, SiteInteriorController target)
        {
            siteId = id ?? siteId;
            targetInterior = target;
        }

        // v57 exact compatibility overload for WorldSpawnPlacer-style Configure(SiteKind, string).
        public void Configure(Elementborn.Core.SiteKind kind, string id)
        {
            siteId = string.IsNullOrWhiteSpace(id) ? kind.ToString() : id;
        }

        // v57 broad compatibility overload for older local world setup scripts.
        public void Configure(object kindOrDefinition, string id)
        {
            string extracted = ReadStringMember(kindOrDefinition, "SiteId")
                ?? ReadStringMember(kindOrDefinition, "Id")
                ?? ReadStringMember(kindOrDefinition, "Key")
                ?? ReadStringMember(kindOrDefinition, "Name");
            siteId = !string.IsNullOrWhiteSpace(id)
                ? id
                : (!string.IsNullOrWhiteSpace(extracted) ? extracted : siteId);
        }

        // v56 compatibility overload for older WorldSpawnPlacer scripts.
        public void Configure(object siteDefinition, SiteInteriorController target)
        {
            siteId = ReadStringMember(siteDefinition, "SiteId")
                ?? ReadStringMember(siteDefinition, "Id")
                ?? ReadStringMember(siteDefinition, "Key")
                ?? ReadStringMember(siteDefinition, "Name")
                ?? siteId;
            targetInterior = target;
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

        public void Exit(GameObject actor)
        {
            var interior = targetInterior != null ? targetInterior : SiteInteriorController.Ensure();
            if (!string.IsNullOrWhiteSpace(siteId))
            {
                interior.Exit(siteId, actor);
                return;
            }

            interior.Exit(actor);
        }

        public void Exit()
        {
            Exit((GameObject)null);
        }

        public void Interact(GameObject actor)
        {
            Exit(actor);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!exitOnPlayerTrigger || other == null)
            {
                return;
            }

            if (other.CompareTag("Player"))
            {
                Exit(other.gameObject);
            }
        }
    }
}
