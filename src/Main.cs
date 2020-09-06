using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using static CitizenFX.Core.Native.API;

namespace VehicleObjects
{
    public class Main : BaseScript
    {

        public Dictionary<int, int> LPlates { get; set; }
        public Dictionary<int, int> PPlates { get; set; }
        public Dictionary<int, int> PenaltyCharge { get; set; }

        public Main()
        {
            TriggerEvent("chat:addSuggestion", "/lplate", "Adds or removes an L plate to your current vehicle");
            TriggerEvent("chat:addSuggestion", "/pplate", "Adds or removes a P plate to your current vehicle");
            TriggerEvent("chat:addSuggestion", "/penaltycharge", "Adds or removes a penalty charge notice to your current vehicle");
            LPlates = new Dictionary<int, int> { };
            PPlates = new Dictionary<int, int> { };
            PenaltyCharge = new Dictionary<int, int> { };
        }

        [Command("lplate")]
        private void LPlateCommand()
        {
            var vehicle = Raycast();
            if (vehicle == 0)
            {
                ReturnError();
            }
            else
            {
                if (LPlates.ContainsValue(vehicle))
                {
                    foreach (KeyValuePair<int, int> entry in LPlates)
                    {
                        if (entry.Value == vehicle)
                        {
                            var ent = entry.Key;
                            DeleteEntity(ref ent);
                        }
                    }
                }
                else
                {
                    var hash = GetHashKey("prop_lplate");
                    RequestPropModel(hash);
                    var coords = GetEntityCoords(vehicle, true);
                    var boneIndexFront = GetEntityBoneIndexByName(vehicle, "windscreen");
                    if (boneIndexFront != -1)
                    {
                        var obj = CreateObject(hash, coords.X, coords.Y, coords.Z, true, true, true);
                        SyncEntity(obj);
                        AttachEntityToEntity(obj, vehicle, boneIndexFront, 0.0f, 0.3f, -0.1f, -25.0f, 0.0f, 180.0f, true, true, false, true, 0, true);
                        LPlates.Add(obj, vehicle);
                    }
                    

                    var boneIndexRear = GetEntityBoneIndexByName(vehicle, "windscreen_r");
                    if (boneIndexRear != -1)
                    {
                        var obj2 = CreateObject(hash, coords.X, coords.Y, coords.Z, true, true, true);
                        SyncEntity(obj2);
                        AttachEntityToEntity(obj2, vehicle, boneIndexRear, 0.0f, 0.2f, -0.1f, -10.0f, 0.0f, 0.0f, true, true, false, true, 0, true);
                        LPlates.Add(obj2, vehicle);
                    }

                }
            }
        }

        [Command("pplate")]
        private void PPlateCommand()
        {
            var vehicle = Raycast();
            if (vehicle == 0)
            {
                ReturnError();
            }
            else
            {
                if (PPlates.ContainsValue(vehicle))
                {
                    foreach (KeyValuePair<int, int> entry in PPlates)
                    {
                        if (entry.Value == vehicle)
                        {
                            var ent = entry.Key;
                            DeleteEntity(ref ent);
                        }
                    }
                }
                else
                {
                    var hash = GetHashKey("prop_pplate");
                    RequestPropModel(hash);
                    var coords = GetEntityCoords(vehicle, true);
                    var boneIndexFront = GetEntityBoneIndexByName(vehicle, "windscreen");
                    if (boneIndexFront != -1)
                    {
                        var obj = CreateObject(hash, coords.X, coords.Y, coords.Z, true, true, true);
                        SyncEntity(obj);
                        AttachEntityToEntity(obj, vehicle, boneIndexFront, 0.0f, 0.3f, -0.1f, -25.0f, 0.0f, 180.0f, true, true, false, true, 0, true);
                        PPlates.Add(obj, vehicle);
                    }


                    var boneIndexRear = GetEntityBoneIndexByName(vehicle, "windscreen_r");
                    if (boneIndexRear != -1)
                    {
                        var obj2 = CreateObject(hash, coords.X, coords.Y, coords.Z, true, true, true);
                        SyncEntity(obj2);
                        AttachEntityToEntity(obj2, vehicle, boneIndexRear, 0.0f, 0.2f, -0.1f, -10.0f, 0.0f, 0.0f, true, true, false, true, 0, true);
                        PPlates.Add(obj2, vehicle);
                    }

                }
            }
        }

        [Command("penaltycharge")]
        private void PenaltyChargeCommand()
        {
            var vehicle = Raycast();
            if (vehicle == 0)
            {
                ReturnError();
            }
            else
            {
                if (PenaltyCharge.ContainsKey(vehicle))
                {
                    foreach (KeyValuePair<int, int> kvp in PenaltyCharge)
                    {
                        if (kvp.Key == vehicle)
                        {
                            var entity = kvp.Value;
                            SetEntityVisible(entity, false, false);
                            DeleteEntity(ref entity);

                            PenaltyCharge.Remove(vehicle);
                        }
                    }
                }
                else
                {
                    var hash = GetHashKey("prop_penaltycharge");
                    RequestPropModel(hash);
                    var coords = GetEntityCoords(vehicle, true);
                    var obj = CreateObject(hash, coords.X, coords.Y, coords.Z, true, true, true);
                    SyncEntity(obj);
                    AttachEntityToEntity(obj, vehicle, GetEntityBoneIndexByName(vehicle, "windscreen"), 0.0f, 0.2f, -0.1f, 30.0f, 0.0f, 0.0f, true, true, false, true, 0, true);
                    PenaltyCharge.Add(vehicle, obj);

                }
            }
        }

        private void SyncEntity(int entity)
        {
            var networkId = ObjToNet(entity);
            SetNetworkIdExistsOnAllMachines(networkId, true);
            SetNetworkIdCanMigrate(networkId, false);
            NetworkSetNetworkIdDynamic(networkId, true);
            FreezeEntityPosition(entity, true);
        }

        private int Raycast()
        {
            var location = GetEntityCoords(PlayerPedId(), true);
            var offSet = GetOffsetFromEntityInWorldCoords(PlayerPedId(), 0.0f, 6.0f, 0.0f);
            var shapeTest = StartShapeTestCapsule(location.X, location.Y, location.Z, offSet.X, offSet.Y, offSet.Z, 2f, 2, PlayerPedId(), 0);
            bool hit = false;
            Vector3 endCoords = new Vector3(0f, 0f, 0f);
            Vector3 surfaceNormal = new Vector3(0f, 0f, 0f);
            int entityHit = 0;
            var result = GetShapeTestResult(shapeTest, ref hit, ref endCoords, ref surfaceNormal, ref entityHit);
            if (!hit)
            {
                return GetVehiclePedIsIn(PlayerPedId(), false);
            }
            else
            {
                return entityHit;
            }
        }

        private void ReturnError(string message = "~r~Error: ~w~You must be near or inside a vehicle.")
        {
            CitizenFX.Core.UI.Screen.ShowNotification(message);
        }

        private async void RequestPropModel(int hash)
        {
            RequestModel((uint)hash);
            while (!HasModelLoaded((uint)hash))
            {
                await Delay(0);
            }
        }
    }
}
