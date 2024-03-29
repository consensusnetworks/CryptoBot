﻿using System;
using System.Linq;
namespace TFABot
{
    public class clsNodeGroup : ISpreadsheet<clsNodeGroup>
    {
        public clsNodeGroup()
        {
        }

        public clsNetwork Network { get; private set; }

        [ASheetColumnHeader(true, "group")]
        public string Name { get; set; }

        [ASheetColumnHeader("network")]
        public string NetworkString
        {
            get
            {
                return Network?.Name ?? "Not Set";
            }
            set
            {
                clsNetwork network;
                if (Program.NetworkList.TryGetValue(value, out network))
                {
                    Network = network;
                }
                else
                {
                    throw new Exception("Network Name not found.");
                }
            }
        }

        [ASheetColumnHeader("ping")]
        public string Ping { get; set; }

        [ASheetColumnHeader("height")]
        public string Height { get; set; }

        [ASheetColumnHeader("latency")]
        public string Latency { get; set; }

        public void Monitor()
        {
            foreach (var node in Program.NodesList.Values.Where(x => x.Group == this.Name && x.Monitor))
            {
                //Check the height, against heighest known height
                if (node.LeaderHeight > Network.TopHeight) //New highest LearderHeight.
                {
                    if (Network.TopHeight > 0 && (node.LeaderHeight - Network.TopHeight > 10))
                    {
                        //Suspect wrong network setting
                        Program.SendAlert($"WARNING: {node.Name} height too high!  Wrong Bot setting?");
                        node.SyncMode = true;
                    }
                    else
                    {
                        Network.SetTopHeight(node.LeaderHeight);
                        node.HeightLowCount = 0;
                    }
                }
                else if (node.LeaderHeight < Network.TopHeight && node.RequestFailCount == 0)  //Node height too low.
                {
                    if (Network.TopHeight - node.LeaderHeight > 10) //If by a large amount, maybe Wrong setting or syncing?
                    {
                        if (!node.SyncMode) node.SyncMode = true;
                    }
                    else
                    {
                        node.HeightLowCount++;
                    }
                }
                else  //All good
                {
                    node.HeightLowCount = 0;
                }

                //Check latency
                if (node.Latency > node.LatencyLowest * 3 && node.LatencyLowest > 50) node.LatencyLowCount++;
            }
        }

        public void Update(clsNodeGroup group)
        {
            if (Name != group.Name) throw new Exception("index name does not match");

            Ping = group.Ping;
            Height = group.Height;
            Latency = group.Latency;
            Network = group.Network;
        }

        public string PostPopulate()
        {
            return (!Program.NotificationPolicyList.ContainsKey(Ping)) ||
            !Program.NotificationPolicyList.ContainsKey(Latency) ||
            !Program.NotificationPolicyList.ContainsKey(Height) ? $"Error: NodeGroup {Name} has incorrect notification." : null;
        }
    }
}
