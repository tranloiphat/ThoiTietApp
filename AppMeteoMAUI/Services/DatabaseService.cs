using SQLite;
using ThoiTietApp.Models;

namespace ThoiTietApp.Services
{
    // DatabaseService: quản lý toàn bộ thao tác đọc/ghi SQLite cho app
    public class DatabaseService
    {
        private SQLiteAsyncConnection? _db;

        // Lazy-init pattern: chỉ khởi tạo kết nối lần đầu tiên cần dùng
        // "if (_db != null) return" → các lần gọi sau bỏ qua, không mở kết nối trùng
        public async Task InitAsync()
        {
            if (_db != null) return;
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "weather.db");
            _db = new SQLiteAsyncConnection(dbPath);
            await _db.CreateTableAsync<SearchHistory>();
            await _db.CreateTableAsync<FavoriteCity>();
        }

        // Lưu lịch sử tìm kiếm — nếu thành phố đã có thì chỉ cập nhật thời gian
        public async Task SaveSearchAsync(string cityName)
        {
            await InitAsync();
            var existing = await _db!.Table<SearchHistory>()
                .Where(s => s.CityName == cityName)
                .FirstOrDefaultAsync();

            if (existing != null)
            {
                existing.SearchTime = DateTime.Now;
                await _db.UpdateAsync(existing);
            }
            else
            {
                await _db.InsertAsync(new SearchHistory
                {
                    CityName = cityName,
                    SearchTime = DateTime.Now
                });
            }
        }

        // Lấy danh sách tìm kiếm gần đây, sắp xếp mới nhất lên đầu
        public async Task<List<SearchHistory>> GetRecentSearchesAsync(int limit = 10)
        {
            await InitAsync();
            return await _db!.Table<SearchHistory>()
                .OrderByDescending(s => s.SearchTime)
                .Take(limit)
                .ToListAsync();
        }

        // Thêm thành phố yêu thích — bỏ qua nếu đã tồn tại
        public async Task AddFavoriteAsync(string cityName)
        {
            await InitAsync();
            var existing = await _db!.Table<FavoriteCity>()
                .Where(f => f.CityName == cityName)
                .FirstOrDefaultAsync();

            if (existing == null)
                await _db.InsertAsync(new FavoriteCity
                {
                    CityName = cityName,
                    AddedTime = DateTime.Now
                });
        }

        // Lấy toàn bộ thành phố yêu thích, sắp xếp mới thêm lên đầu
        public async Task<List<FavoriteCity>> GetFavoritesAsync()
        {
            await InitAsync();
            return await _db!.Table<FavoriteCity>()
                .OrderByDescending(f => f.AddedTime)
                .ToListAsync();
        }

        // Xóa thành phố khỏi danh sách yêu thích theo tên
        public async Task RemoveFavoriteAsync(string cityName)
        {
            await InitAsync();
            var item = await _db!.Table<FavoriteCity>()
                .Where(f => f.CityName == cityName)
                .FirstOrDefaultAsync();

            if (item != null)
                await _db.DeleteAsync(item);
        }
    }
}
