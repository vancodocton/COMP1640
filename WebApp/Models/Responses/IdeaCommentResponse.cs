﻿namespace WebApp.Models.Responses
{
    public class IdeaCommentResponse
    {
        public int IdeaId { get; set; }

        public string? UserName { get; set; }

        public int? CommentId { get; set; }

        public string? Content { get; set; }
    }
}