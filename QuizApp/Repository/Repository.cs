using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuizApp.Models;
using System.IO;

namespace QuizApp.Repository
{
    public class Repository
    {
        private static string _dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "quizapp.db");
        private readonly string _connectionString = $"Data Source={_dbPath};";

        public void InitializeDatabase()
        {
            CreateDataBase();
        }

        public void CreateDataBase()
        {
            if (!File.Exists(_dbPath))
            {
                SQLiteConnection.CreateFile(_dbPath);
            }

            using (var connection = new SQLiteConnection(this._connectionString))
            {
                connection.Open();

                string sql = "CREATE TABLE IF NOT EXISTS quiz (id INTEGER PRIMARY KEY, name TEXT)";
                SqlRequest(sql, connection);

                sql = "CREATE TABLE IF NOT EXISTS questions (" +
                    "id INTEGER PRIMARY KEY AUTOINCREMENT, " +
                    "question TEXT, " +
                    "quiz_id INTEGER, " +
                    "FOREIGN KEY (quiz_id) REFERENCES quiz(id))";
                SqlRequest(sql, connection);

                sql = "CREATE TABLE IF NOT EXISTS answers (" +
                 "id INTEGER PRIMARY KEY AUTOINCREMENT, " +
                 "answer TEXT, " +
                 "question_id INTEGER, " +
                 "isCorrected TEXT CHECK(isCorrected IN ('Tak', 'Nie')), " +
                 "FOREIGN KEY (question_id) REFERENCES questions(id))";
                SqlRequest(sql, connection);

                connection.Close();
            }
        }

        // przydatna funkcja do dodawania rekordów do bazy danych
        public void InsertToTable(string tableName, Dictionary<string, string> columns)
        {
            using (var connection = new SQLiteConnection(this._connectionString))
            {
                connection.Open();

                // Wstawienia wartości do bazy danych
                string columnsPart = string.Join(", ", columns.Keys);
                string valuesPart = string.Join(", ", columns.Values.Select(value => $"'{value}'"));
                string sql = $"INSERT INTO {tableName} ({columnsPart}) VALUES ({valuesPart})";
                SqlRequest(sql, connection);

                connection.Close();
            }
        }

        public void DeleteQuizById(int id)
        {
            using (var connection = new SQLiteConnection(this._connectionString))
            {
                connection.Open();

                // Usuń powiązane odpowiedzi
                string sql = "DELETE FROM answers WHERE question_id IN (SELECT id FROM questions WHERE quiz_id = @quizId)";
                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@quizId", id);
                    command.ExecuteNonQuery();
                }

                // Usuń powiązane pytania
                sql = "DELETE FROM questions WHERE quiz_id = @quizId";
                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@quizId", id);
                    command.ExecuteNonQuery();
                }

                // Usuń quiz
                sql = "DELETE FROM quiz WHERE id = @id";
                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void DeleteQuestionById(int id)
        {
            using (var connection = new SQLiteConnection(this._connectionString))
            {
                connection.Open();

                // Usuń powiązane odpowiedzi
                string sql = "DELETE FROM answers WHERE question_id = @questionId";
                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@questionId", id);
                    command.ExecuteNonQuery();
                }

                // Usuń pytanie
                sql = "DELETE FROM questions WHERE id = @questionId";
                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@questionId", id);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void SqlRequest(string sql, SQLiteConnection connection)
        {
            try
            {
                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        
        public void DisplayElementsInTable(string tableName)
        {
            using (var connection = new SQLiteConnection(this._connectionString))
            {
                connection.Open();

                // Wyświetlanie zawartości tabeli
                string sql = $"SELECT * FROM {tableName}";
                using (var command = new SQLiteCommand(sql, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                Console.Write($"{reader.GetName(i)}: {reader[i]}, ");
                            }
                            Console.WriteLine();
                        }
                    }
                }

                connection.Close();
            }
        }

        // przydatna funkcja do wyswietlenia w widoku wybór Quizu 
        public List<string> LoadQuizzesName()
        {
            using (var connection = new SQLiteConnection(this._connectionString))
            {
                connection.Open();

                List<string> list = new List<string>();

                string sql = $"SELECT * FROM quiz";
                using (var command = new SQLiteCommand(sql, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(reader["name"].ToString());
                        }
                    }
                }

                connection.Close();
                return list;
            }
        }

        // przydatna funkcja po wyborze quizu do wczytania wszystkich pytań dla danego Quizu
        public Quiz LoadQuizByName(string quizName)
        {
            Quiz quiz = null;

            using (var connection = new SQLiteConnection(this._connectionString))
            {
                connection.Open();

                string sql = "SELECT id, name FROM quiz WHERE name = @quizName";
                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@quizName", quizName);

                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            quiz = new Quiz
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Questions = new List<Question>()
                            };
                        }
                    }
                }

                if (quiz != null)
                {
                    sql = "SELECT q.id, q.question, a.id, a.answer, a.isCorrected " +
                          "FROM questions q " +
                          "JOIN answers a ON q.id = a.question_id " +
                          "WHERE q.quiz_id = @quizId";
                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@quizId", quiz.Id);

                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            var questionMap = new Dictionary<int, Question>();

                            while (reader.Read())
                            {
                                int questionId = reader.GetInt32(0);
                                if (!questionMap.ContainsKey(questionId))
                                {
                                    questionMap[questionId] = new Question
                                    {
                                        Id = questionId,
                                        Text = reader.GetString(1),
                                        Answers = new List<Answer>()
                                    };
                                }

                                var question = questionMap[questionId];

                                var answer = new Answer
                                {
                                    Id = reader.GetInt32(2),
                                    Text = reader.GetString(3),
                                    IsCorrect = reader.GetString(4) == "Tak"
                                };

                                question.Answers.Add(answer);
                            }

                            quiz.Questions.AddRange(questionMap.Values);
                        }
                    }
                }

                connection.Close();
            }

            return quiz;
        }

        // przydatna funkcja do wczytania wszystkich elementów w bazie
        public QuizCollection LoadAllQuizzes()
        {
            QuizCollection quizCollection = new QuizCollection();

            using (var connection = new SQLiteConnection(this._connectionString))
            {
                connection.Open();

                string sql = "SELECT id, name FROM quiz";
                using (var command = new SQLiteCommand(sql, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var quiz = new Quiz
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Questions = new List<Question>()
                            };

                            quizCollection.Quizzes.Add(quiz);
                        }
                    }
                }

                foreach (var quiz in quizCollection.Quizzes)
                {
                    sql = "SELECT q.id, q.question, a.id, a.answer, a.isCorrected " +
                          "FROM questions q " +
                          "JOIN answers a ON q.id = a.question_id " +
                          "WHERE q.quiz_id = @quizId";
                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@quizId", quiz.Id);

                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            var questionMap = new Dictionary<int, Question>();

                            while (reader.Read())
                            {
                                int questionId = reader.GetInt32(0);
                                if (!questionMap.ContainsKey(questionId))
                                {
                                    questionMap[questionId] = new Question
                                    {
                                        Id = questionId,
                                        Text = reader.GetString(1),
                                        Answers = new List<Answer>()
                                    };
                                }

                                var question = questionMap[questionId];

                                var answer = new Answer
                                {
                                    Id = reader.GetInt32(2),
                                    Text = reader.GetString(3),
                                    IsCorrect = reader.GetString(4) == "Tak"
                                };

                                question.Answers.Add(answer);
                            }

                            quiz.Questions.AddRange(questionMap.Values);
                        }
                    }
                }

                connection.Close();
            }

            return quizCollection;
        }
    }
}
