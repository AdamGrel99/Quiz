using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using System.Windows.Navigation;
using QuizApp.Models;
using QuizApp.ViewModel.BaseClass;
using QuizApp.Views;
using System.Windows.Threading;

namespace QuizApp.ViewModel
{
    public class QuizSolver : INotifyPropertyChanged
    {
        private Quiz _currentQuiz;
        private Question _currentQuestion;
        private int _currentQuestionIndex;
        private string _selectedQuiz;
        private int _elapsedTime;
        private DispatcherTimer _timer;
        private int _score;
        private int _totalScore;
        private Repository.Repository _repository = new Repository.Repository();
        private List<string> _allQuizzesName;

        // Zmienne sprawdzające stan aplikacji
        private bool _isNextQuestionButtonEnabled;
        private bool _isQuizChoiceEnabled;
        private bool _isStartQuizButtonEnabled;
        private bool _isBackToMenuButtonEnabled;
        private bool _isEndQuizButtonEnabled;

        public event PropertyChangedEventHandler PropertyChanged;

        public QuizSolver()
        {
            IsQuizChoiceEnabled = true;
            IsBackToMenuButtonEnabled = true;
            IsStartQuizButtonEnabled = true;
            IsEndQuizButtonEnabled = false;
            IsNextQuestionButtonEnabled = false;

            NextQuestionCommand = new RelayCommand(NextQuestion, NextQuestionCanExecute);
            StartQuizCommand = new RelayCommand(StartQuiz, StartQuizCanExecute);
            BackToMenuCommand = new RelayCommand(BackToMenu, BackToMenuCanExecute);
            EndQuizCommand = new RelayCommand(EndQuiz, EndQuizCanExecute);
            
            _allQuizzesName = _repository.LoadQuizzesName();

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += TimerTick;
        }

        public bool IsNextQuestionButtonEnabled
        {
            get => _isNextQuestionButtonEnabled;
            set
            {
                _isNextQuestionButtonEnabled = value;
                OnPropertyChanged(nameof(IsNextQuestionButtonEnabled));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public bool IsQuizChoiceEnabled
        {
            get => _isQuizChoiceEnabled;
            set
            {
                _isQuizChoiceEnabled = value;
                OnPropertyChanged(nameof(IsQuizChoiceEnabled));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public bool IsStartQuizButtonEnabled
        {
            get => _isStartQuizButtonEnabled;
            set
            {
                _isStartQuizButtonEnabled = value;
                OnPropertyChanged(nameof(IsStartQuizButtonEnabled));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public bool IsBackToMenuButtonEnabled
        {
            get => _isBackToMenuButtonEnabled;
            set
            {
                _isBackToMenuButtonEnabled = value;
                OnPropertyChanged(nameof(IsBackToMenuButtonEnabled));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public bool IsEndQuizButtonEnabled
        {
            get => _isEndQuizButtonEnabled;
            set
            {
                _isEndQuizButtonEnabled = value;
                OnPropertyChanged(nameof(IsEndQuizButtonEnabled));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public Quiz CurrentQuiz
        {
            get => _currentQuiz;
            set
            {
                _currentQuiz = value;
                OnPropertyChanged(nameof(CurrentQuiz));
            }
        }

        public Question CurrentQuestion
        {
            get => _currentQuestion;
            set
            {
                _currentQuestion = value;
                OnPropertyChanged(nameof(CurrentQuestion));
                OnPropertyChanged(nameof(QuestionsRemaining));
            }
        }

        public int ElapsedTime
        {
            get => _elapsedTime;
            set
            {
                _elapsedTime = value;
                OnPropertyChanged(nameof(ElapsedTimeMinutes));
            }
        }

        public string ElapsedTimeMinutes => TimeSpan.FromSeconds(ElapsedTime).ToString(@"mm\:ss");

        public int QuestionsRemaining => CurrentQuiz?.Questions.Count - _currentQuestionIndex - 1 ?? 0;

        public int Score
        {
            get => _score;
            set
            {
                _score = value;
                OnPropertyChanged(nameof(Score));
            }
        }

        public string ScoreDisplay => $"{Score} / {_totalScore}";

        public List<string> Quizzes
        {
            get => _allQuizzesName;
        }

        public string SelectedQuiz { 
            get => _selectedQuiz;
            set
            {
                _selectedQuiz = value;
                OnPropertyChanged(nameof(SelectedQuiz));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public ICommand NextQuestionCommand { get; }
        public ICommand StartQuizCommand { get; }
        public ICommand BackToMenuCommand { get; }
        public ICommand EndQuizCommand { get; }

        private void NextQuestion(object parameter)
        {
            if (CurrentQuestion.IsAnsweredCorrectly())
            {
                Score++;
            }
            _totalScore++;
            OnPropertyChanged(nameof(ScoreDisplay));

            if (_currentQuestionIndex < CurrentQuiz.Questions.Count - 1)
            {
                _currentQuestionIndex++;
                CurrentQuestion = CurrentQuiz.Questions[_currentQuestionIndex];
                OnPropertyChanged(nameof(QuestionsRemaining));
            }
            else
            {
                EndQuiz(null);
                string message = $"Ukończyłeś ten quiz!\nWynik: {ScoreDisplay}\nCzas trwania: {ElapsedTimeMinutes:mm\\:ss}";
                MessageBox.Show(message, "Quiz Ukończony", MessageBoxButton.OK);
            }
        }

        private bool NextQuestionCanExecute(object parameter) => IsNextQuestionButtonEnabled;
        
        private void StartQuiz(object parameter)
        {
            _currentQuiz = _repository.LoadQuizByName(SelectedQuiz);

            if (_currentQuiz.Questions.Count > 0) 
            {
                _currentQuestionIndex = 0;
                _totalScore = 0;
                _score = 0;
                OnPropertyChanged(nameof(ScoreDisplay));
                CurrentQuestion = CurrentQuiz.Questions[_currentQuestionIndex];

                ElapsedTime = 0;
                _timer.Start();

                IsQuizChoiceEnabled = false;
                IsBackToMenuButtonEnabled = false;
                IsStartQuizButtonEnabled = false;
                IsEndQuizButtonEnabled = true;
                IsNextQuestionButtonEnabled = true;
            }
            else
            {
                MessageBox.Show("Ten Quiz jest pusty. Dodaj Pytania!");
            }
        }

        private bool StartQuizCanExecute(object parameter) => IsStartQuizButtonEnabled && !string.IsNullOrEmpty(SelectedQuiz);

        private void BackToMenu(object parameter)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.MainFrame.Navigate(new MainMenu());
            }
        }

        private bool BackToMenuCanExecute(object parameter) => IsBackToMenuButtonEnabled;

        private void EndQuiz(object parameter)
        {
            _timer.Stop();

            IsQuizChoiceEnabled = true;
            IsBackToMenuButtonEnabled = true;
            IsStartQuizButtonEnabled = true;
            IsEndQuizButtonEnabled = false;
            IsNextQuestionButtonEnabled = false;
        }

        private bool EndQuizCanExecute(object parameter) => IsEndQuizButtonEnabled;

        private void TimerTick(object sender, EventArgs e)
        {
            ElapsedTime++;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
