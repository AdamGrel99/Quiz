using QuizApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizApp.Adapter
{
    public class CollectionToDatabaseAdapter
    {
        private Repository.Repository _repository = new Repository.Repository();

        public void GetOperationInDatabase(List<(Question, Operation)> modifiedQuestion)
        {
            foreach (var item in modifiedQuestion)
            {
                if (item.Item2 == Operation.Delete)
                {
                    _repository.DeleteQuestionById(item.Item1.Id);
                }
                else if (item.Item2 == Operation.Add)
                {
                    _repository.InsertToTable("questions", new Dictionary<string, string> 
                    {
                        { "id", $"{item.Item1.Id}" },
                        { "question", item.Item1.Text }, 
                        { "quiz_id", $"{item.Item1.IdQuiz}"}
                    });
                    foreach(var answer in item.Item1.Answers)
                    {
                        _repository.InsertToTable("answers", new Dictionary<string, string> 
                        {
                            { "id", $"{answer.Id}" },
                            { "answer", $"{answer.Text}" }, 
                            { "question_id", $"{item.Item1.Id}" }, 
                            { "isCorrected", answer.IsCorrect ? "Tak" : "Nie" } 
                        });
                    }
                }
                else if(item.Item2 == Operation.Modify)
                {                    
                    _repository.DeleteQuestionById(item.Item1.Id);

                    _repository.InsertToTable("questions", new Dictionary<string, string>
                    {
                        { "id", $"{item.Item1.Id}" },
                        { "question", item.Item1.Text },
                        { "quiz_id", $"{item.Item1.IdQuiz}"}
                    });
                    foreach (var answer in item.Item1.Answers)
                    {
                        _repository.InsertToTable("answers", new Dictionary<string, string>
                        {
                            { "id", $"{answer.Id}" },
                            { "answer", $"{answer.Text}" },
                            { "question_id", $"{item.Item1.Id}" },
                            { "isCorrected", answer.IsCorrect ? "Tak" : "Nie" }
                        });
                    }
                }
            }
        }

        public void GetOperationInDatabase(List<(Quiz, Operation)> modifiedQuizzes, QuizCollection quizCollection)
        {
            foreach (var item in modifiedQuizzes)
            {
                if (item.Item2 == Operation.Delete)
                {
                    if(item.Item1.Id != null)
                    {
                        _repository.DeleteQuizById(item.Item1.Id);
                    }         
                }
                else if (item.Item2 == Operation.Add)
                {
                    _repository.InsertToTable("quiz", new Dictionary<string, string> 
                    { 
                        { "name", item.Item1.Name } 
                    });
                }
            }
        }
    }
}
