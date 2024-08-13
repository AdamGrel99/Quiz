using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using QuizApp.Adapter;
using QuizApp.Models;
using QuizApp.ViewModel.BaseClass;
using QuizApp.Views;

namespace QuizApp.ViewModel
{
    public class QuizGenerator : INotifyPropertyChanged
    {
        private Quiz _selectedQuiz;
        private Question _selectedQuestion;
        private QuizCollection _quizCollection;
        private List<Quiz> _quizList;
        private List<Question> _questionList;
        private Repository.Repository _repository = new Repository.Repository();

        // Zmienne do aktualizacji bazy danych
        private List<(Quiz, Operation)> _modifiedQuizzes = new List<(Quiz, Operation)>();
        private List<(Question, Operation)> _modifiedQuestions = new List<(Question, Operation)>();
        private CollectionToDatabaseAdapter _adapterToDatabase = new CollectionToDatabaseAdapter();
        
        private string _quizName;
        private string _questionText;
        private string _answer1Text;
        private string _answer2Text;
        private string _answer3Text;
        private string _answer4Text;
        private bool _isAnswer1Correct;
        private bool _isAnswer2Correct;
        private bool _isAnswer3Correct;
        private bool _isAnswer4Correct;

        // Zmienne sprawdzające stan aplikacji
        private bool _isLoadQuizButtonEnabled;
        private bool _isCreateQuizButtonEnabled;
        private bool _isDeleteQuizButtonEnabled;
        private bool _isSaveQuizButtonEnabled;
        private bool _isAddQuestionButtonEnabled;
        private bool _isModifyQuestionButtonEnabled;
        private bool _isRemoveQuestionButtonEnabled;

        public event PropertyChangedEventHandler PropertyChanged;

        public QuizGenerator()
        {  
            _quizCollection = _repository.LoadAllQuizzes();

            IsLoadQuizButtonEnabled = true;
            IsDeleteQuizButtonEnabled= false;
            IsCreateQuizButtonEnabled = false;
            IsSaveQuizButtonEnabled = false;
            IsAddQuestionButtonEnabled = false;
            IsRemoveQuestionButtonEnabled = false;
            IsModifyQuestionButtonEnabled = false;

            AddQuestionCommand = new RelayCommand(AddQuestion);
            RemoveQuestionCommand = new RelayCommand(RemoveQuestion);
            ModifyQuestionCommand = new RelayCommand(ModifyQuestion);
            SaveQuizCommand = new RelayCommand(SaveQuiz);
            LoadQuizCommand = new RelayCommand(LoadQuiz);
            CreateQuizCommand = new RelayCommand(CreateQuiz);
            BackToMenuCommand = new RelayCommand(BackToMenu);
            DeleteQuizCommand = new RelayCommand(DeleteQuiz);
            ClearAnsAndQueCommand = new RelayCommand(ClearAnsQue);
        }

        public bool IsLoadQuizButtonEnabled
        {
            get => _isLoadQuizButtonEnabled;
            set
            {
                _isLoadQuizButtonEnabled = value;
                OnPropertyChanged(nameof(IsLoadQuizButtonEnabled));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public bool IsCreateQuizButtonEnabled
        {
            get => _isCreateQuizButtonEnabled;
            set
            {
                _isCreateQuizButtonEnabled = value;
                OnPropertyChanged(nameof(IsCreateQuizButtonEnabled));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public bool IsDeleteQuizButtonEnabled
        {
            get => _isDeleteQuizButtonEnabled;
            set
            {
                _isDeleteQuizButtonEnabled = value;
                OnPropertyChanged(nameof(IsDeleteQuizButtonEnabled));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public bool IsSaveQuizButtonEnabled
        {
            get => _isSaveQuizButtonEnabled;
            set
            {
                _isSaveQuizButtonEnabled = value;
                OnPropertyChanged(nameof(IsSaveQuizButtonEnabled));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public bool IsAddQuestionButtonEnabled
        {
            get => _isAddQuestionButtonEnabled;
            set
            {
                _isAddQuestionButtonEnabled = value;
                OnPropertyChanged(nameof(IsAddQuestionButtonEnabled));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public bool IsRemoveQuestionButtonEnabled
        {
            get => _isRemoveQuestionButtonEnabled;
            set
            {
                _isRemoveQuestionButtonEnabled = value;
                OnPropertyChanged(nameof(IsRemoveQuestionButtonEnabled));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public bool IsModifyQuestionButtonEnabled
        {
            get => _isModifyQuestionButtonEnabled;
            set
            {
                _isModifyQuestionButtonEnabled = value;
                OnPropertyChanged(nameof(IsModifyQuestionButtonEnabled));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string QuizName
        {
            get => _quizName;
            set
            {
                _quizName = value;
                OnPropertyChanged(nameof(QuizName));
            }
        }

        public string QuestionText
        {
            get => _questionText;
            set
            {
                _questionText = value;
                OnPropertyChanged(nameof(QuestionText));
            }
        }

        public Quiz SelectedQuiz
        {
            get => _selectedQuiz;
            set
            {
                _selectedQuiz = value;
                OnPropertyChanged(nameof(SelectedQuiz));

                if (SelectedQuiz != null)
                {
                    // wyśwetlamy wszystkie pytania dla danego quizzu
                    _questionList = _selectedQuiz.Questions;
                    OnPropertyChanged(nameof(Questions));

                    // ustawiamy w jakim Quizie możemy modyfikować
                    _quizName = _selectedQuiz.Name;
                    OnPropertyChanged(nameof(QuizName));
                }  
            }
        }

        public Question SelectedQuestion
        {
            get => _selectedQuestion;
            set
            {
                _selectedQuestion = value;
                OnPropertyChanged(nameof(SelectedQuestion));

                if (SelectedQuestion != null)
                {
                    _questionText = _selectedQuestion.Text;
                    OnPropertyChanged(nameof(QuestionText));
                    LoadAnswers();
                }       
            }
        }

        public List<Quiz> Quizzes
        {
            get => _quizList;
            set
            {
                _quizList = value;
                OnPropertyChanged(nameof(Quizzes));
            }
        }

        public List<Question> Questions
        {
            get => _questionList;
            set
            {
                _questionList = value;
                OnPropertyChanged(nameof(Questions));
            }
        }

        public string Answer1Text
        {
            get => _answer1Text;
            set
            {
                _answer1Text = value;
                OnPropertyChanged(Answer1Text);
            }
        }

        public string Answer2Text
        {
            get => _answer2Text;
            set
            {
                _answer2Text = value;
                OnPropertyChanged(Answer2Text);
            }
        }

        public string Answer3Text
        {
            get => _answer3Text;
            set
            {
                _answer3Text = value;
                OnPropertyChanged(Answer3Text);
            }
        }

        public string Answer4Text
        {
            get => _answer4Text;
            set
            {
                _answer4Text = value;
                OnPropertyChanged(Answer4Text);
            }
        }

        public bool IsAnswer1Correct
        {
            get => _isAnswer1Correct;
            set
            {
                _isAnswer1Correct = value;
                OnPropertyChanged(nameof(IsAnswer1Correct));
            }
        }

        public bool IsAnswer2Correct
        {
            get => _isAnswer2Correct;
            set
            {
                _isAnswer2Correct = value;
                OnPropertyChanged(nameof(IsAnswer2Correct));
            }
        }

        public bool IsAnswer3Correct
        {
            get => _isAnswer3Correct;
            set
            {
                _isAnswer3Correct = value;
                OnPropertyChanged(nameof(IsAnswer3Correct));
            }
        }

        public bool IsAnswer4Correct
        {
            get => _isAnswer4Correct;
            set
            {
                _isAnswer4Correct = value;
                OnPropertyChanged(nameof(IsAnswer4Correct));
            }
        }

        public ICommand AddQuestionCommand { get; }
        public ICommand RemoveQuestionCommand { get; }
        public ICommand ModifyQuestionCommand { get; }
        public ICommand SaveQuizCommand { get; }
        public ICommand LoadQuizCommand { get; }
        public ICommand CreateQuizCommand { get; }
        public ICommand BackToMenuCommand { get; }
        public ICommand DeleteQuizCommand { get; }
        public ICommand ClearAnsAndQueCommand { get; }

        private void AddQuestion(object parameter)
        {
            if(ValidateInputs(_quizName, _questionText, _answer1Text, _answer2Text, _answer3Text, _answer4Text) && 
                IsCheckedCorrectAnswer(_isAnswer1Correct, _isAnswer2Correct, _isAnswer3Correct, _isAnswer4Correct))
            {
                Quiz quiz = _quizCollection.SelectQuizByName(_quizName);

                List<Answer> newAnswerList = new List<Answer>
                {
                new Answer { Id = _quizCollection.NextIdAnswer(), Text =  _answer1Text, IsCorrect = _isAnswer1Correct },
                new Answer { Id = _quizCollection.NextIdAnswer() + 1, Text =  _answer2Text, IsCorrect = _isAnswer2Correct },
                new Answer { Id = _quizCollection.NextIdAnswer() + 2, Text =  _answer3Text, IsCorrect = _isAnswer3Correct },
                new Answer { Id = _quizCollection.NextIdAnswer() + 3, Text =  _answer4Text, IsCorrect = _isAnswer4Correct }
                };

                Question newQuestion = new Question
                {
                    Id = _quizCollection.NextIdQuestion(),
                    Text = _questionText,
                    Answers = newAnswerList,
                    IdQuiz = quiz.Id
                };

                _quizCollection.AddQuestionToQuizByName(_quizName, newQuestion);
                _modifiedQuestions.Add((newQuestion, Operation.Add));
                UpdateQuestionsList();
                IsSaveQuizButtonEnabled = true;
            }
            else if (!ValidateInputs(_quizName, _questionText, _answer1Text, _answer2Text, _answer3Text, _answer4Text))
            {
                MessageBox.Show("Jakiś element pytania jest pusty. Uzupełnij go!");
            }
            else
            {
                MessageBox.Show("Przynajmniej jedna odpowiedź musi być poprawna!");
            }
        }

        private void RemoveQuestion(object parameter)
        {
            if(_selectedQuestion != null)
            {
                _modifiedQuestions.Add((_selectedQuestion, Operation.Delete));
                _quizCollection.DeleteQuestionInQuizByName(_quizName, _selectedQuestion.Id);
                UpdateQuestionsList();
                IsSaveQuizButtonEnabled = true;
            }
        }

        private void ModifyQuestion(object parameter)
        {
            if (ValidateInputs(_quizName, _questionText, _answer1Text, _answer2Text, _answer3Text, _answer4Text) &&
                IsCheckedCorrectAnswer(_isAnswer1Correct, _isAnswer2Correct, _isAnswer3Correct, _isAnswer4Correct))
            {
                Quiz quiz = _quizCollection.SelectQuizByName(_quizName);

                List<Answer> newAnswerList = new List<Answer>
                {
                    new Answer { Id = _selectedQuestion.Answers[0].Id, Text =  _answer1Text, IsCorrect = _isAnswer1Correct },
                    new Answer { Id = _selectedQuestion.Answers[1].Id, Text =  _answer2Text, IsCorrect = _isAnswer2Correct },
                    new Answer { Id = _selectedQuestion.Answers[2].Id, Text =  _answer3Text, IsCorrect = _isAnswer3Correct },
                    new Answer { Id = _selectedQuestion.Answers[3].Id, Text =  _answer4Text, IsCorrect = _isAnswer4Correct }
                };

                Question newQuestion = new Question
                {
                    Id = _selectedQuestion.Id,
                    Text = _questionText,
                    Answers = newAnswerList,
                    IdQuiz = quiz.Id
                };

                _quizCollection.InsertQuestionInQuizByName(_quizName, newQuestion, _selectedQuestion.Id);
                _modifiedQuestions.Add((newQuestion, Operation.Modify));
                UpdateQuestionsList();
                IsSaveQuizButtonEnabled = true;
            }
            else if (!ValidateInputs(_quizName, _questionText, _answer1Text, _answer2Text, _answer3Text, _answer4Text))
            {
                MessageBox.Show("Jakiś element pytania jest pusty. Uzupełnij go!");
            }
            else
            {
                MessageBox.Show("Przynajmniej jedna odpowiedź musi być poprawna!");
            }
            
        }

        private void SaveQuiz(object parameter)
        {        
            if (_modifiedQuestions.Count > 0)
            {
                _adapterToDatabase.GetOperationInDatabase(_modifiedQuestions);
                IsSaveQuizButtonEnabled = false;
            }

            if (_modifiedQuizzes.Count > 0) 
            { 
                _adapterToDatabase.GetOperationInDatabase(_modifiedQuizzes, _quizCollection);
                IsSaveQuizButtonEnabled = false;
            }
        }

        private void ClearAnsQue(object parameter)
        {
            QuestionText = string.Empty;
            ClearAnswers();
            SelectedQuestion = null;
        }

        private void DeleteQuiz(object parameter)
        {
            if (SelectedQuiz != null)
            {
                _modifiedQuizzes.Add((_quizCollection.SelectQuizByName(SelectedQuiz.Name), Operation.Delete));
                _quizCollection.DeleteQuizByName(SelectedQuiz.Name);

                Quizzes = new List<Quiz>(_quizCollection.Quizzes);
                SelectedQuiz = new Quiz();
                ClearAnswers();
                IsSaveQuizButtonEnabled = true;
            }
        }

        private void LoadQuiz(object parameter)
        {
            IsLoadQuizButtonEnabled = false;
            IsDeleteQuizButtonEnabled = true;
            IsCreateQuizButtonEnabled = true;
            IsSaveQuizButtonEnabled = false;
            IsAddQuestionButtonEnabled = true;
            IsRemoveQuestionButtonEnabled = true;
            IsModifyQuestionButtonEnabled = true;

            Quizzes = new List<Quiz>(_quizCollection.Quizzes);
        }

        private void CreateQuiz(object parameter)
        {
            _quizName = Microsoft.VisualBasic.Interaction.InputBox("Podaj nazwę nowego quizu", "Stwórz Quiz", "Nowy Quiz");

            if (string.IsNullOrEmpty(_quizName))
            {
                MessageBox.Show("Nazwa quizu nie może być pusta.");
            }
            else if (_quizCollection.QuizNameExists(_quizName))
            {
                MessageBox.Show("Quiz o tej nazwie już istnieje.");
            }
            else
            {
                Quiz newQuiz = new Quiz();
                newQuiz.Name = _quizName;
                newQuiz.Id = _quizCollection.NextIdQuiz();

                _quizCollection.Quizzes.Add(newQuiz);
                _modifiedQuizzes.Add((newQuiz, Operation.Add));
                OnPropertyChanged(nameof(QuizName));
                LoadQuiz(newQuiz);

                // ustawia nowy quiz jako ten wybraby
                _selectedQuiz = newQuiz;
                OnPropertyChanged(nameof(SelectedQuiz));

                // czyści widok dla Listy pytań
                Questions = new List<Question>();

                // czyśći pytania i odpowiedzi
                ClearAnsQue(null);

                // Ustawia button Save na aktywny
                IsSaveQuizButtonEnabled = true;
            }  
        }

        private void BackToMenu(object parameter)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.MainFrame.Navigate(new MainMenu());
            }
        }

        private void LoadAnswers()
        {
            if (SelectedQuestion != null && SelectedQuestion.Answers.Count >= 4)
            {
                _answer1Text = _selectedQuestion.Answers[0].Text;
                OnPropertyChanged(nameof(Answer1Text));
                _isAnswer1Correct = SelectedQuestion.Answers[0].IsCorrect;
                OnPropertyChanged(nameof(IsAnswer1Correct));

                _answer2Text = _selectedQuestion.Answers[1].Text;
                OnPropertyChanged(nameof(Answer2Text));
                _isAnswer2Correct = SelectedQuestion.Answers[1].IsCorrect;
                OnPropertyChanged(nameof(IsAnswer2Correct));

                _answer3Text = _selectedQuestion.Answers[2].Text;
                OnPropertyChanged(nameof(Answer3Text));
                _isAnswer3Correct = SelectedQuestion.Answers[2].IsCorrect;
                OnPropertyChanged(nameof(IsAnswer3Correct));

                _answer4Text = _selectedQuestion.Answers[3].Text;
                OnPropertyChanged(nameof(Answer4Text));
                _isAnswer4Correct = SelectedQuestion.Answers[3].IsCorrect;
                OnPropertyChanged(nameof(IsAnswer4Correct));
            }
        }

        private void ClearAnswers()
        {
            Answer1Text = string.Empty;
            Answer2Text = string.Empty;
            Answer3Text = string.Empty;
            Answer4Text = string.Empty;
            IsAnswer1Correct = false;
            IsAnswer2Correct = false;
            IsAnswer3Correct = false;
            IsAnswer4Correct = false;
            QuestionText = string.Empty;
        }

        private void UpdateQuestionsList()
        {
            // Update zmian dla wybranego quizu            
            SelectedQuiz = _quizCollection.SelectQuizByName(_quizName);

            // Czyścimy listę pytań w widoku
            Questions = new List<Question>();

            // Aktualizujemy listę pytań w widoku
            Questions = _selectedQuiz.Questions;

            ClearAnsQue(null);
        }

        private bool ValidateInputs(string quizName, string questionText, string answer1Text, string answer2Text, string answer3Text, string answer4Text)
        {
            return !string.IsNullOrWhiteSpace(quizName) &&
           !string.IsNullOrWhiteSpace(questionText) &&
           !string.IsNullOrWhiteSpace(answer1Text) &&
           !string.IsNullOrWhiteSpace(answer2Text) &&
           !string.IsNullOrWhiteSpace(answer3Text) &&
           !string.IsNullOrWhiteSpace(answer4Text);
        }

        private bool IsCheckedCorrectAnswer(bool isAnswer1Correct, bool isAnswer2Correct, bool isAnswer3Correct, bool isAnswer4Correct)
        {
            return isAnswer1Correct || isAnswer2Correct || isAnswer3Correct || isAnswer4Correct;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
