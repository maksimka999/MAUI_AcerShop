using Android.App;
using Android.Content;
using Android.Gms.Auth.Api;
using Android.Gms.Auth.Api.SignIn;
using Android.Gms.Common;
using Android.Gms.Tasks;
using Java.Lang;
using System;
using System.Threading.Tasks;

namespace AcerShop.Platforms.Android
{
    public class GoogleAuthService : Java.Lang.Object, IOnCompleteListener
    {
        public static GoogleAuthService Instance { get; } = new GoogleAuthService();

        private TaskCompletionSource<string> _tcs;

        public Task<string> SignInAsync()
        {
            _tcs = new TaskCompletionSource<string>();

            var gso = new GoogleSignInOptions.Builder(GoogleSignInOptions.DefaultSignIn)
                .RequestIdToken("223019245263-blsgcj84m2g88hs492ac8o9cffkkf1g4.apps.googleusercontent.com")
                .RequestEmail()
                .Build();

            var client = GoogleSignIn.GetClient(Platform.CurrentActivity, gso);

            var signInIntent = client.SignInIntent;
            Platform.CurrentActivity.StartActivityForResult(signInIntent, 9001);

            return _tcs.Task;
        }

        public void OnComplete(global::Android.Gms.Tasks.Task task)
        {
            if (task.IsSuccessful)
            {
                var account = (GoogleSignInAccount)task.GetResult(Java.Lang.Class.FromType(typeof(GoogleSignInAccount)));
                _tcs?.TrySetResult(account?.IdToken);
            }
            else
            {
                _tcs?.TrySetException(new Java.Lang.Exception(task.Exception?.Message ?? "Google Sign-in failed"));
            }
        }
    }
}