﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PusherClient.Tests.Utilities
{
    public static class PusherFactory
    {
        public static Pusher GetPusher(IAuthorizer authorizer = null, IList<Pusher> saveTo = null)
        {
            PusherOptions options = new PusherOptions()
            {
                Authorizer = authorizer,
                Cluster = Config.Cluster,
                Encrypted = Config.Encrypted,
                IsTracingEnabled = true,
            };

            Pusher result = new Pusher(Config.AppKey, options);
            result.Error += HandlePusherError;
            if (saveTo != null)
            {
                saveTo.Add(result);
            }

            return result;
        }

        public static Pusher GetPusher(ChannelTypes channelType, string username = null, IList<Pusher> saveTo = null)
        {
            Pusher result;
            switch (channelType)
            {
                case ChannelTypes.Private:
                case ChannelTypes.Presence:
                    result = GetPusher(new FakeAuthoriser(username ?? UserNameFactory.CreateUniqueUserName()), saveTo: saveTo);
                    break;

                default:
                    result = GetPusher(authorizer: null, saveTo: saveTo);
                    break;
            }

            return result;
        }

        public static async Task DisposePusherClientAsync(Pusher pusher)
        {
            if (pusher != null)
            {
                await pusher.ConnectAsync().ConfigureAwait(false);
                await pusher.UnsubscribeAllAsync().ConfigureAwait(false);
                await pusher.DisconnectAsync().ConfigureAwait(false);
            }
        }

        public static async Task DisposePushersAsync(IList<Pusher> pushers)
        {
            if (pushers != null)
            {
                for (int i = 0; i < pushers.Count; i++)
                {
                    await DisposePusherClientAsync(pushers[i]).ConfigureAwait(false);
                    pushers[i] = null;
                }

                pushers.Clear();
            }
        }

        private static void HandlePusherError(object sender, PusherException error)
        {
            Pusher pusher = sender as Pusher;
            System.Diagnostics.Trace.TraceError($"Pusher error detected on socket {pusher.SocketID}:{Environment.NewLine}{error}");
        }
    }
}