﻿using System.Runtime.InteropServices;

namespace FirebaseWebGL.Scripts.FirebaseBridge
{
    public static class FirebaseAuth
    {
        /// <summary>
        /// Creates and signs in a user anonymous
        /// </summary>
        /// <param name="objectName"> Name of the gameobject to call the callback/fallback of </param>
        /// <param name="callback"> Name of the method to call when the operation was successful. Method must have signature: void Method(string output) </param>
        /// <param name="fallback"> Name of the method to call when the operation was unsuccessful. Method must have signature: void Method(string output). Will return a serialized FirebaseError object </param>
        [DllImport("__Internal")]
        public static extern void SignInAnonymously(string objectName, string callback, string fallback);
        
        /// <summary>
        /// Creates a user with email and password
        /// </summary>
        /// <param name="email"> User email </param>
        /// <param name="password"> User password </param>
        /// <param name="objectName"> Name of the gameobject to call the callback/fallback of </param>
        /// <param name="callback"> Name of the method to call when the operation was successful. Method must have signature: void Method(string output) </param>
        /// <param name="fallback"> Name of the method to call when the operation was unsuccessful. Method must have signature: void Method(string output). Will return a serialized FirebaseError object </param>
        [DllImport("__Internal")]
        public static extern void CreateUserWithEmailAndPassword(string email, string password, string objectName, string callback,
            string fallback);
        
        /// <summary>
        /// Signs in a user with email and password
        /// </summary>
        /// <param name="email"> User email </param>
        /// <param name="password"> User password </param>
        /// <param name="objectName"> Name of the gameobject to call the callback/fallback of </param>
        /// <param name="callback"> Name of the method to call when the operation was successful. Method must have signature: void Method(string output) </param>
        /// <param name="fallback"> Name of the method to call when the operation was unsuccessful. Method must have signature: void Method(string output). Will return a serialized FirebaseError object </param>
        [DllImport("__Internal")]
        public static extern void SignInWithEmailAndPassword(string email, string password, string objectName, string callback,
            string fallback);
        
        /// <summary>
        /// Signs in a user with Google
        /// </summary>
        /// <param name="objectName"> Name of the gameobject to call the callback/fallback of </param>
        /// <param name="callback"> Name of the method to call when the operation was successful. Method must have signature: void Method(string output) </param>
        /// <param name="fallback"> Name of the method to call when the operation was unsuccessful. Method must have signature: void Method(string output). Will return a serialized FirebaseError object </param>
        [DllImport("__Internal")]
        public static extern void SignInWithGoogle(string objectName, string callback,
            string fallback);
        
        /// <summary>
        /// Signs in a user with Facebook
        /// </summary>
        /// <param name="objectName"> Name of the gameobject to call the callback/fallback of </param>
        /// <param name="callback"> Name of the method to call when the operation was successful. Method must have signature: void Method(string output) </param>
        /// <param name="fallback"> Name of the method to call when the operation was unsuccessful. Method must have signature: void Method(string output). Will return a serialized FirebaseError object </param>
        [DllImport("__Internal")]
        public static extern void SignInWithFacebook(string objectName, string callback,
            string fallback);
        
        /// <summary>
        /// Signs in a user with custom authentication token
        /// </summary>
        /// <param name="token"> Firebase authentication token </param>
        /// <param name="objectName"> Name of the gameobject to call the callback/fallback of </param>
        /// <param name="callback"> Name of the method to call when the operation was successful. Method must have signature: void Method(string output) </param>
        /// <param name="fallback"> Name of the method to call when the operation was unsuccessful. Method must have signature: void Method(string output). Will return a serialized FirebaseError object </param>
        [DllImport("__Internal")]
        public static extern void SignInWithToken(string token, string objectName, string callback,
            string fallback);
        
        /// <summary>
        /// Listens for changes of the auth state (sign in/sign out)
        /// </summary>
        /// <param name="objectName"> Name of the gameobject to call the onUserSignedIn/onUserSignedOut of </param>
        /// <param name="onUserSignedIn"> Name of the method to call when the user signs in. Method must have signature: void Method(string output). Will return a serialized FirebaseUser object </param>
        /// <param name="onUserSignedOut"> Name of the method to call when the operation was unsuccessful. Method must have signature: void Method(string output) </param>
        [DllImport("__Internal")]
        public static extern void OnAuthStateChanged(string objectName, string onUserSignedIn,
            string onUserSignedOut);

        /// <summary>
        /// 현재 창에서 URL을 열기
        /// </summary>
        /// <param name="url">열려는 URL</param>
        [DllImport("__Internal")]
        public static extern void OpenURLInSameWindow(string url);
    }
}