using System;
using System.Collections.Generic;

namespace ProMa.Models
{
    public partial class ProMaUser
    {
        public ProMaUser()
        {
            CalendarEntries = new HashSet<CalendarEntry>();
            FriendshipRequestsRecipient = new HashSet<FriendshipRequest>();
            FriendshipRequestsSender = new HashSet<FriendshipRequest>();
            FriendshipsMemberOne = new HashSet<Friendship>();
            FriendshipsMemberTwo = new HashSet<Friendship>();
            NoteTypeMemberships = new HashSet<NoteTypeMembership>();
            PostedNotesCompletedUser = new HashSet<PostedNote>();
            PostedNotesEditedUser = new HashSet<PostedNote>();
            PostedNotesUser = new HashSet<PostedNote>();
            SharedChoreMemberships = new HashSet<SharedChoreMembership>();
        }

        public int UserId { get; set; }
        public string UserName { get; set; }
        public string HashedPassword { get; set; }
        public DateTimeOffset JoinTime { get; set; }
        public bool IsAdmin { get; set; }
        public bool EnterIsNewLinePref { get; set; }
        public string EmailAddress { get; set; }
        public bool IsDemo { get; set; }

        public ICollection<CalendarEntry> CalendarEntries { get; set; }
        public ICollection<FriendshipRequest> FriendshipRequestsRecipient { get; set; }
        public ICollection<FriendshipRequest> FriendshipRequestsSender { get; set; }
        public ICollection<Friendship> FriendshipsMemberOne { get; set; }
        public ICollection<Friendship> FriendshipsMemberTwo { get; set; }
        public ICollection<NoteTypeMembership> NoteTypeMemberships { get; set; }
        public ICollection<PostedNote> PostedNotesCompletedUser { get; set; }
        public ICollection<PostedNote> PostedNotesEditedUser { get; set; }
        public ICollection<PostedNote> PostedNotesUser { get; set; }
        public ICollection<SharedChoreMembership> SharedChoreMemberships { get; set; }
    }
}
