﻿namespace WebApp.Models.Hubs
{
    public class RevokeSentIdeaResponse
    {
        public int? CommentId { get; set; }

        public string? CommentOwnerUserName { get; set; }

        public string? RevokerUserName { get; set; }
    }
}
