﻿using LiteDB;
using MoreHomes.Helpers;
using MoreHomes.Models;
using Rocket.API;
using Steamworks;
using System;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MoreHomes
{
    public class MoreHomesDatabase
    {
        private MoreHomes pluginInstance => MoreHomes.Instance;

        public bool ClaimBed(CSteamID steamId, string bedName, byte x, byte y, Vector3 position, bool checkPermissions = true)
        {
            using (LiteDatabase database = Utilities.GetLiteDb("Database.db"))
            {
                LiteCollection<PlayerBed> BedsData = database.GetCollection<PlayerBed>("HomesData");

                var results = BedsData.Find(bed => bed.SteamId == steamId.m_SteamID);

                if (bedName == null)
                    bedName = GetNameForBed(steamId);

                if (results.Any(bed => bed.SteamId == steamId.m_SteamID && bed.Position == position.ToString()))
                    return false;

                if (checkPermissions)
                {
                    int maxLimit = pluginInstance.Configuration.Instance.DefaultHomes;

                    RocketPlayer player = new RocketPlayer(steamId.m_SteamID.ToString());

                    foreach (var item in pluginInstance.Configuration.Instance.Permissions)
                    {
                        if (player.HasPermission(item.SPermission))
                        {
                            maxLimit = item.MaxHomes;
                        }
                    }
                    if (results.Count() >= maxLimit)
                    {
                        return false;
                    }
                }                

                PlayerBed bedData = new PlayerBed(steamId.m_SteamID, bedName, x, y, position);
                BedsData.Insert(bedData);
            }
            return true;
        }

        public PlayerBed GetBedByName(CSteamID steamId, string bedName)
        {
            using (LiteDatabase database = Utilities.GetLiteDb("Database.db"))
            {
                LiteCollection<PlayerBed> BedsData = database.GetCollection<PlayerBed>("HomesData");

                return BedsData.FindOne(x => x.SteamId == steamId.m_SteamID && x.BedName.Equals(bedName, System.StringComparison.OrdinalIgnoreCase));
            }
        }

        public PlayerBed GetBedByPosition(CSteamID steamId, Vector3 position)
        {
            using (LiteDatabase database = Utilities.GetLiteDb("Database.db"))
            {
                LiteCollection<PlayerBed> BedsData = database.GetCollection<PlayerBed>("HomesData");

                return BedsData.FindOne(x => x.SteamId == steamId.m_SteamID && x.Position == position.ToString());
            }
        }

        public void RemoveBedByPosition(byte x, byte y, Vector3 position)
        {
            using (LiteDatabase database = Utilities.GetLiteDb("Database.db"))
            {
                LiteCollection<PlayerBed> BedsData = database.GetCollection<PlayerBed>("HomesData");
                BedsData.Delete(bed => bed.X == x && bed.Y == y && bed.Position == position.ToString());
            }
        }

        public void RemoveBedByName(CSteamID steamId, string bedName)
        {
            using (LiteDatabase database = Utilities.GetLiteDb("Database.db"))
            {
                LiteCollection<PlayerBed> BedsData = database.GetCollection<PlayerBed>("HomesData");
                BedsData.Delete(x=> x.SteamId == steamId.m_SteamID && x.BedName.Equals(bedName, System.StringComparison.OrdinalIgnoreCase));
            }
        }

        public string GetAllBedsMessage(CSteamID steamId)
        {
            using (LiteDatabase database = Utilities.GetLiteDb("Database.db"))
            {
                LiteCollection<PlayerBed> BedsData = database.GetCollection<PlayerBed>("HomesData");

                var beds = BedsData.Find(x => x.SteamId == steamId.m_SteamID);

                if (beds.Count() == 0)
                    return pluginInstance.Translate("no_home");

                StringBuilder result = new StringBuilder(MoreHomes.Instance.Translate("command_homes"));

                foreach (PlayerBed bed in beds)
                {
                    result.Append($" {bed.BedName},");
                }

                string returnValue = result.ToString();
                char[] charsToTrim = { ',', ' ' };
                return returnValue.TrimEnd(charsToTrim);

            }
        }

        public string GetNameForBed(CSteamID steamId)
        {
            using (LiteDatabase database = Utilities.GetLiteDb("Database.db"))
            {
                LiteCollection<PlayerBed> BedsData = database.GetCollection<PlayerBed>("HomesData");

                int count = BedsData.Count(x => x.SteamId == steamId.m_SteamID);

                while(BedsData.Exists(x=> x.BedName.Equals("bed" + count) && x.SteamId.Equals(steamId.m_SteamID)))
                {
                    count++;
                }
                return "bed" + count;
            }
        }

        public bool RenameBed(CSteamID steamId, string oldName, string newName)
        {
            using (LiteDatabase database = Utilities.GetLiteDb("Database.db"))
            {
                LiteCollection<PlayerBed> BedsData = database.GetCollection<PlayerBed>("HomesData");

                if (!BedsData.Exists(x => x.BedName.Equals(oldName, System.StringComparison.OrdinalIgnoreCase) && x.SteamId == steamId.m_SteamID))
                    return false;

                PlayerBed bed = BedsData.FindOne(x => (x.SteamId == steamId.m_SteamID) && (x.BedName.Equals(oldName, System.StringComparison.OrdinalIgnoreCase)));

                bed.BedName = newName;
                BedsData.Update(bed);
            }
            return true;
        }
    }



}
