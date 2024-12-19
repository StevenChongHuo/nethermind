// SPDX-FileCopyrightText: 2022 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only

namespace Nethermind.JsonRpc.Modules.Subscribe
{
    public struct PeerEventsSubscriptionType
    {
        public const string Add = "add";
        public const string Drop = "drop";
        public const string Msgsend = "msgsend";
        public const string Msgrecv = "msgrecv";
        public const string PeerEvents = "peerEvents";
    }
}
