using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizApp.Models
{
    public class Question
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public List<Answer> Answers { get; set; }
        public int IdQuiz { get; set; }

        public bool IsAnsweredCorrectly()
        {
            foreach (var answer in Answers)
            {
                if ((answer.IsSelected && !answer.IsCorrect) || (!answer.IsSelected && answer.IsCorrect))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
