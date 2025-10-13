using Supabase;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using Xunit;
using System.Linq;

[Table("uploaded_files")]
public class UploadedFile : BaseModel
{
    [PrimaryKey("id")]
    public int Id { get; set; }
    [Column("uploader_user_id")]
    public int UploaderUserId { get; set; }
    [Column("filename")]
    public string Filename { get; set; }
    [Column("file_type")]
    public string FileType { get; set; }
    [Column("size_bytes")]
    public int SizeBytes { get; set; }
    [Column("storage_location")]
    public string StorageLocation { get; set; }
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}

public class UploadedFileTests
{
    [Fact]
    public async Task InsertUploadedFile_ShouldInsert_WhenDataIsValid()
    {
        string supabaseUrl = "https://uywbyuywxwujlsrrpwok.supabase.co";
        string supabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InV5d2J5dXl3eHd1amxzcnJwd29rIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NTc0MjkxNTgsImV4cCI6MjA3MzAwNTE1OH0.5buDqTjfAzVCgRO3pmhAVR3n2Rxeexd47xiq3HWIZOM";
        var client = new Supabase.Client(supabaseUrl, supabaseKey);
        await client.InitializeAsync();

        var file = new UploadedFile
        {
            UploaderUserId = 1,
            Filename = "xunit-test.pdf",
            FileType = "application/pdf",
            SizeBytes = 2048,
            StorageLocation = "https://bucket/path/xunit-test.pdf",
            CreatedAt = DateTime.UtcNow
        };
        var response = await client.Table<UploadedFile>().Insert(file);
        Assert.NotNull(response.Models.FirstOrDefault());
    }
}
