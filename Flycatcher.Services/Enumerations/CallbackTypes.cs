using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flycatcher.Services.Enumerations
{
    public enum CallbackType
    {
        ServerInvite,               // Server invite received or responded to from the recipient's side
        ChannelMessageEvent,        // Channel message received, edited or deleted, updates the message list in the channel
        FriendRequest,              // Friend request received or responded to from the recipient's side
        ServerPropertyUpdated,      // Updates the left side bar, so channel names, server names, added or removed channels etc.
        ServerUserUpdated,          // Updates the right side bar, so member names, roles, added or removed members etc.
        UserServerListUpdated,      // Updates the server list when a user joins or leaves a server
        FriendsListUpdated,         // Updates the friends list for both RX and TX, so added or removed friends, friend requests etc.
        ChannelDeleted,             // Channel deleted
        ServerDeleted,              // Server deleted
        DirectMessageEvent          // Direct message received between two users
    }
}
