using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;

namespace BagoScout.Services
{
    public static class PushNotificationHelper
    {
        private static readonly object LockObj = new object();
        private static bool _initialized = false;
        private static bool _hasCredentials = false;

        private static void InitializeFirebase()
        {
            if (_initialized) return;

            lock (LockObj)
            {
                if (_initialized) return;

                try
                {
                    // Check standard paths for service account key
                    var keyPath = Path.Combine(Directory.GetCurrentDirectory(), "firebase-key.json");
                    if (!File.Exists(keyPath))
                    {
                        // Fallback check in base directory
                        keyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "firebase-key.json");
                    }

                    if (File.Exists(keyPath))
                    {
                        if (FirebaseApp.DefaultInstance == null)
                        {
                            FirebaseApp.Create(new AppOptions()
                            {
                                Credential = GoogleCredential.FromFile(keyPath)
                            });
                        }
                        _hasCredentials = true;
                        System.Diagnostics.Debug.WriteLine("Firebase Admin SDK successfully initialized from firebase-key.json.");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Firebase service account credentials file (firebase-key.json) not found. Push notifications will run in Mock Mode.");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to initialize Firebase Admin SDK: {ex.Message}");
                }
                finally
                {
                    _initialized = true;
                }
            }
        }

        public static async Task SendNotificationAsync(string fcmToken, string title, string body, Dictionary<string, string>? data = null)
        {
            if (string.IsNullOrEmpty(fcmToken)) return;

            InitializeFirebase();

            if (_hasCredentials)
            {
                try
                {
                    var message = new FirebaseAdmin.Messaging.Message()
                    {
                        Token = fcmToken,
                        Notification = new FirebaseAdmin.Messaging.Notification()
                        {
                            Title = title,
                            Body = body
                        },
                        Data = data ?? new Dictionary<string, string>()
                    };

                    string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                    System.Diagnostics.Debug.WriteLine($"Successfully sent FCM notification. Message ID: {response}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"FCM Notification Delivery Error: {ex.Message}");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"=== [MOCK PUSH NOTIFICATION] ===");
                System.Diagnostics.Debug.WriteLine($"To FCM Token: {fcmToken}");
                System.Diagnostics.Debug.WriteLine($"Title       : {title}");
                System.Diagnostics.Debug.WriteLine($"Body        : {body}");
                if (data != null)
                {
                    System.Diagnostics.Debug.WriteLine("Data Payload:");
                    foreach (var kvp in data)
                    {
                        System.Diagnostics.Debug.WriteLine($"  {kvp.Key}: {kvp.Value}");
                    }
                }
                System.Diagnostics.Debug.WriteLine($"================================");
            }
        }
    }
}
