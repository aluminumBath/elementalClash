using System;
using System.Reflection;
using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Compatibility layer between the newer prompt-based interaction system and older
    /// scaffold interactables that only had Interact(), GetPrompt(), or TryGetInteraction(...).
    /// </summary>
    public static class InteractableCompatibility
    {
        public static bool CanInteract(IInteractable target, GameObject interactor)
        {
            if (target == null)
            {
                return false;
            }

            if (target is BaseInteractable baseInteractable)
            {
                return baseInteractable.CanInteract(interactor);
            }

            if (TryInvokeBool(target, "CanInteract", new object[] { interactor }, out bool withInteractor))
            {
                return withInteractor;
            }

            if (TryInvokeBool(target, "CanInteract", Array.Empty<object>(), out bool noArgs))
            {
                return noArgs;
            }

            if (TryGetLegacyInteraction(target, interactor, out _))
            {
                return true;
            }

            return !(target is Behaviour behaviour) || behaviour.isActiveAndEnabled;
        }

        public static InteractionPromptData GetPrompt(IInteractable target, GameObject interactor)
        {
            if (target == null)
            {
                return InteractionPromptData.Simple("Interact", "Interact");
            }

            if (target is BaseInteractable baseInteractable)
            {
                return baseInteractable.GetPrompt(interactor);
            }

            if (TryInvoke(target, "GetPrompt", new object[] { interactor }, out object promptObj))
            {
                if (promptObj is InteractionPromptData prompt)
                {
                    return prompt;
                }

                if (promptObj is string text && !string.IsNullOrWhiteSpace(text))
                {
                    return InteractionPromptData.Simple(text, "Interact");
                }
            }

            if (TryInvoke(target, "GetPrompt", Array.Empty<object>(), out object noArgPromptObj))
            {
                if (noArgPromptObj is InteractionPromptData prompt)
                {
                    return prompt;
                }

                if (noArgPromptObj is string text && !string.IsNullOrWhiteSpace(text))
                {
                    return InteractionPromptData.Simple(text, "Interact");
                }
            }

            if (TryGetLegacyInteraction(target, interactor, out object legacyInteraction))
            {
                string label = ExtractText(legacyInteraction, "Prompt", "Label", "Text", "Title", "Name");
                if (!string.IsNullOrWhiteSpace(label))
                {
                    return InteractionPromptData.Simple(label, "Interact");
                }
            }

            string fallback = target is Component component ? component.gameObject.name : target.GetType().Name;
            return InteractionPromptData.Simple(fallback, "Interact");
        }

        public static void Interact(IInteractable target, GameObject interactor)
        {
            if (target == null)
            {
                return;
            }

            if (target is BaseInteractable baseInteractable)
            {
                baseInteractable.Interact(interactor);
                return;
            }

            if (TryInvoke(target, "Interact", new object[] { interactor }, out _))
            {
                return;
            }

            if (TryInvoke(target, "Interact", Array.Empty<object>(), out _))
            {
                return;
            }

            if (TryInvoke(target, "Use", new object[] { interactor }, out _))
            {
                return;
            }

            if (TryInvoke(target, "Use", Array.Empty<object>(), out _))
            {
                return;
            }

            if (TryGetLegacyInteraction(target, interactor, out object legacyInteraction) && TryInvokeLegacyAction(legacyInteraction))
            {
                return;
            }

            Debug.Log($"Interactable {target.GetType().Name} has no compatible Interact/Use method.");
        }

        private static bool TryInvokeBool(object target, string methodName, object[] args, out bool value)
        {
            value = false;
            if (!TryInvoke(target, methodName, args, out object result) || !(result is bool boolValue))
            {
                return false;
            }

            value = boolValue;
            return true;
        }

        private static bool TryInvoke(object target, string methodName, object[] args, out object result)
        {
            result = null;
            if (target == null)
            {
                return false;
            }

            MethodInfo method = FindMethod(target.GetType(), methodName, args != null ? args.Length : 0);
            if (method == null)
            {
                return false;
            }

            try
            {
                result = method.Invoke(target, args);
                return true;
            }
            catch (TargetInvocationException ex)
            {
                Debug.LogException(ex.InnerException ?? ex);
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return false;
            }
        }

        private static MethodInfo FindMethod(Type type, string methodName, int parameterCount)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            foreach (MethodInfo method in type.GetMethods(flags))
            {
                if (method.Name == methodName && method.GetParameters().Length == parameterCount)
                {
                    return method;
                }
            }

            return null;
        }

        private static bool TryGetLegacyInteraction(object target, GameObject interactor, out object interaction)
        {
            interaction = null;
            if (target == null)
            {
                return false;
            }

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            foreach (MethodInfo method in target.GetType().GetMethods(flags))
            {
                if (method.Name != "TryGetInteraction")
                {
                    continue;
                }

                ParameterInfo[] parameters = method.GetParameters();
                if (parameters.Length != 2 || !parameters[1].ParameterType.IsByRef)
                {
                    continue;
                }

                Vector3 position = interactor != null ? interactor.transform.position : Vector3.zero;
                object[] args = { position, null };
                try
                {
                    object result = method.Invoke(target, args);
                    if (result is bool ok && ok)
                    {
                        interaction = args[1];
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    return false;
                }
            }

            return false;
        }

        private static bool TryInvokeLegacyAction(object interaction)
        {
            if (interaction == null)
            {
                return false;
            }

            foreach (string methodName in new[] { "Invoke", "Execute", "Run", "Interact", "Use" })
            {
                if (TryInvoke(interaction, methodName, Array.Empty<object>(), out _))
                {
                    return true;
                }
            }

            foreach (string memberName in new[] { "Action", "Callback", "OnInteract", "Handler" })
            {
                object value = ExtractMember(interaction, memberName);
                if (value is Delegate del)
                {
                    del.DynamicInvoke();
                    return true;
                }
            }

            return false;
        }

        private static string ExtractText(object value, params string[] names)
        {
            foreach (string name in names)
            {
                object member = ExtractMember(value, name);
                if (member is string text && !string.IsNullOrWhiteSpace(text))
                {
                    return text;
                }
            }

            return string.Empty;
        }

        private static object ExtractMember(object value, string name)
        {
            if (value == null)
            {
                return null;
            }

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            Type type = value.GetType();
            PropertyInfo prop = type.GetProperty(name, flags);
            if (prop != null)
            {
                return prop.GetValue(value);
            }

            FieldInfo field = type.GetField(name, flags);
            return field != null ? field.GetValue(value) : null;
        }
    }
}
