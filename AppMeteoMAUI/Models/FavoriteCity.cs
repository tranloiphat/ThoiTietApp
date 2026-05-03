using SQLite;

namespace ThoiTietApp.Models
{
    // Model ánh xạ sang bảng FavoriteCity trong SQLite
    [Table("FavoriteCity")]
    public class FavoriteCity
    {
        // Khóa chính tự tăng — SQLite tự quản lý
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string CityName { get; set; } = string.Empty;

        public DateTime AddedTime { get; set; }
    }
}
