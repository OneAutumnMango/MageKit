using MageQuitModFramework.Utilities;
using Photon;
using UnityEngine;

namespace MageKit.SpellRain
{
    /// <summary>
    /// Handles Photon networking for SpellRain pickups (spawning and pickup synchronization)
    /// </summary>
    public static class SpellRainNetworking
    {
        private static PhotonRpcManager _rpcManager;
        private const string SPAWN_RPC = "SpellRain_Spawn";
        private const string PICKUP_RPC = "SpellRain_Pickup";

        /// <summary>
        /// Initialize the networking system. Call once during module load.
        /// </summary>
        public static void Initialize()
        {
            if (_rpcManager != null)
            {
                Plugin.Log.LogInfo("[SpellRainNetworking] Already initialized");
                return;
            }

            // Create persistent RPC manager with a specific view ID
            _rpcManager = PhotonRpcManager.CreatePersistent("SpellRainRpcManager", viewID: 999);

            // Register handlers
            _rpcManager.RegisterHandler(SPAWN_RPC, HandleSpawnRpc);
            _rpcManager.RegisterHandler(PICKUP_RPC, HandlePickupRpc);

            Plugin.Log.LogInfo("[SpellRainNetworking] Initialized with RPC handlers");
        }

        /// <summary>
        /// Cleanup networking. Call during module unload.
        /// </summary>
        public static void Cleanup()
        {
            if (_rpcManager != null)
            {
                _rpcManager.ClearAllHandlers();
                Object.Destroy(_rpcManager.gameObject);
                _rpcManager = null;
            }
        }

        /// <summary>
        /// Network-safe spawn. Only master client actually spawns, then tells all clients.
        /// </summary>
        public static GameObject NetworkSpawnPickup(Vector3 position, SpellName spell, SpellButton targetSlot = SpellButton.Secondary)
        {
            // Generate unique ID for this pickup
            string pickupId = System.Guid.NewGuid().ToString();

            GameObject localPickup = SpawnPickupLocal(pickupId, position, spell, targetSlot);

            if (!PhotonNetwork.connected)
            {
                // Offline mode: spawn locally only
                return localPickup;
            }

            if (!PhotonNetwork.isMasterClient)
            {
                Plugin.Log.LogWarning("[SpellRainNetworking] Only master client can spawn pickups");
                return null;
            }

            // Send RPC to all other clients
            _rpcManager?.SendRpc(SPAWN_RPC, PhotonTargets.Others,
                pickupId,
                position.x, position.y, position.z,
                (int)spell,
                (int)targetSlot);

            Plugin.Log.LogInfo($"[SpellRainNetworking] Master spawned pickup {pickupId} and notified others");

            return localPickup;
        }

        /// <summary>
        /// Handle incoming spawn RPC from master client
        /// </summary>
        private static void HandleSpawnRpc(object[] args)
        {
            if (args.Length < 6)
            {
                Plugin.Log.LogError($"[SpellRainNetworking] Invalid spawn RPC args: {args.Length}");
                return;
            }

            try
            {
                string pickupId     = (string)args[0];
                float x             = System.Convert.ToSingle(args[1]);
                float y             = System.Convert.ToSingle(args[2]);
                float z             = System.Convert.ToSingle(args[3]);
                SpellName spell     = (SpellName)System.Convert.ToInt32(args[4]);
                SpellButton slot    = (SpellButton)System.Convert.ToInt32(args[5]);

                Vector3 position = new Vector3(x, y, z);
                SpawnPickupLocal(pickupId, position, spell, slot);

                Plugin.Log.LogInfo($"[SpellRainNetworking] Spawned pickup {pickupId} from RPC at {position}");
            }
            catch (System.Exception ex)
            {
                Plugin.Log.LogError($"[SpellRainNetworking] Error handling spawn RPC: {ex.Message}");
            }
        }

        /// <summary>
        /// Local spawn helper (used by both master and clients)
        /// </summary>
        private static GameObject SpawnPickupLocal(string pickupId, Vector3 position, SpellName spell, SpellButton targetSlot)
        {
            GameObject pickup = SpellRainSpawner.SpawnPickupCrystal(position, spell, targetSlot);
            if (pickup != null)
            {
                // Attach network ID to the pickup
                var helper = pickup.GetComponent<SpellRainHelper>();
                if (helper != null)
                {
                    helper.networkId = pickupId;
                }
            }
            return pickup;
        }

        /// <summary>
        /// Network-safe pickup removal. Picker tells all clients to remove the pickup.
        /// </summary>
        public static void NetworkPickup(string pickupId, int pickerOwner)
        {
            if (!PhotonNetwork.connected)
            {
                // Offline mode: already handled locally
                return;
            }

            // Tell ALL other clients to remove this pickup (send to Others, we already handled it locally)
            _rpcManager?.SendRpc(PICKUP_RPC, PhotonTargets.Others, pickupId);

            Plugin.Log.LogInfo($"[SpellRainNetworking] Player {pickerOwner} picked up {pickupId}, notified others");
        }

        /// <summary>
        /// Handle incoming pickup RPC (remove the crystal on this client)
        /// </summary>
        private static void HandlePickupRpc(object[] args)
        {
            if (args.Length < 1)
            {
                Plugin.Log.LogError($"[SpellRainNetworking] Invalid pickup RPC args: {args.Length}");
                return;
            }

            try
            {
                string pickupId = (string)args[0];

                // Find the pickup in the scene by ID
                SpellRainHelper[] allPickups = Object.FindObjectsOfType<SpellRainHelper>();
                foreach (var pickup in allPickups)
                {
                    if (pickup.networkId == pickupId)
                    {
                        Object.Destroy(pickup.gameObject);
                        Plugin.Log.LogInfo($"[SpellRainNetworking] Removed pickup {pickupId} via RPC");
                        return;
                    }
                }

                Plugin.Log.LogWarning($"[SpellRainNetworking] Could not find pickup {pickupId} to remove");
            }
            catch (System.Exception ex)
            {
                Plugin.Log.LogError($"[SpellRainNetworking] Error handling pickup RPC: {ex.Message}");
            }
        }
    }
}
