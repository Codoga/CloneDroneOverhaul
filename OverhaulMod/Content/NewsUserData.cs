using System.Collections.Generic;

namespace OverhaulMod.Content
{
    public class NewsUserData
    {
        public List<string> AnsweredSurveys;

        public void FixValues()
        {
            if (AnsweredSurveys == null)
                AnsweredSurveys = new List<string>();
        }

        public bool HasAnswered(string title)
        {
            return AnsweredSurveys.Contains(title);
        }
    }
}
