using AcerShop.Model;
using AcerShop.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Firebase.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Maui.Controls;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AcerShop.ViewModel
{
    public partial class RegisterViewModel : ObservableObject, INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private readonly PasswordValidationService _passwordValidationService;
        private readonly IServiceProvider _serviceProvider;

        public ICommand ValidatePasswordCommand { get; set; }

        private readonly string webApiKey = "AIzaSyBkE5KxjecVJLycx1kb_ZCj0HrfsE2y7SA";

        [ObservableProperty]
        private string _email;

        [ObservableProperty]
        private string _password;

        partial void OnPasswordChanged(string value)
        {
            ValidatePassword();
        }

        public double PasswordStrength { get; set; }

        public string PasswordLengthString { get; set; } = "Password length: 0";

        public PasswordValidationModel validation { get; set; } = new PasswordValidationModel();

        public RegisterViewModel(ApiService apiService, IServiceProvider serviceProvider, PasswordValidationService passwordValidationService)
        {
            Console.WriteLine("RegisterViewModel initialized!");
            _apiService = apiService;
            _serviceProvider = serviceProvider;
            _passwordValidationService = passwordValidationService;

            ValidatePasswordCommand = new Command(() =>
            {
                ValidatePassword();
            });

        }

        [RelayCommand]
        private async Task RegisterUserAsync()
        {
            if (string.IsNullOrWhiteSpace(Email) || !IsValidEmail(Email))
            {
                await Shell.Current.DisplayAlert("Ошибка", "Введите корректный Email.", "OK");
                return;
            }

            var passwordErrors = ValidateFullPassword(Password);

            if (passwordErrors != null)
            {
                await Shell.Current.DisplayAlert("Ошибка", passwordErrors, "OK");
                return;
            }

            if (string.IsNullOrEmpty(webApiKey))
            {
                await Shell.Current.DisplayAlert("Ошибка конфигурации", "Firebase Web API Key не найден.", "OK");
                return;
            }

            var authProvider = new FirebaseAuthProvider(new Firebase.Auth.FirebaseConfig(webApiKey));

            try
            {
                var auth = await authProvider.CreateUserWithEmailAndPasswordAsync(Email, Password);
                var firebaseToken = auth.FirebaseToken;
                var firebaseUid = auth.User.LocalId;

                if (!string.IsNullOrEmpty(firebaseToken))
                {
                    Console.WriteLine($"Registered user in Firebase. UID: {firebaseUid}, Email: {Email}");

                    var user = await _apiService.EnsureUserCreated(firebaseUid, Email);

                    if (user == null)
                    {
                        await Shell.Current.DisplayAlert("Ошибка", "Не удалось получить данные пользователя с сервера после регистрации.", "OK");
                        return;
                    }

                    App.CurrentUser = user;
                    Console.WriteLine($"Created/Synchronized user: ID={user.Id} with role '{user.CustomRole}' for FirebaseUID={user.FirebaseUid}");

                    await SecureStorage.Default.SetAsync("firebase_id_token", firebaseToken);
                    await SecureStorage.Default.SetAsync("firebase_uid", firebaseUid);

                    await Shell.Current.DisplayAlert("Успех", "Пользователь успешно зарегистрирован и синхронизирован.", "OK");

                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    await Shell.Current.DisplayAlert("Ошибка", "Не удалось получить токен Firebase после регистрации.", "OK");
                }
            }
            catch (FirebaseAuthException ex)
            {
                string errorMessage = "Ошибка Firebase регистрации.";
                if (ex.Reason == AuthErrorReason.EmailExists)
                {
                    errorMessage = "Пользователь с такой почтой уже существует.";
                }
                else if (ex.Reason == AuthErrorReason.WeakPassword)
                {
                    errorMessage = "Пароль слишком слабый. Пожалуйста, используйте более надежный пароль.";
                }
                else if (ex.Reason == AuthErrorReason.InvalidEmailAddress)
                {
                    errorMessage = "Некорректный формат Email.";
                }
                else
                {
                    errorMessage = $"Ошибка Firebase: {ex.Reason}";
                }

                Console.WriteLine($"Firebase Auth Error during registration: {ex.Reason} - {ex.Message}");
                await Shell.Current.DisplayAlert("Ошибка регистрации", errorMessage, "OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Registration Error: {ex.Message}");
                await Shell.Current.DisplayAlert("Ошибка", $"Не удалось зарегистрироваться: {ex.Message}", "OK");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void ValidatePassword()
        {
            validation = _passwordValidationService.CalculatePasswordStrength(Password);
            PasswordStrength = validation.StrengthScore / 100.0; // Используйте 100.0 для double деления
            PasswordLengthString = $"Password length: {validation.Length}";
            RaisePropertyChanged(nameof(PasswordStrength), nameof(validation), nameof(PasswordLengthString));


        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
                return;

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void RaisePropertyChanged(params string[] properties)
        {
            foreach (var propertyName in properties)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private string? ValidateFullPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password)) return "Пароль не может быть пустым.";
            var errors = new List<string>();
            if (password.Length < 8) errors.Add("Пароль должен содержать минимум 8 символов.");
            if (!Regex.IsMatch(password, "[A-Z]")) errors.Add("Пароль должен содержать хотя бы одну заглавную букву.");
            if (!Regex.IsMatch(password, "[a-z]")) errors.Add("Пароль должен содержать хотя бы одну строчную букву.");
            if (!Regex.IsMatch(password, "[0-9]")) errors.Add("Пароль должен содержать хотя бы одну цифру.");
            if (!Regex.IsMatch(password, @"[^a-zA-Z0-9]")) errors.Add("Пароль должен содержать хотя бы один специальный символ (например, !@#$%).");
            return errors.Count == 0 ? null : string.Join("\n", errors);
        }

        private bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }
    }
}