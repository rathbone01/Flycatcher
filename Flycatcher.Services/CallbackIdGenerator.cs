using System.Security.Cryptography;
using System.Text;
using Flycatcher.Services.Enumerations;

namespace Flycatcher.Services
{
    public static class CallbackIdGenerator
    {
        // Fixed namespace GUID used as the base for UUID v5 generation.
        // This value is arbitrary but must remain constant.
        private static readonly Guid Namespace =
            Guid.Parse("7c9e6679-7425-40de-944b-e07fc1f90ae7");

        /// <summary>
        /// Generates a deterministic Guid from a CallbackType and a single entity ID.
        /// Both subscribers and notifiers call this with the same arguments to get the same Guid.
        /// </summary>
        public static Guid CreateId(CallbackType type, int entityId)
        {
            string input = $"{type}:{entityId}";
            return GenerateGuidV5(Namespace, input);
        }

        /// <summary>
        /// Generates a deterministic, order-independent Guid for a conversation between two users.
        /// </summary>
        public static Guid CreateConversationId(int userId1, int userId2)
        {
            int min = Math.Min(userId1, userId2);
            int max = Math.Max(userId1, userId2);
            string input = $"{CallbackType.DirectMessageEvent}:{min}:{max}";
            return GenerateGuidV5(Namespace, input);
        }

        /// <summary>
        /// Generates a deterministic Guid for RolesUpdated events scoped to a server.
        /// </summary>
        public static Guid CreateRolesUpdatedId(int serverId)
        {
            return CreateId(CallbackType.RolesUpdated, serverId);
        }

        /// <summary>
        /// Generates a deterministic Guid for UserRoleChanged events for a user in a server.
        /// </summary>
        public static Guid CreateUserRoleChangedId(int userId, int serverId)
        {
            string input = $"{CallbackType.UserRoleChanged}:{userId}:{serverId}";
            return GenerateGuidV5(Namespace, input);
        }

        /// <summary>
        /// Generates a deterministic Guid for UserBanned events for a specific user.
        /// </summary>
        public static Guid CreateUserBannedId(int userId)
        {
            return CreateId(CallbackType.UserBanned, userId);
        }

        /// <summary>
        /// Generates a fixed Guid for admin-wide notifications (reports, appeals).
        /// This allows all admins to subscribe to the same callback channel.
        /// </summary>
        public static Guid CreateAdminNotificationId()
        {
            // Use a fixed input to generate a consistent GUID for all admin notifications
            string input = "AdminNotifications";
            return GenerateGuidV5(Namespace, input);
        }

        /// <summary>
        /// UUID Version 5 implementation per RFC 4122.
        /// Produces a deterministic GUID from a namespace GUID and a name string.
        /// </summary>
        private static Guid GenerateGuidV5(Guid namespaceId, string name)
        {
            byte[] namespaceBytes = namespaceId.ToByteArray();
            SwapGuidByteOrder(namespaceBytes);

            byte[] nameBytes = Encoding.UTF8.GetBytes(name);
            byte[] hashInput = new byte[namespaceBytes.Length + nameBytes.Length];
            Buffer.BlockCopy(namespaceBytes, 0, hashInput, 0, namespaceBytes.Length);
            Buffer.BlockCopy(nameBytes, 0, hashInput, namespaceBytes.Length, nameBytes.Length);

            byte[] hash = SHA1.HashData(hashInput);

            byte[] guidBytes = new byte[16];
            Array.Copy(hash, 0, guidBytes, 0, 16);

            SwapGuidByteOrder(guidBytes);

            // Set version to 5
            guidBytes[7] = (byte)((guidBytes[7] & 0x0F) | 0x50);
            // Set variant to RFC 4122
            guidBytes[8] = (byte)((guidBytes[8] & 0x3F) | 0x80);

            return new Guid(guidBytes);
        }

        private static void SwapGuidByteOrder(byte[] guid)
        {
            (guid[0], guid[3]) = (guid[3], guid[0]);
            (guid[1], guid[2]) = (guid[2], guid[1]);
            (guid[4], guid[5]) = (guid[5], guid[4]);
            (guid[6], guid[7]) = (guid[7], guid[6]);
        }
    }
}
