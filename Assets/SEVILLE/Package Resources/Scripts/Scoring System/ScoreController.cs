
using UnityEngine;
using TMPro;
using UnityEngine.Events;

namespace Seville
{
    public class ScoreController : MonoBehaviour
    {
        public DataManager dataManager;
        public TextMeshProUGUI textScore;

        public UnityEvent OnScoreFinished;

        private void Update()
        {
            if (dataManager)
                textScore.text = dataManager.playerScore.ToString();
        }

        public void IncreaseScore(int val)
        {
            if (IsScoreFinished())
            {
                Invoke(nameof(FinishScore), .2f);
                return;
            }

            dataManager.IncreaseScore(val);
        }

        private void FinishScore() => OnScoreFinished.Invoke();

        public void DecreaseScore(int val)
        {
            dataManager.playerScore -= val;
        }

        public void ResetScore()
        {
            dataManager.playerScore = 0;
        }

        public bool IsScoreFinished()
        {
            bool state = false;

            if (dataManager.playerScore >= dataManager.maxScore)
            {
                state = true;
            }
            else
            {
                state = false;
            }

            return state;
        }
    }
}