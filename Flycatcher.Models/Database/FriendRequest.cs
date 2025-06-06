﻿using Microsoft.EntityFrameworkCore;

namespace Flycatcher.Models.Database
{
    public class FriendRequest
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }

        // Navigation properties
        public User Sender { get; set; } = null!;
        public User Receiver { get; set; } = null!;
    }
}
