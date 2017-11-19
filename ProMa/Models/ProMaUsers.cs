using System;
using System.Collections.Generic;

namespace ProMa.Models
{
    public partial class ProMaUsers
    {
        public ProMaUsers()
        {
            CalendarEntries = new HashSet<CalendarEntries>();
            FriendshipRequestsRecipient = new HashSet<FriendshipRequests>();
            FriendshipRequestsSender = new HashSet<FriendshipRequests>();
            FriendshipsMemberOne = new HashSet<Friendships>();
            FriendshipsMemberTwo = new HashSet<Friendships>();
            NoteTypeMemberships = new HashSet<NoteTypeMemberships>();
            PostedNotesCompletedUser = new HashSet<PostedNotes>();
            PostedNotesEditedUser = new HashSet<PostedNotes>();
            PostedNotesUser = new HashSet<PostedNotes>();
            SharedChoreMemberships = new HashSet<SharedChoreMemberships>();
        }

        public int UserId { get; set; }
        public string UserName { get; set; }
        public string HashedPassword { get; set; }
        public DateTimeOffset JoinTime { get; set; }
        public bool IsAdmin { get; set; }
        public bool EnterIsNewLinePref { get; set; }
        public string EmailAddress { get; set; }
        public bool IsDemo { get; set; }

        public ICollection<CalendarEntries> CalendarEntries { get; set; }
        public ICollection<FriendshipRequests> FriendshipRequestsRecipient { get; set; }
        public ICollection<FriendshipRequests> FriendshipRequestsSender { get; set; }
        public ICollection<Friendships> FriendshipsMemberOne { get; set; }
        public ICollection<Friendships> FriendshipsMemberTwo { get; set; }
        public ICollection<NoteTypeMemberships> NoteTypeMemberships { get; set; }
        public ICollection<PostedNotes> PostedNotesCompletedUser { get; set; }
        public ICollection<PostedNotes> PostedNotesEditedUser { get; set; }
        public ICollection<PostedNotes> PostedNotesUser { get; set; }
        public ICollection<SharedChoreMemberships> SharedChoreMemberships { get; set; }
    }
}
