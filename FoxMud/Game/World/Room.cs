﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoxMud.Db;
using FoxMud.Game.Item;
using Newtonsoft.Json;

namespace FoxMud.Game.World
{
    class Area : Storable
    {
        public Area()
        {
            Rooms = new List<string>();
        }

        public string Key { get { return Name.ToLower(); } }
        public string Name { get; set; }
        public List<string> Rooms { get; private set; }
    }

    class RoomExit
    {
        public string LeadsTo { get; set; }
        public bool IsDoor { get; set; }
        public bool IsOpen { get; set; }
        public bool IsLocked { get; set; }
        public string KeyItemKey { get; set; }
    }

    class Room : Storable
    {
        [JsonIgnore]
        private List<Player> players;

        public Room()
        {
            players = new List<Player>();
            Exits = new Dictionary<string, RoomExit>();
            Items = new Dictionary<string, string>();
        }

        public string Key { get; set; }
        public string Area { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Dictionary<string, RoomExit> Exits { get; private set; }

        [JsonIgnore]
        public Dictionary<string, string> Items { get; private set; }

        public IEnumerable<Player> GetPlayers()
        {
            return players;
        }

        public void AddPlayer(Player player)
        {
            if (!players.Contains(player))
                players.Add(player);
        }

        public void RemovePlayer(Player player)
        {
            players.Remove(player);
        }

        public bool HasExit(string exitName)
        {
            return Exits.ContainsKey(exitName);
        }

        public RoomExit GetExit(string exitName)
        {
            return Exits[exitName];
        }

        public void AddItem(PlayerItem item)
        {
            throw new NotImplementedException();
        }

        public void RemoveItem(PlayerItem item)
        {
            throw new NotImplementedException();
        }

        public RoomExit FindExitByPartialName(string exitSearch)
        {
            foreach (var exit in Exits)
            {
                if (exit.Key.StartsWith(exitSearch))
                    return exit.Value;
            }

            return null;
        }

        public RoomExit FindExitByLinkedRoom(string roomKey)
        {
            foreach (var exit in Exits)
            {
                if (exit.Value.LeadsTo == roomKey)
                    return exit.Value;
            }

            return null;
        }

        public void SendPlayers(string format, Player subject, Player target, params Player[] ignore)
        {
            foreach (var player in players)
            {
                if (ignore.Contains(player))
                    continue;

                player.Send(format, subject, target);
            }
        }

        public Player LookUpPlayer(Player doLookupFor, string keywords)
        {
            string[] lookUpKeywords = StringHelpers.GetKeywords(keywords);

            foreach (var player in players)
            {
                if (player == doLookupFor)
                    continue;

                List<string> possiblePlayerKeywords = new List<string>();

                possiblePlayerKeywords.AddRange(StringHelpers.GetKeywords(player.ShortDescription));

                if (doLookupFor.RememberedNames.ContainsKey(player.Key))
                {
                    string rememberedName = doLookupFor.RememberedNames[player.Key];
                    possiblePlayerKeywords.AddRange(StringHelpers.GetKeywords(rememberedName));
                }
                else
                {
                    doLookupFor.RememberedNames.Add(player.Key, player.Forename);
                }

                bool successful = true;
                foreach (var keyword in lookUpKeywords)
                {
                    if (!possiblePlayerKeywords.Contains(keyword))
                    {
                        successful = false;
                        break;
                    }
                }

                if (successful)
                    return player;
            }

            return null;
        }
    }
}
