// SPDX-FileCopyrightText: 2022 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only

using System;
//using Nethermind.Blockchain;
//using Nethermind.Core;
//using Nethermind.Core.Specs;
//using Nethermind.Facade.Eth;
//using Nethermind.JsonRpc.Modules.Eth;
using Nethermind.Logging;
using Nethermind.JsonRpc.Modules.Subscribe;
using Nethermind.Network;

namespace Nethermind.JsonRpc.Modules.Admin
{
    public class PeerAddedSubscription : Subscription
    {
        //private readonly IBlockTree _blockTree;
        //private readonly bool _includeTransactions;
        //private readonly ISpecProvider _specProvider;
        private readonly IPeerPool _peerPool;


        public PeerAddedSubscription(
            IJsonRpcDuplexClient jsonRpcDuplexClient,
            //IBlockTree? blockTree,
            ILogManager? logManager,
            //ISpecProvider specProvider,
            IJsonRpcParam? options = null,
            IPeerPool peerPool = null)
            : base(jsonRpcDuplexClient)
        {
            //_blockTree = blockTree ?? throw new ArgumentNullException(nameof(blockTree));
            _logger = logManager?.GetClassLogger() ?? throw new ArgumentNullException(nameof(logManager));
            //_includeTransactions = options?.IncludeTransactions ?? false;
            //_specProvider = specProvider;
            _peerPool = peerPool;

            _peerPool.PeerAdded += OnPeerAdded;
            if (_logger.IsTrace) _logger.Trace($"admin_peerEvent.Add subscription {Id} will track PeerAdded");
        }

        private void OnPeerAdded(object? sender, PeerEventArgs e)
        {
            ScheduleAction(async () =>
            {
                using JsonRpcResult result = CreateSubscriptionMessage(e.Peer, "admin_subscription"); // What to pass? Probably a summary or a short version of this object.
                await JsonRpcDuplexClient.SendJsonRpcResult(result);
                //if (_logger.IsTrace) _logger.Trace($"admin_peerEvent.Add subscription {Id} printed new peer");
                _logger.Trace($"admin_peerEvent.Add subscription {Id} printed new peer");
                System.Diagnostics.Trace.WriteLine("This line was excecuted");
            });
        }

        public override string Type => PeerEventsSubscriptionType.Add;

        public override void Dispose()
        {
            _peerPool.PeerAdded -= OnPeerAdded;
            base.Dispose();
            if (_logger.IsTrace) _logger.Trace($"admin_peerEvent.Add subscription {Id} will no longer track PeerAdded");
        }
    }
}
