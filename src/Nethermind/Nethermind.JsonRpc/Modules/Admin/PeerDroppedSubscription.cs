// SPDX-FileCopyrightText: 2022 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only

using System;
//using Nethermind.Blockchain;
//using Nethermind.Core;
//using Nethermind.Core.Specs;
//using Nethermind.Facade.Eth;
using Nethermind.JsonRpc.Modules.Eth;
using Nethermind.Logging;
using Nethermind.JsonRpc.Modules.Subscribe;
using Nethermind.Network;

namespace Nethermind.JsonRpc.Modules.Admin
{
    public class PeerDroppedSubscription : Subscription
    {
        //private readonly IBlockTree _blockTree;
        //private readonly bool _includeTransactions;
        //private readonly ISpecProvider _specProvider;
        private readonly IPeerPool _peerPool;


        public PeerDroppedSubscription(
            IJsonRpcDuplexClient jsonRpcDuplexClient,
            //IBlockTree? blockTree,
            ILogManager? logManager,
            //ISpecProvider specProvider,
            TransactionsOption? options = null,
            IPeerPool peerPool = null)
            : base(jsonRpcDuplexClient)
        {
            //_blockTree = blockTree ?? throw new ArgumentNullException(nameof(blockTree));
            _logger = logManager?.GetClassLogger() ?? throw new ArgumentNullException(nameof(logManager));
            //_includeTransactions = options?.IncludeTransactions ?? false;
            //_specProvider = specProvider;
            _peerPool = peerPool;

            _peerPool.PeerRemoved += OnPeerDropped;
            if (_logger.IsTrace) _logger.Trace($"admin_peerEvent.Drop subscription {Id} will track PeerRemoved");
        }

        private void OnPeerDropped(object? sender, PeerEventArgs e)
        {
            ScheduleAction(async () =>
            {
                using JsonRpcResult result = CreateSubscriptionMessage(e.Peer, "admin_subscription");
                await JsonRpcDuplexClient.SendJsonRpcResult(result);
                if (_logger.IsTrace) _logger.Trace($"admin_peerEvent.Drop subscription {Id} printed removed peer");
            });
        }

        public override string Type => SubscriptionType.NewHeads;

        public override void Dispose()
        {
            _peerPool.PeerRemoved -= OnPeerDropped;
            base.Dispose();
            if (_logger.IsTrace) _logger.Trace($"admin_peerEvent.Drop subscription {Id} will no longer track PeerRemoved");
        }
    }
}
