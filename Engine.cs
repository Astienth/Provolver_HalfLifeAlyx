﻿using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Provolver_HalfLifeAlyx
{
    public class Engine
      {
        public bool menuOpen;
        private int grenadeLauncherState;

        public async Task initSyncAsync()
        {
            await ForceTubeVRInterface.InitAsync(true);
            Thread.Sleep(10000);
        }
    
        public void PlayerShoot(string weapon)
        {
          switch (weapon)
          {
            case "hlvr_weapon_energygun":
                ForceTubeVRInterface.Kick(210, ForceTubeVRChannel.all);
                break;
            case "hlvr_weapon_rapidfire":
                ForceTubeVRInterface.Kick(255, ForceTubeVRChannel.all);
                break;
            case "hlvr_weapon_shotgun":
                ForceTubeVRInterface.Shoot(210, 255, 65f, ForceTubeVRChannel.all);
                break;
            default:
                ForceTubeVRInterface.Kick(210, ForceTubeVRChannel.all);
                break;
          }
        }

        public void ClipInserted()
        {
            ForceTubeVRInterface.Rumble(85, 20f, ForceTubeVRChannel.all);
        }

        public void ChamberedRound()
        {
            ForceTubeVRInterface.Rumble(85, 20f, ForceTubeVRChannel.all);
        }

        public void GrenadeLauncherStateChange(int newState)
        {
            if (this.grenadeLauncherState == 2 && newState == 0)
            {
                ForceTubeVRInterface.Shoot(180, 126, 50f, ForceTubeVRChannel.all);
            }
            this.grenadeLauncherState = newState;
        }
    }
}
