// SPDX-FileCopyrightText: 2022 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only

using System;
using Nethermind.Logging;
using Nethermind.JsonRpc.Modules.Subscribe;
using Nethermind.Network;

namespace Nethermind.JsonRpc.Modules.Admin
{
    public class PeerEventsSubscription : Subscription
    {
        private readonly IPeerPool _peerPool;


        public PeerEventsSubscription(
            IJsonRpcDuplexClient jsonRpcDuplexClient,
            ILogManager? logManager,
            IPeerPool peerPool = null)
            : base(jsonRpcDuplexClient)
        {
            _logger = logManager?.GetClassLogger() ?? throw new ArgumentNullException(nameof(logManager));
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
