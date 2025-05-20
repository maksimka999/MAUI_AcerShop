using AcerShop.View;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection; 
#if ANDROID
using AcerShop.Platforms.Android;
#endif
using AcerShop.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Firebase.Auth;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Maui.Controls;
using System;


namespace AcerShop.ViewModel;
public class FirebaseIdpResponse
{
    [JsonProperty("localId")]
    public string LocalId { get; set; }

    [JsonProperty("email")]
    public string Email { get; set; }

    [JsonProperty("idToken")]
    public string IdToken { get; set; }

    [JsonProperty("refreshToken")]
    public string RefreshToken { get; set; }

    [JsonProperty("expiresIn")]
    public string ExpiresIn { get; set; }
}

public partial class LoginPageViewModel : ObservableObject
{
    private readonly ApiService _apiService; 
    private readonly IServiceProvider _serviceProvider;

     private readonly string webApiKey = "AIzaSyBkE5KxjecVJLycx1kb_ZCj0HrfsE2y7SA";

    [ObservableProperty]
    private string _userName;

    [ObservableProperty]
    private string _userPassword;
     

    public LoginPageViewModel(ApiService apiService, IServiceProvider serviceProvider)
    {

        _apiService = apiService;
        _serviceProvider = serviceProvider;

    }
    [RelayCommand] // Команда для входа через Google
    private async Task GoogleLoginAsync() 
    {

   
        if (string.IsNullOrEmpty(webApiKey))
        {
            await Shell.Current.DisplayAlert("Ошибка конфигурации", "Firebase Web API Key не найден.", "OK");
            return;
        }

#if ANDROID 
        try
        {
            var googleIdToken = await GoogleAuthService.Instance.SignInAsync(); 

            if (!string.IsNullOrEmpty(googleIdToken))
            {

                using var client = new HttpClient(); 
                var payload = new
                {
                    postBody = $"id_token={googleIdToken}&providerId=google.com",
                    requestUri = "http://localhost", 
                    returnIdpCredential = true,
                    returnSecureToken = true
                };

                var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

                var response = await client.PostAsync(
                    $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithIdp?key={webApiKey}",
                    content);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Firebase response: {result}");

                    var auth = JsonConvert.DeserializeObject<FirebaseIdpResponse>(result);
                    if (string.IsNullOrEmpty(auth?.LocalId))
                    {
                        await Shell.Current.DisplayAlert("Ошибка", "Не удалось получить Firebase UID", "OK");
                        return;
                    }

                    var user = await _apiService.EnsureUserCreated(auth.LocalId, auth.Email);

                    if (user == null)
                    {
                        await Shell.Current.DisplayAlert("Ошибка", "Не удалось получить данные пользователя с сервера", "OK");
                        return;
                    }

                    App.CurrentUser = user;
                    Console.WriteLine($"User {user.Name} ({user.Id}) logged in via Google.");

                    await SecureStorage.Default.SetAsync("firebase_id_token", auth.IdToken);
                    await SecureStorage.Default.SetAsync("firebase_refresh_token", auth.RefreshToken);
                    await SecureStorage.Default.SetAsync("firebase_uid", auth.LocalId);

                    Preferences.Set("user_logged_in", true);

          
             await Shell.Current.GoToAsync("//MainPage");
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                     Console.WriteLine($"Firebase IDP API Error: {response.StatusCode} - {error}");
                    await Shell.Current.DisplayAlert("Ошибка Firebase API", $"Не удалось войти через Google: {response.StatusCode}", "OK"); // Упрощенное сообщение
                }
            } else {
                Console.WriteLine("Google Sign-in did not return an ID token.");
            }
        }

        catch (Exception ex)
        {
         Console.WriteLine($"Google Login Exception: {ex.Message}");
           await Shell.Current.DisplayAlert("Ошибка", $"{ex.Message}", "OK");
            
        }
#else 
        await Shell.Current.DisplayAlert("Не поддерживается", "Вход через Google доступен только на Android.", "OK");
#endif
    }

    [RelayCommand] 
    private async Task LoginAsync()
    {


        if (string.IsNullOrWhiteSpace(UserName) || string.IsNullOrWhiteSpace(UserPassword))
        {
            await Shell.Current.DisplayAlert("Ошибка", "Пожалуйста, заполните все поля.", "OK");
            return;
        }

        if (!IsValidEmail(UserName)) 
        {
            await Shell.Current.DisplayAlert("Ошибка", "Введите корректный Email.", "OK");
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
            var auth = await authProvider.SignInWithEmailAndPasswordAsync(UserName, UserPassword);
            Console.WriteLine($"Firebase UID: {auth.User.LocalId}");

            Preferences.Set("FirebaseUID", auth.User.LocalId);
            Preferences.Set("FirebaseIdToken", auth.FirebaseToken);
   
            var user = await _apiService.EnsureUserCreated(auth.User.LocalId, auth.User.Email);

            if (user == null)
            {
                await Shell.Current.DisplayAlert("Ошибка", "Не удалось получить данные пользователя с сервера", "OK");
                return;
            }

            App.CurrentUser = user;

            await Shell.Current.GoToAsync("//MainPage");

        }
        catch (FirebaseAuthException ex) when (ex.Reason == AuthErrorReason.UnknownEmailAddress ||
                                                ex.Message.Contains("EMAIL_NOT_FOUND") ||
                                                ex.Reason == AuthErrorReason.WrongPassword ||
                                                ex.Reason == AuthErrorReason.InvalidEmailAddress || 
                                                ex.Reason == AuthErrorReason.UserDisabled) 
        {
            string errorMessage = "Неверный Email или пароль.";
            if (ex.Reason == AuthErrorReason.UnknownEmailAddress || ex.Message.Contains("EMAIL_NOT_FOUND"))
            {
                errorMessage = "Пользователь с таким Email не найден.";
            }
            else if (ex.Reason == AuthErrorReason.UserDisabled)
            {
                errorMessage = "Ваша учетная запись отключена.";
            }
            await Shell.Current.DisplayAlert("Ошибка входа", errorMessage, "OK");
            Console.WriteLine($"Firebase Auth Error: {ex.Reason} - {ex.Message}");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Ошибка", $"Произошла ошибка при входе: неверный пароль",@"OK");
            Console.WriteLine($"Login Error: {ex.Message}");
        }
    }


    private bool IsValidEmail(string email)
    {
        return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    }
    [RelayCommand]
    private async Task RegisterBtnTappedAsync()
    {
        try
        {
            await Shell.Current.GoToAsync(nameof(RegisterPage));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Navigation Error: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert("Ошибка навигации", "Не удалось перейти на страницу профиля.", "OK");
        }
    }
}

