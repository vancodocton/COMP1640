namespace WebApp.Data
{
    public static class UploadFileHelper
    {
        public static readonly List<string> DocumentExtensions = new()
        {
            ".xlsx",
            ".pptx",
            ".docx",
            ".xls",
            ".ppt",
            ".doc",
            ".pdf",
            ".txt",
        };

        public const int MaxFileSize = 5242880; // = 5MB
    }
}
