using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Provolver_HalfLifeAlyx
{
    public class Engine
      {
        public bool menuOpen;

        public async Task initSyncAsync()
        {
            Debug.WriteLine("Initializing ProTube gear...");
            await ForceTubeVRInterface.InitAsync(true);
            Thread.Sleep(10000);
        }
    
        public void PlayerShoot(string weapon)
        {
          switch (weapon)
          {
            case "hlvr_weapon_energygun":
                ForceTubeVRInterface.Kick(210, ForceTubeVRChannel.pistol1);
                break;
            case "hlvr_weapon_rapidfire":
                ForceTubeVRInterface.Kick(255, ForceTubeVRChannel.pistol1);
                break;
            case "hlvr_weapon_shotgun":
                ForceTubeVRInterface.Shoot(210, 255, 200f, ForceTubeVRChannel.pistol1);
                break;
            default:
                ForceTubeVRInterface.Kick(210, ForceTubeVRChannel.pistol1);
                break;
          }
        }

        public void ClipInserted()
        {

        }

        public void ChamberedRound()
        {

        }
    }
}
