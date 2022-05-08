using System;
using System.Data;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Reflection;
using PlayFab.ClientModels;
//using PlayFab.ServerModels;
using PlayFab;
using System.Collections;


namespace ProjectSpecificGlobals
{
    public enum ContactType { GroundContact, CubeContact, BlockContact };
}

/// <summary>
/// The utilities class is for storing the general purpose commonly used functions and data structures 
/// </summary>
namespace Utilities
{

    public static class HelperFunctions
    {
        private static string conn = "URI=file:" + Application.dataPath + "/CardDataBase.db";
        private static Color[] colors = { Color.black, Color.gray, Color.clear, Color.blue, Color.cyan, Color.red, Color.green, Color.magenta, Color.yellow };

        #region Vector3 Operations

        /// <summary>
        /// Rotates a vector a specific number of degrees using Trig
        /// </summary>
        /// <param name="startingVector"></param>
        /// <param name="degree"></param>
        /// <returns></returns>
        public static Vector3 RotateVector(Vector3 startingVector, float degree)
        {
            float rads = ConvertToRadian(degree);

            float newZ = startingVector.z * Mathf.Cos(rads) - startingVector.y * Mathf.Sin(rads);
            float newY = startingVector.z * Mathf.Sin(rads) + startingVector.y * Mathf.Cos(rads);



            return new Vector3(startingVector.x, newY, newZ);
        }

        public static float ConvertToRadian(float degrees)
        {
            float rads = degrees * (Mathf.PI / 180);
            return rads;
        }

        #endregion

        #region Physics Calculations
        
        public static List<Vector3> CalculateProjectilePath(int numOfPoints, float timeInterval, Vector3 initialVel, Vector3 initialPos)
        {
            List<Vector3> points = new List<Vector3>();

            for(float t = 0; t < numOfPoints; t+= timeInterval)
            {
                Vector3 newPoint = initialPos + t * initialVel;
                newPoint.y = initialPos.y + initialVel.y * t + Physics.gravity.y * 0.5f * t * t;
                points.Add(newPoint);

            }

            return points;
        }

        #endregion

        #region Random Mathematics Funcions

        public static float Map(float outputMin, float outputMax, float inputMin, float inputMax, float value)
        {
            if(value >= inputMax)
            {
                return outputMax;
            }
            else if(value <= inputMin)
            {
                return outputMin;
            }

            return (outputMax - outputMin) * ((value - inputMin) / (inputMax - inputMin)) + outputMin;
        }

        public static float GetPercentageOf(float percentage,  float number)
        {
            percentage /= 100;
            return number - (number * percentage);
        }

        #endregion Random Mathematics Funcions

        #region Debug Utility Functions

        public static void Error(string msg)
        {
            throw new Exception(msg);
        }

        public static void Log(string msg)
        {
            Debug.Log(DateTime.Now + ": " + msg);
        }

        public static void LogListContent<T>(string msg, List<T> list)
        {
            string contents = PrintListContent(list);
            Log(msg + contents);
        }

        public static void LogListContent<T>(List<T> list)
        {
            string contents = PrintListContent(list);
            Log(contents);
        }

        public static string PrintListContent<T>(List<T> list)
        {
            string contents = "";
            foreach (T element in list)
            {
                contents += element.ToString() + " ";
            }

            return contents;
        }

        public static void Print(string msg)
        {
            Debug.Log(msg);
        }

        public static void CatchException(Exception e)
        {
            Debug.LogWarning(e.Source);
            Debug.LogWarning(e.Message);
            Debug.LogWarning(e.StackTrace);
            Debug.LogWarning(e.InnerException);
        }

        public static void PrintObjectProperties<T>(T src)
        {
            Type type = typeof(T);

            PropertyInfo[] propertyInfo = type.GetProperties();

            foreach (PropertyInfo pInfo in propertyInfo)
            {
                string val = type.GetProperty(pInfo.Name)?.GetValue(src, null)?.ToString();
                if(!String.IsNullOrEmpty(val))
                {
                    Print(pInfo.Name + ": " + val);
                }
            }
        }
        #endregion

        #region Misc
        public static Color RandomColor()
        {
            return colors[UnityEngine.Random.Range(0, colors.Length)];
        }

        public static T ParseEnum<T>(string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

        public static IEnumerator Timer(int seconds)
        {
            HelperFunctions.Log("Waiting for " + seconds + " seconds");
            yield return new WaitForSeconds(seconds);
        }

        public static IEnumerator Timer(float seconds)
        {
            //HelperFunctions.Log("Waiting for " + seconds + " seconds");
            yield return new WaitForSeconds(seconds);
        }


        public static List<int> CountDigit(int num)
        {
            List<int> digits = new List<int>();
            while (num >= 10)
            {
                digits.Add(num % 10);
                num /= 10;
            }

            digits.Add(num);
            digits.Reverse();
            return digits;
        }
        #endregion
    }

    public class Pointer
    {

    }

    public class PlayfabHelper
    {
        #region PlayFab Custom Event Name Enums
        public enum CustomEventNames
        {
            pd_player_connected_server,
            pd_player_disconnected_server,
            pd_player_completed_bs,
            pd_all_players_completed_bs,
            pd_all_clients_connected,
            pd_server_scene_change,
            pd_server_made_barrier_selects
        }
        #endregion
        private static PlayfabHelper _instance;

        #region Properties
        public static PlayfabHelper Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = new PlayfabHelper();
                }
                return _instance;
            }
        }

        public static string TitleID
        {
            get;
            private set;
        }

        public static string PlayFabID
        {
            get;
            private set;
        }

        public static string SessionTicket
        {
            get;
            private set;
        }
        #endregion

        

        #region Events
        public delegate void LoginSuccessEvent(LoginResult success);
        public static event LoginSuccessEvent OnLoginSuccess;

        //public delegate void AuthenticateSessionTicketSuccessEvent(AuthenticateSessionTicketResult success, NetworkConnection conn);
        //public static event AuthenticateSessionTicketSuccessEvent OnAuthSessionTicketSuccess;

        //public delegate void WriteTelemetryEvent(PlayFab.ServerModels.WriteEventResponse success);
        //public static event WriteTelemetryEvent OnWriteTelemetrySuccess;

        public delegate void PlayFabErrorEvent(PlayFabError error);
        public static event PlayFabErrorEvent OnPlayFabError;
        #endregion

        public PlayfabHelper()
        {
            _instance = this;
            //PlayFabSettings.TitleId = TitleID = "5EB3B";
            //PlayFabSettings.DisableFocusTimeCollection = true;
            //PlayFabSettings.DeveloperSecretKey = "OHWSMJBCG4U18Y4KYY4ZDBNRQBUQAORC1HODRYIHKFU8BNAWDY";
            //OnPlayFabError += HandlePlayFabError;

        }

        /*public static void WritePlayerSpecificEvent(WriteServerPlayerEventRequest eventData)
        {
            PlayFabServerAPI.WritePlayerEvent(eventData, (result) =>
            {
                if (OnWriteTelemetrySuccess != null)
                {
                    //report login result back to subscriber
                    OnWriteTelemetrySuccess.Invoke(result);
                }
            }, (error) =>
            {
                if (OnPlayFabError != null)
                {
                    //report error back to subscriber
                    OnPlayFabError.Invoke(error);
                }
            });
            return;
        }

        public static void WriteTitleEvent(PlayFab.ServerModels.WriteTitleEventRequest eventData)
        {
            PlayFabServerAPI.WriteTitleEvent(eventData, (result) =>
            {
                if (OnWriteTelemetrySuccess != null)
                {
                    //report login result back to subscriber
                    OnWriteTelemetrySuccess.Invoke(result);
                }
            }, (error) =>
            {
                if (OnPlayFabError != null)
                {
                    //report error back to subscriber
                    OnPlayFabError.Invoke(error);
                }
            });
            return;
        }
        public static void Login()
        {
            string customID = "";
            if(ClientStartUp.Instance.useOtherAccount)
            {
                customID = "SecondaryDevAccount";
            }
            else
            {
                customID = "MainDevAccount";
            }

            PlayFab.PlayFabClientAPI.LoginWithCustomID(new LoginWithCustomIDRequest()
            {
                TitleId = PlayFabSettings.TitleId,
                CustomId = customID,
                CreateAccount = true
            }, (result) =>
            {
                //Store identity and session
                PlayFabID = result.PlayFabId;
                SessionTicket = result.SessionTicket;

                if (OnLoginSuccess != null)
                {
                    //report login result back to subscriber
                    OnLoginSuccess.Invoke(result);
                }
            }, (error) =>
            {
                if (OnPlayFabError != null)
                {
                    //report error back to subscriber
                    OnPlayFabError.Invoke(error);
                }
            });
            return;
        }

        /*public static void AuthenticateSessionTicket(string sessionTicket, NetworkConnection conn, Action<PlayFabError, NetworkConnection> OnError)
        {
            HelperFunctions.Log("SessionTicket: " + sessionTicket);
            PlayFabServerAPI.AuthenticateSessionTicket(new AuthenticateSessionTicketRequest()
            {
                SessionTicket = sessionTicket
            }, (result) =>
            {
                OnAuthSessionTicketSuccess?.Invoke(result, conn);
            },
            (error)=>
            {
                OnError(error, conn);
            });
        }

        
        private static void HandlePlayFabError(PlayFabError error)
        {
            string fullErrorDetails = "Error in PlayFab API: " + error.ApiEndpoint + "\n" +
                "Error: " + error.Error.ToString() + "\n" + "Error Message: " + error.ErrorMessage
                + "\n" + "Error Details: " + error.ErrorDetails.ToString();
            HelperFunctions.Log(fullErrorDetails);

        }

        public static string DisplayPlayFabError(PlayFabError error)
        {
            string fullErrorDetails = "Error in PlayFab API: " + error.ApiEndpoint + "\n" +
                "Error: " + error.Error.ToString() + "\n" + "Error Message: " + error.ErrorMessage
                + "\n" + "Error Details: " + error.ErrorDetails.ToString();

            return fullErrorDetails;
        }*/
    }

}
