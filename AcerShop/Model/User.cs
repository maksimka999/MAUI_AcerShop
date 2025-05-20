using System;
using System.IO; 
using CommunityToolkit.Mvvm.ComponentModel; 
using Microsoft.Maui.Controls; 

namespace AcerShop.Model 
{
    public partial class User : ObservableObject
    {
        [ObservableProperty]
        private int _id;

        [ObservableProperty]
        private string _firebaseUid = string.Empty;

        [ObservableProperty]
        private DateTime _registrationDate;

        [ObservableProperty]
        private string _customRole = string.Empty;

        [ObservableProperty]
        private string _name;

        [ObservableProperty]
        private string _gender;
            
        [ObservableProperty]
        private DateTime _dateOfBirth;

        [ObservableProperty]
        private string _phoneNumber;

        [ObservableProperty]
        private byte[] _photo;

        public ImageSource PhotoSource =>
            _photo != null && _photo.Length > 0
                ? ImageSource.FromStream(() => new MemoryStream(_photo))
                : ImageSource.FromFile("default_user_icon.png");

        public string DisplayDateOfBirth => _dateOfBirth.ToString("d") ?? "Не указано";

        public string DisplayPhoneNumber => string.IsNullOrWhiteSpace(_phoneNumber) ? "Не указан" : _phoneNumber;

        public bool IsAdmin => _customRole?.Equals("admin", StringComparison.OrdinalIgnoreCase) ?? false;
    }

}