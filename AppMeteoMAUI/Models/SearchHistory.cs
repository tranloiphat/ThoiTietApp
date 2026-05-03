using SQLite;

namespace ThoiTietApp.Models
{
    // Model ánh xạ sang bảng SearchHistory trong SQLite
    [Table("SearchHistory")]
    public class SearchHistory
    {
        // Khóa chính tự tăng — SQLite tự quản lý
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string CityName { get; set; } = string.Empty;

        public DateTime SearchTime { get; set; }
    }
}
