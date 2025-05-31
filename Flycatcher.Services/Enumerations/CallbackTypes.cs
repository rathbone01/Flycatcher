using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flycatcher.Services.Enumerations
{
    public enum CallbackType
    {
        // These basic types will be deprecated in favor of more specific types to cause less server rendering
        Channel,
        Server,
        User,

        // Need a more comprehensive list of callback types
        ServerInvite,               // Server invite received or responded to from the recipient's side
        ChannelMessageRecieved,     // Channel message received, so a new message in a channel, updates the message list in the channel
        FriendRequest,              // Friend request received or responded to from the recipient's side
        ServerPropertyUpdated,      // Updates the left side bar, so channel names, server names, added or removed channels etc.
        ServerMemberUpdated,        // Updates the right side bar, so member names, roles, added or removed members etc.
        FriendsListUpdated,         // Updates the friends list for both RX and TX, so added or removed friends, friend requests etc.
        ChannelDeleted,             // Channel deleted
        ServerDeleted,              // Server deleted
    }
}
