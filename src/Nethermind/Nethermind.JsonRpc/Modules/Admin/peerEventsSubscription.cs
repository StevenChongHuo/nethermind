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
using Nethermind.Stats.Model;

namespace Nethermind.JsonRpc.Modules.Admin
{
    public class PeerEventsSubscription : Subscription
    {
        //private readonly IBlockTree _blockTree;
        //private readonly bool _includeTransactions;
        //private readonly ISpecProvider _specProvider;
        private readonly IPeerPool _peerPool;


        public PeerEventsSubscription(
            IJsonRpcDuplexClient jsonRpcDuplexClient,
            //IBlockTree? blockTree,
            ILogManager? logManager,
            //ISpecProvider specProvider,
            //IJsonRpcParam? options = null,
            IPeerPool peerPool = null)
            : base(jsonRpcDuplexClient)
        {
            //_blockTree = blockTree ?? throw new ArgumentNullException(nameof(blockTree));
            _logger = logManager?.GetClassLogger() ?? throw new ArgumentNullException(nameof(logManager));
            //_includeTransactions = options?.IncludeTransactions ?? false;
            //_specProvider = specProvider;
            _peerPool = peerPool;

            _peerPool.PeerAdded += OnPeerAdded;
            _peerPool.PeerRemoved += OnPeerRemoved;
            if (_logger.IsTrace) _logger.Trace($"admin_peerEvent.Add subscription {Id} will track PeerAdded");
        }

        private void OnPeerAdded(object? sender, PeerEventArgs peerEventArgs)
        {
            PeerInfo peerInfo = new(peerEventArgs.Peer, false);
            ScheduleAction(async () =>
            {
                using JsonRpcResult result = CreateSubscriptionMessage(new PeerAddDropResponse(peerInfo, "Add", null), "admin_subscription"); // TODO: error message?
                //result.Response.MethodName = "admin_subscription"; // this is a hack at the moment, because MethodName is hard-coded in JsonRpcSubscriptionResponse
                await JsonRpcDuplexClient.SendJsonRpcResult(result);
                if (_logger.IsTrace) _logger.Trace($"admin_subscription {Id} printed new peer.");
            });
        }

        private void OnPeerRemoved(object? sender, PeerEventArgs peerEventArgs)
        {
            PeerInfo peerInfo = new(peerEventArgs.Peer, false); ;

            ScheduleAction(async () =>
            {
                using JsonRpcResult result = CreateSubscriptionMessage(new PeerAddDropResponse(peerInfo, "Drop", null), "admin_subscription"); // TODO: error message?
                result.Response.MethodName = "admin_subscription";
                await JsonRpcDuplexClient.SendJsonRpcResult(result);
                if (_logger.IsTrace) _logger.Trace($"admin_subscription {Id} printed dropped peer.");
            });
        }

        public override string Type => PeerEventsSubscriptionType.PeerEvents;

        public override void Dispose()
        {
            _peerPool.PeerAdded -= OnPeerAdded;
            _peerPool.PeerRemoved -= OnPeerRemoved;
            base.Dispose();
            if (_logger.IsTrace) _logger.Trace($"admin_subscription.peerEvent {Id} is no longer subscribed.");
        }
    }
}
