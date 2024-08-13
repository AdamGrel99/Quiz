using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizApp.Models
{
    public class Quiz
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Question> Questions { get; set; }
        public void displayQuiz()
        {
            Console.WriteLine($"Quiz: {this.Name}");
            foreach (var question in this.Questions)
            {
                Console.WriteLine($"  Question: {question.Text}");
                foreach (var answer in question.Answers)
                {
                    Console.WriteLine($"    Answer: {answer.Text} (Correct: {answer.IsCorrect})");
                }
            }
        }
    }
}
