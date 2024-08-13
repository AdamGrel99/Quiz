using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizApp.Models
{
    public class QuizCollection
    {
        public List<Quiz> Quizzes { get; set; } = new List<Quiz>();

        public int NextIdQuiz()
        {
            int nextId = 1;
            foreach (var quiz in Quizzes)
            {
                if (quiz.Id != nextId)
                {
                    return nextId;
                }
                nextId++;
            }
            return nextId;
        }

        public int NextIdQuestion()
        {
            HashSet<int> existingIds = new HashSet<int>();

            foreach (var quiz in Quizzes)
            {
                if (quiz.Questions == null)
                {
                    return 1;
                }

                foreach (var question in quiz.Questions)
                {
                    existingIds.Add(question.Id);
                }
            }

            int nextId = 1;
            while (existingIds.Contains(nextId))
            {
                nextId++;
            }

            return nextId;
        }

        public int NextIdAnswer()
        {
            // Zbierz wszystkie istniejące identyfikatory odpowiedzi
            var existingIds = new HashSet<int>();
            foreach (var quiz in Quizzes)
            {
                if(quiz.Questions == null)
                {
                    return 1;
                }

                foreach (var question in quiz.Questions)
                {
                    foreach (var answer in question.Answers)
                    {
                        existingIds.Add(answer.Id);
                    }
                }
            }

            // Znajdź najmniejszy brakujący identyfikator
            int nextId = 1;
            while (existingIds.Contains(nextId))
            {
                nextId++;
            }

            return nextId;
        }

        public Quiz SelectQuizByName(string quizName)
        {
            return Quizzes.FirstOrDefault(q => q.Name.Equals(quizName, StringComparison.OrdinalIgnoreCase));
        }

        public bool QuizNameExists(string quizName)
        {
            return Quizzes.Any(q => q.Name.Equals(quizName, StringComparison.OrdinalIgnoreCase));
        }

        public void DeleteQuizByName(string quizName)
        {
            var quizToRemove = Quizzes.FirstOrDefault(q => q.Name.Equals(quizName, StringComparison.OrdinalIgnoreCase));
            if (quizToRemove != null)
            {
                Quizzes.Remove(quizToRemove);
            }
        }

        public void AddQuestionToQuizByName(string quizName, Question question)
        {
            var quiz = Quizzes.FirstOrDefault(q => q.Name.Equals(quizName, StringComparison.OrdinalIgnoreCase));
            if (quiz != null)
            {
                if (quiz.Questions == null)
                {
                    quiz.Questions = new List<Question>();
                }
                quiz.Questions.Add(question);
            }
        }

        public void DeleteQuestionInQuizByName(string quizName, int questionId)
        {
            var quiz = Quizzes.FirstOrDefault(q => q.Name.Equals(quizName, StringComparison.OrdinalIgnoreCase));
            if (quiz != null)
            {
                var questionToRemove = quiz.Questions.FirstOrDefault(q => q.Id == questionId);
                if (questionToRemove != null)
                {
                    quiz.Questions.Remove(questionToRemove);
                }
            }
        }

        public void InsertQuestionInQuizByName(string quizName, Question question, int questionId)
        {
            var quiz = Quizzes.FirstOrDefault(q => q.Name.Equals(quizName, StringComparison.OrdinalIgnoreCase));
            if (quiz != null)
            {
                var questionToModify = quiz.Questions.FirstOrDefault(q => q.Id == questionId);
                if (questionToModify != null)
                {
                    questionToModify.Text = question.Text;
                    questionToModify.Answers = question.Answers;
                }
            }
        }

        public void displayQuizzes()
        {
            if (Quizzes != null)
            {
                foreach (var el in Quizzes)
                {
                    Console.WriteLine($"Quiz: {el.Name}");
                    foreach (var question in el.Questions)
                    {
                        Console.WriteLine($"  Question: {question.Text}");
                        foreach (var answer in question.Answers)
                        {
                            Console.WriteLine($"    Answer: {answer.Text} (Correct: {answer.IsCorrect})");
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("Quiz not found.");
            }
        }
    }
}
