﻿using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MoreHomes.Commands
{
    public class CommandRenameBed : IRocketCommand
    {

        public string Name => "renamehome";

        public string Help => "Renames your bed";

        public string Syntax => "<oldBedName> <newBedName>";

        public List<string> Aliases
        {
            get { return new List<string>(); }
        }

        public AllowedCaller AllowedCaller
        {
            get { return AllowedCaller.Player; }
        }

        public List<string> Permissions => new List<string>() { "home.rename" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer player = (UnturnedPlayer)caller;
            
            if (command.Count() == 0 && command.Count() > 2)
            {
                UnturnedChat.Say(caller, string.Format(MoreHomes.Instance.Translate("command_rename_format")), Color.red);
                return;
            } 

            if (!Database.RenameBed(player.CSteamID, command[0], command[1]))
            {
                UnturnedChat.Say(caller, string.Format(MoreHomes.Instance.Translate("command_rename_not_found"), command[0]), Color.red);
                return;
            }

            UnturnedChat.Say(caller, string.Format(MoreHomes.Instance.Translate("command_rename_success"), command[0], command[1]), Color.yellow);
        }
    }
}