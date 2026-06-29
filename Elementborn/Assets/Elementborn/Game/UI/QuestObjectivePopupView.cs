using UnityEngine;
using UnityEngine.UI;
namespace Elementborn.Game
{
    public sealed class QuestObjectivePopupView : MonoBehaviour
    {
        [SerializeField] private GameObject root; [SerializeField] private Text titleText; [SerializeField] private Text bodyText; [SerializeField] private float visibleSeconds = 2.5f; private float hideAt;
        private void OnEnable(){ QuestUiEventHub.QuestStarted += QStart; QuestUiEventHub.ObjectiveCompleted += Obj; QuestUiEventHub.QuestCompleted += QDone; }
        private void OnDisable(){ QuestUiEventHub.QuestStarted -= QStart; QuestUiEventHub.ObjectiveCompleted -= Obj; QuestUiEventHub.QuestCompleted -= QDone; }
        private void Update(){ if(root!=null && root.activeSelf && Time.unscaledTime>=hideAt) root.SetActive(false); }
        public void Show(string title,string body){ if(root!=null) root.SetActive(true); if(titleText!=null) titleText.text=title; if(bodyText!=null) bodyText.text=body; hideAt=Time.unscaledTime+Mathf.Max(0.25f,visibleSeconds); }
        private void QStart(QuestUiRecord q)=>Show("Quest Started", q!=null?q.Title:""); private void Obj(QuestUiRecord q,QuestObjectiveUiRecord o)=>Show("Objective Complete", o!=null?o.Title:""); private void QDone(QuestUiRecord q)=>Show("Quest Complete", q!=null?q.Title:"");
    }
}
