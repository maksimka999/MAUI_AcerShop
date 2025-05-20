using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AcerShop.Model;
using Microsoft.Maui.Controls;
using System;
using AcerShop.Services;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Extensions.DependencyInjection;
using AcerShop.View;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.Messaging;
using AcerShop.Messages;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;

namespace AcerShop.ViewModel
{
    [Preserve(AllMembers = true)]
    public partial class ProfilePageViewModel : ObservableObject
    {
        private readonly ApiService _apiService;
        private readonly IServiceProvider _serviceProvider;
        public Model.User? CurrentUser => App.CurrentUser;

        [ObservableProperty]
        private User _editableUser;

        [ObservableProperty]
        private ImageSource _userPhotoSource;

        public List<string> Genders { get; } = new List<string> { "Мужской", "Женский", "Другой" };

        private const string BelarusPhonePattern = @"^\+375\d{9}$";
        private const int MaxNameLength = 15;

        [RelayCommand]
        private async Task GoBackAsync()
        {
            WeakReferenceMessenger.Default.Send(new ProductsChangedMessage());

            var mainVm = _serviceProvider.GetService<MainPageViewModel>();
            if (mainVm != null)
            {
                mainVm.ShouldRefreshOnAppearing = true;
            }

            await Shell.Current.GoToAsync("//MainPage");
        }


        public ProfilePageViewModel(ApiService apiService, IServiceProvider serviceProvider)
        {
            _apiService = apiService;
            _serviceProvider = serviceProvider;

            if (App.CurrentUser == null)
            {
                Debug.WriteLine("Error: CurrentUser is null in ProfilePageViewModel constructor. Navigating to LoginPage.");
                EditableUser = new User();
                UserPhotoSource = ImageSource.FromFile("user_placeholder.png");
                return;
            }

            EditableUser = new User
            {
                Id = App.CurrentUser.Id,
                Name = App.CurrentUser.Name,
                Gender = App.CurrentUser.Gender,
                DateOfBirth = App.CurrentUser.DateOfBirth == DateTime.MinValue ? DateTime.Now.AddYears(-18) : App.CurrentUser.DateOfBirth,
                PhoneNumber = App.CurrentUser.PhoneNumber,
                Photo = App.CurrentUser.Photo,
            };

            UserPhotoSource = (App.CurrentUser.Photo != null && App.CurrentUser.Photo.Length > 0)
                ? ImageSource.FromStream(() => new MemoryStream(App.CurrentUser.Photo))
                : ImageSource.FromFile("user_placeholder.png");
        }

        [RelayCommand]
        private async Task ChangePhoto()
        {
            if (EditableUser == null) return;

            try
            {
                var result = await MediaPicker.PickPhotoAsync(new MediaPickerOptions { Title = "Выберите фото профиля" });

                if (result != null)
                {
                    using var stream = await result.OpenReadAsync();
                    var memoryStream = new MemoryStream();
                    await stream.CopyToAsync(memoryStream);
                    var newPhotoBytes = memoryStream.ToArray();

                    EditableUser.Photo = newPhotoBytes;
                    OnPropertyChanged(nameof(EditableUser));

                    UserPhotoSource = ImageSource.FromStream(() => new MemoryStream(newPhotoBytes));
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", $"Не удалось выбрать фото: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        private async Task Save()
        {
            if (EditableUser == null)
            {
                await Shell.Current.DisplayAlert("Ошибка", "Нет данных пользователя для сохранения.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(EditableUser.Name))
            {
                await Shell.Current.DisplayAlert("Ошибка валидации", "Имя не может быть пустым.", "OK");
                return;
            }
            if (EditableUser.Name.Length > MaxNameLength)
            {
                await Shell.Current.DisplayAlert("Ошибка валидации", $"Имя не может быть длиннее {MaxNameLength} символов.", "OK");
                return;
            }

            if (!string.IsNullOrWhiteSpace(EditableUser.PhoneNumber) && !Regex.IsMatch(EditableUser.PhoneNumber, BelarusPhonePattern))
            {
                await Shell.Current.DisplayAlert("Ошибка валидации", "Неверный формат номера телефона для Беларуси. Ожидается формат: +375XXXXXXXXX (например, +375291234567).", "OK");
                return;
            }

            if (!string.IsNullOrEmpty(EditableUser.Gender) && !Genders.Contains(EditableUser.Gender))
            {
                await Shell.Current.DisplayAlert("Ошибка валидации", "Пожалуйста, выберите пол из списка.", "OK");
                return;
            }

            if (EditableUser.DateOfBirth > DateTime.Now)
            {
                await Shell.Current.DisplayAlert("Ошибка валидации", "Дата рождения не может быть в будущем.", "OK");
                return;
            }
            if (EditableUser.DateOfBirth > DateTime.Now.AddYears(-3))
            {
                await Shell.Current.DisplayAlert("Ошибка валидации", "Указана некорректная дата рождения.", "OK");
                return;
            }


            try
            {
                App.CurrentUser.Name = EditableUser.Name;
                App.CurrentUser.Gender = EditableUser.Gender;
                App.CurrentUser.DateOfBirth = EditableUser.DateOfBirth;
                App.CurrentUser.PhoneNumber = EditableUser.PhoneNumber;
                App.CurrentUser.Photo = EditableUser.Photo;

                var success = await _apiService.UpdateUserAsync(App.CurrentUser);

                if (success)
                {
                    await Shell.Current.DisplayAlert("Успех", "Данные сохранены", "OK");
                }
                else
                {
                    await Shell.Current.DisplayAlert("Ошибка", "Не удалось сохранить данные на сервере", "OK");
                }
                OnPropertyChanged(nameof(EditableUser));
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", $"Ошибка при сохранении: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        private async Task SwitchAccount()
        {
            App.CurrentUser = null;
            Preferences.Clear();
            SecureStorage.Default.RemoveAll();

            await Shell.Current.GoToAsync("//LoginPage");
        }

        [RelayCommand]
        private async Task OpenAdminPanel()
        {
            try
            {
                await Shell.Current.GoToAsync(nameof(AdminProductsPage));
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", $"Не удалось открыть админ-панель: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        private async Task GoToMainAsync()
        {
            WeakReferenceMessenger.Default.Send(new ProductsChangedMessage());

            var mainVm = _serviceProvider.GetService<MainPageViewModel>();
            if (mainVm != null)
            {
                mainVm.ShouldRefreshOnAppearing = true;
            }

            await Shell.Current.GoToAsync("//MainPage");
        }

        [RelayCommand]
        private async Task GoToCartAsync()
        {
            if (App.CurrentUser == null)
            {
                await Shell.Current.DisplayAlert("Ошибка", "Необходимо войти в систему для доступа к корзине.", "OK");
                return;
            }
            await Shell.Current.GoToAsync(nameof(CartPage));
        }

        [RelayCommand]
        private async Task GoToSettingsAsync()
        {
            await Shell.Current.DisplayAlert("Навигация", "Раздел настроек находится в разработке.", "OK");
        }
    }
}