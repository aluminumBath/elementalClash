using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Elementborn.Game
{
    /// <summary>
    /// Optional backend bridge for AI-driven NPC responses.
    /// Do NOT put API keys in Unity clients. Point this at your own server/backend.
    /// Expected backend contract can be customized, but this sends simple JSON:
    /// { npcName, role, personality, localKnowledge, playerText }
    /// </summary>
    public sealed class ExternalNpcResponseProviderStub : MonoBehaviour, INpcResponseProvider
    {
        [SerializeField] private string backendUrl = "http://localhost:3000/npc/respond";
        [SerializeField] private float timeoutSeconds = 10f;
        [SerializeField] private bool fallbackToRules = true;

        private RuleBasedNpcResponseProvider fallback;

        private void Awake()
        {
            fallback = GetComponent<RuleBasedNpcResponseProvider>();
            if (fallback == null)
            {
                fallback = gameObject.AddComponent<RuleBasedNpcResponseProvider>();
            }
        }

        public void GenerateResponse(NpcConversationRequest request, Action<NpcConversationResponse> onResponse)
        {
            if (string.IsNullOrWhiteSpace(backendUrl))
            {
                Fallback(request, onResponse);
                return;
            }

            StartCoroutine(Post(request, onResponse));
        }

        private IEnumerator Post(NpcConversationRequest request, Action<NpcConversationResponse> onResponse)
        {
            string payload = JsonUtility.ToJson(new BackendNpcRequest
            {
                npcName = request.Profile != null ? request.Profile.NpcName : "NPC",
                role = request.Profile != null ? request.Profile.Role : "",
                personality = request.Profile != null ? request.Profile.Personality : "",
                localKnowledge = request.Profile != null ? request.Profile.LocalKnowledge : "",
                playerText = request.PlayerText ?? ""
            });

            using var req = new UnityWebRequest(backendUrl, "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(payload);
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.timeout = Mathf.CeilToInt(timeoutSeconds);
            req.SetRequestHeader("Content-Type", "application/json");

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"NPC backend failed: {req.error}");
                Fallback(request, onResponse);
                yield break;
            }

            var response = JsonUtility.FromJson<BackendNpcResponse>(req.downloadHandler.text);
            if (response == null || string.IsNullOrWhiteSpace(response.text))
            {
                Fallback(request, onResponse);
                yield break;
            }

            onResponse?.Invoke(new NpcConversationResponse
            {
                Text = response.text,
                Intent = response.intent,
                Confidence = response.confidence
            });
        }

        private void Fallback(NpcConversationRequest request, Action<NpcConversationResponse> onResponse)
        {
            if (fallbackToRules && fallback != null)
            {
                fallback.GenerateResponse(request, onResponse);
            }
            else
            {
                onResponse?.Invoke(new NpcConversationResponse
                {
                    Text = "I need a moment to think.",
                    Intent = "backend_unavailable",
                    Confidence = 0f
                });
            }
        }

        [Serializable]
        private sealed class BackendNpcRequest
        {
            public string npcName;
            public string role;
            public string personality;
            public string localKnowledge;
            public string playerText;
        }

        [Serializable]
        private sealed class BackendNpcResponse
        {
            public string text;
            public string intent;
            public float confidence;
        }
    }
}
