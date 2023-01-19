using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics;

namespace Provolver_HalfLifeAlyx
{

    ///<summary>
    ///Useful to target different ForceTubeVR.
    ///"all" send requests to all, ignoring the channel settings, and "rifle" send requests to both "rifleButt" and "rifleBolt".
    ///By default, InitAsync() make the first ForceTubeVR detected is placed in the channel "rifleButt", the second is placed in "rifleBolt", and following are placed in channels "pistol1", "pistol2", "other" and "vest".
    ///</summary>
    public enum ForceTubeVRChannel : int { all, rifle, rifleButt, rifleBolt, pistol1, pistol2, other, vest };


    public class ForceTubeVRInterface
    {
        [DllImport("UserLibs\\ForceTubeVR_API_x64", EntryPoint = "InitRifle")]
        private static extern void InitRifle_x64();

        [DllImport("UserLibs\\ForceTubeVR_API_x64", EntryPoint = "InitPistol")]
        private static extern void InitPistol_x64();

        [DllImport("UserLibs\\ForceTubeVR_API_x64", EntryPoint = "SetActive")]
        private static extern void SetActiveResearch_x64(bool active);

        [DllImport("UserLibs\\ForceTubeVR_API_x64", EntryPoint = "KickChannel")]
        private static extern void Kick_x64(Byte power, ForceTubeVRChannel channel);

        [DllImport("UserLibs\\ForceTubeVR_API_x64", EntryPoint = "RumbleChannel")]
        private static extern void Rumble_x64(Byte power, float timeInSeconds, ForceTubeVRChannel channel);

        [DllImport("UserLibs\\ForceTubeVR_API_x64", EntryPoint = "ShotChannel")]
        private static extern void Shot_x64(Byte kickPower, Byte rumblePower, float rumbleDuration, ForceTubeVRChannel channel);

        [DllImport("UserLibs\\ForceTubeVR_API_x64", EntryPoint = "TempoToKickPower")]
        private static extern Byte TempoToKickPower_x64(float tempo);

        [DllImport("UserLibs\\ForceTubeVR_API_x64", EntryPoint = "GetBatteryLevel")]
        private static extern Byte GetBatteryLevel_x64();


        [DllImport("UserLibs\\ForceTubeVR_API_x64", EntryPoint = "ListConnectedForceTube")]
        [return: MarshalAs(UnmanagedType.LPStr)]
        private static extern string ListConnectedForceTube_x64();

        [DllImport("UserLibs\\ForceTubeVR_API_x64", EntryPoint = "ListChannels")]
        [return: MarshalAs(UnmanagedType.LPStr)]
        private static extern string ListChannels_x64();

        [DllImport("UserLibs\\ForceTubeVR_API_x64", EntryPoint = "InitChannels")]
        private static extern bool InitChannels_x64([MarshalAs(UnmanagedType.LPStr)] string sJsonChannelList);

        [DllImport("UserLibs\\ForceTubeVR_API_x64", EntryPoint = "AddToChannel")]
        private static extern bool AddToChannel_x64(int nChannel, [MarshalAs(UnmanagedType.LPStr)] string sName);

        [DllImport("UserLibs\\ForceTubeVR_API_x64", EntryPoint = "RemoveFromChannel")]
        private static extern bool RemoveFromChannel_x64(int nChannel, [MarshalAs(UnmanagedType.LPStr)] string sName);

        [DllImport("UserLibs\\ForceTubeVR_API_x64", EntryPoint = "ClearChannel")]
        private static extern void ClearChannel_x64(int nChannel);

        [DllImport("UserLibs\\ForceTubeVR_API_x64", EntryPoint = "ClearAllChannel")]
        private static extern void ClearAllChannel_x64();



        ///<summary>
        ///As suggered, this method is asynchronous.
        ///Only need to be called once to let the Dll manage the ForceTubeVR's connection. 
        ///By default, InitAsync() place the first ForceTubeVR detected in the channel "rifleButt" and the second in "rifleBolt". 
        ///If it receives a boolean true as first param, the first forcetubevr is placed in "pistol1" and the second in "pistol2". 
        ///</summary>
        public static Task InitAsync(bool pistolsFirst = false)
        {
            if (pistolsFirst)
            {
                return Task.Run(() => InitPistol_x64());
            }
            else
            {
                return Task.Run(() => InitRifle_x64());
            }
        }

        ///<summary>
        ///0 = no power, 255 = max power, this function is linear.
        ///</summary>
        public static void Kick(Byte power, ForceTubeVRChannel target = ForceTubeVRChannel.rifle)
        {
            Kick_x64(power, target);
        }

        ///<summary>
        ///For power : 0 = no power, 255 = max power, if power is 126 or less, only the little motor is activated, this function is linear.
	    ///For timeInSeconds : 0.0f seconds is a special command that make the ForceTubeVR never stop the rumble.
        ///</summary>
	    public static void Rumble(Byte power, float duration, ForceTubeVRChannel target = ForceTubeVRChannel.rifle)
        {
            Rumble_x64(power, duration, target);
        }

        ///<summary>
        ///Combination of kick and rumble methods.
	    ///Rumble duration still be in seconds and still don't stop if you set this parameter at 0.0f.
        ///</summary>
	    public static void Shoot(Byte kickPower, Byte rumblePower, float rumbleDuration, ForceTubeVRChannel target = ForceTubeVRChannel.rifle)
        {
            Shot_x64(kickPower, rumblePower, rumbleDuration, target);
        }

        ///<summary>
	    ///It is true by default.  
	    ///Set it to false prevent the DLL to make a thread regularly check for connections and (re)connect ForceTubeVR when paired.
        ///</summary>
        public static void SetActiveResearch(bool active)
        {
            SetActiveResearch_x64(active);
        }

        ///<summary>
        ///Take duration in seconds between two shots(for auto-shots) and give you the maximal kick power you can use without any loss. 
	    ///If you don't use it, you may have some loss of kick if kick power is too big in high shot frequencies.
        ///</summary>
        public static Byte TempoToKickPower(float tempo)
        {
            return TempoToKickPower_x64(tempo);
        }

        ///<summary>
        ///Return the battery level in percents. 
	    ///So it's an unsigned byte value between 0 and 100.
        ///</summary>
        public static Byte GetBatteryLevel()
        {
            return GetBatteryLevel_x64();
        }



        public static string ListConnectedForceTube()
        {
            return ListConnectedForceTube_x64();
        }

        public static string ListChannels()
        {
            return ListChannels_x64();
        }

        public static bool InitChannels(string sJsonChannelList)
        {

            return InitChannels_x64(sJsonChannelList);
        }


        public static bool LoadChannelJSon()
        {
            string path = Application.ExecutablePath;
            string filePath = path + "/Channels.json";
            Debug.WriteLine("filePath : " + filePath);
            string dataAsJson = File.ReadAllText(filePath);
            return ForceTubeVRInterface.InitChannels(dataAsJson);
        }

        public static bool SaveChannelJSon()
        {
            string sText = ForceTubeVRInterface.ListChannels();
            string path = Application.ExecutablePath;
            string filePath = path + "/ProTubeChannels.json";
            Debug.WriteLine("filePath : " + filePath);
            File.WriteAllText(filePath, sText);
            return true;
        }

        [Serializable]
        public class FTChannel
        {
            public string name;
            public int batteryLevel;
        }

        [Serializable]
        public class FTCType
        {
            public List<FTChannel> rifleButt;
            public List<FTChannel> rifleBolt;
            public List<FTChannel> pistol1;
            public List<FTChannel> pistol2;
            public List<FTChannel> other;
            public List<FTChannel> vest;
        }

        [Serializable]
        public class FTChannelFile
        {
            public FTCType channels;
        }

        public static bool AddToChannel(int nChannel, string sName)
        {
            if (IntPtr.Size == 8)
            {
                return AddToChannel_x64(nChannel, sName);
            }
            else
                return false;
        }

        public static bool RemoveFromChannel(int nChannel, string sName)
        {
            if (IntPtr.Size == 8)
            {
                return RemoveFromChannel_x64(nChannel, sName);
            }
            else
                return false;
        }

        public static void ClearChannel(int nChannel)
        {
            if (IntPtr.Size == 8)
            {
                ClearChannel_x64(nChannel);
            }
        }

        public static void ClearAllChannel()
        {
            if (IntPtr.Size == 8)
            {
                ClearAllChannel_x64();
            }
        }

    }
}