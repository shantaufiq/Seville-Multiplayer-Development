using System.Collections.Generic;
using UnityEngine;

namespace Seville
{
    [CreateAssetMenu(fileName = "DataManager", menuName = "Custom/DataManager", order = 1)]
    public class DataManager : ScriptableObject
    {
        [System.Serializable]
        public class QuestItem
        {
            public Sprite iconSprite;
            public string title;
            [TextArea]
            public string description;
            public bool isDone;
            public string doneMessage;
            public int score;
        }

        public List<QuestItem> questList = new List<QuestItem>();
        public int playerScore = 0;
        public int maxScore;

        public void AddQuest(Sprite icon, string title, string description, bool isDone, string doneMessage, int score)
        {
            QuestItem newQuest = new QuestItem
            {
                iconSprite = icon,
                title = title,
                description = description,
                isDone = isDone,
                doneMessage = doneMessage,
                score = score
            };

            questList.Add(newQuest);
        }

        public void IncreaseScore(int amount)
        {
            playerScore += amount;
        }

        public void DecreaseScore(int val)
        {
            playerScore -= val;
        }

        public void ResetScore()
        {
            playerScore = 0;
        }

        public List<QuestItem> GetQuestData()
        {
            List<QuestItem> temp = new List<QuestItem>();

            foreach (var item in questList)
            {
                temp.Add(item);
            }

            return temp;
        }

        public void UpdateQuizItemDone(DataManager.QuestItem _item, int _itemId)
        {
            questList[_itemId] = _item;
        }
    }
}