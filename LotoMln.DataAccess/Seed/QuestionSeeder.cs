using LotoMln.DataAccess.DBContext;
using LotoMln.Models.Entities;
using LotoMln.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace LotoMln.DataAccess.Seed;

public static class QuestionSeeder
{
    public static async Task SeedAsync(AppDbContext db, CancellationToken ct = default)
    {
        if (await db.Questions.AnyAsync(ct))
            return;   // idempotent: đã seed rồi thì skip

        var questions = new List<Question>
        {
            // ============== NORMAL QUESTIONS (15) ==============
            new() { Id = Guid.NewGuid(), Type = QuestionType.Normal, Source = "VK XIV T1 tr.45",
                Text = "Theo VK ĐH XIV, năm 2025 quy mô GDP của Việt Nam ước đạt khoảng bao nhiêu, đưa Việt Nam lên thứ hạng nào về quy mô kinh tế thế giới?",
                Options = ["350 tỷ USD - hạng 50", "514 tỷ USD - hạng 32", "700 tỷ USD - hạng 25", "1000 tỷ USD - hạng 20"],
                CorrectIndex = 1 },

            new() { Id = Guid.NewGuid(), Type = QuestionType.Normal, Source = "VK XIV T1 tr.45",
                Text = "GDP bình quân đầu người Việt Nam 2025 đạt ~5.026 USD. Điều này có ý nghĩa gì?",
                Options = ["Vẫn ở nhóm thu nhập thấp", "Đã vào nhóm thu nhập trung bình cao", "Đã thành nước phát triển", "Đã vượt nhóm OECD"],
                CorrectIndex = 1 },

            new() { Id = Guid.NewGuid(), Type = QuestionType.Normal, Source = "VK XIV T1 tr.47",
                Text = "Đóng góp của TFP vào tăng trưởng 2021-2025 đạt ~47%, thể hiện điều gì?",
                Options = ["Dựa chủ yếu vào tài nguyên", "Chuyển sang chiều sâu, chất lượng", "Hoàn toàn dựa vào KH-CN", "Vượt mức các nước phát triển"],
                CorrectIndex = 1 },

            new() { Id = Guid.NewGuid(), Type = QuestionType.Normal, Source = "VK XIV T1 tr.63",
                Text = "Khái niệm 'điểm nghẽn của điểm nghẽn' trong VK ĐH XIV chỉ về vấn đề nào?",
                Options = ["Hạ tầng giao thông", "Thể chế", "Nguồn nhân lực", "Vốn đầu tư"],
                CorrectIndex = 1 },

            new() { Id = Guid.NewGuid(), Type = QuestionType.Normal, Source = "VK XIV T1 tr.52",
                Text = "Đâu KHÔNG phải 1 trong 3 trụ cột xây dựng CNXH ở Việt Nam?",
                Options = ["Kinh tế thị trường định hướng XHCN", "Nhà nước pháp quyền XHCN", "Dân chủ XHCN", "Hợp tác quốc tế đa phương"],
                CorrectIndex = 3 },

            new() { Id = Guid.NewGuid(), Type = QuestionType.Normal, Source = "VK XIV T1 tr.48",
                Text = "Việt Nam có ĐTCL Toàn diện với 38 nước. Điểm đặc biệt nào nói lên vị thế đối ngoại VN?",
                Options = ["ĐTCLTD với 3/5 ủy viên thường trực HĐBA", "ĐTCLTD với 5/5 P5 + 7/7 G7", "ĐTCLTD với toàn bộ ASEAN", "ĐTCLTD với toàn bộ G20"],
                CorrectIndex = 1 },

            new() { Id = Guid.NewGuid(), Type = QuestionType.Normal, Source = "VK XIV T1 tr.57",
                Text = "Trong cải cách 'sắp xếp giang sơn' 2025, thay đổi nào là CHƯA TỪNG CÓ kể từ 1945?",
                Options = ["Giảm số xã 66,9%", "Bỏ cấp huyện", "Giảm số tỉnh 46%", "Giảm bộ ngành TW 34,9%"],
                CorrectIndex = 1 },

            new() { Id = Guid.NewGuid(), Type = QuestionType.Normal, Source = "VK XIV T1 tr.46",
                Text = "HDI Việt Nam 2021-2025 tăng 14 bậc đạt 0,766. Điều này phản ánh đặc trưng nào của mô hình phát triển VN?",
                Options = ["Tăng trưởng nóng bất chấp xã hội", "Phát triển hài hòa KT-XH-con người", "Ưu tiên kinh tế hơn con người", "Phát triển chậm nhưng đều"],
                CorrectIndex = 1 },

            new() { Id = Guid.NewGuid(), Type = QuestionType.Normal, Source = "VK XIV T1 tr.46",
                Text = "Tỷ lệ nghèo đa chiều VN giảm từ 4,2% (2022) xuống 1,3% (2025). So với cam kết SDG 2030 của LHQ thì sao?",
                Options = ["Việt Nam chậm hơn nhiều nước", "Việt Nam đi trước cam kết SDG 2030", "Việt Nam đúng tiến độ", "Không liên quan đến SDG"],
                CorrectIndex = 1 },

            new() { Id = Guid.NewGuid(), Type = QuestionType.Normal, Source = "VK XIV T1 tr.65",
                Text = "Hệ số ICOR Việt Nam 2021-2025 ở mức 6,4. Đây là dấu hiệu của hạn chế nào?",
                Options = ["Tăng trưởng quá nhanh", "Hiệu quả đầu tư còn thấp", "Lạm phát cao", "Xuất khẩu giảm"],
                CorrectIndex = 1 },

            new() { Id = Guid.NewGuid(), Type = QuestionType.Normal, Source = "VK XIV T1 tr.66",
                Text = "Câu nào KHÔNG xuất hiện trong VK ĐH XIV với tư cách hạn chế?",
                Options = ["'Tư duy nhiệm kỳ'", "'Lợi ích nhóm'", "'Bệnh thành tích'", "'Đại nhảy vọt'"],
                CorrectIndex = 3 },

            new() { Id = Guid.NewGuid(), Type = QuestionType.Normal, Source = "VK XIV T1 tr.45",
                Text = "Tăng trưởng GDP 2025 ước 8,02%. Yếu tố chính tạo nên kết quả này theo VK XIV?",
                Options = ["May mắn chu kỳ kinh tế", "Tổng hòa cải cách thể chế + đầu tư công + xuất khẩu", "FDI tăng đột biến", "Du lịch phục hồi"],
                CorrectIndex = 1 },

            new() { Id = Guid.NewGuid(), Type = QuestionType.Normal, Source = "VK XIV T1 tr.46",
                Text = "Bao phủ BHYT Việt Nam 2025 đạt ~95,2%. Đây là chỉ số phản ánh gì?",
                Options = ["Chỉ số kinh tế thuần", "Tiến bộ ASXH theo định hướng XHCN", "Sức khỏe người dân", "Phát triển ngành dược"],
                CorrectIndex = 1 },

            new() { Id = Guid.NewGuid(), Type = QuestionType.Normal, Source = "VK XIV T1 tr.46",
                Text = "VN xếp 46/143 báo cáo Hạnh phúc 2025, tăng 33 bậc so 2020. Điều này KHÔNG phải minh chứng cho?",
                Options = ["Phát triển vì con người", "Hiệu quả 3 trụ cột XHCN", "Ổn định chính trị-xã hội", "Việt Nam đã là nước phát triển"],
                CorrectIndex = 3 },

            new() { Id = Guid.NewGuid(), Type = QuestionType.Normal, Source = "VK XIV T1 tr.48",
                Text = "Số hiệp định thương mại tự do (FTA) Việt Nam đã ký đến 2025?",
                Options = ["8", "12", "17", "25"],
                CorrectIndex = 2 },

            // ============== REDEMPTION QUESTIONS (5) - 6 options ==============
            new() { Id = Guid.NewGuid(), Type = QuestionType.Redemption, Source = "VK XIV T1 (mốc lịch sử)",
                Text = "Sắp xếp đúng trình tự các sự kiện Đổi mới:",
                Options = [
                    "ĐH VI (1986) → WTO (2007) → Cương lĩnh 1991 → ĐTCLTD đầu tiên (2008)",
                    "ĐH VI (1986) → Cương lĩnh 1991 → WTO (2007) → ĐTCLTD đầu tiên (2008)",
                    "Cương lĩnh 1991 → ĐH VI (1986) → WTO (2007) → ĐTCLTD (2008)",
                    "ĐH VI (1986) → WTO (2007) → ĐTCLTD (2008) → Cương lĩnh 1991",
                    "Cương lĩnh 1991 → WTO (2007) → ĐH VI (1986) → ĐTCLTD (2008)",
                    "ĐTCLTD (2008) → ĐH VI (1986) → Cương lĩnh 1991 → WTO (2007)"
                ],
                CorrectIndex = 1 },

            new() { Id = Guid.NewGuid(), Type = QuestionType.Redemption, Source = "VK XIV T1 tr.67",
                Text = "Theo VK XIV, đâu là 'nguy cơ lớn' đối với Đảng cầm quyền cần cảnh giác?",
                Options = [
                    "Tham nhũng, lãng phí",
                    "Suy thoái tư tưởng chính trị, đạo đức, lối sống",
                    "'Tự diễn biến', 'tự chuyển hóa' trong nội bộ",
                    "Tất cả các nguy cơ trên",
                    "Chỉ A và B",
                    "Chỉ tham nhũng"
                ],
                CorrectIndex = 3 },

            new() { Id = Guid.NewGuid(), Type = QuestionType.Redemption, Source = "VK XIV T1 tr.70",
                Text = "Theo VK XIV, VN phấn đấu trở thành nước có thu nhập cao vào năm nào?",
                Options = ["2030", "2035", "2040", "2045", "2050", "2060"],
                CorrectIndex = 3 },

            new() { Id = Guid.NewGuid(), Type = QuestionType.Redemption, Source = "VK XIV T1 tr.72-76",
                Text = "Đâu KHÔNG phải 1 trong 5 bài học sau 40 năm Đổi mới?",
                Options = [
                    "Kiên định mục tiêu CNXH, sáng tạo lý luận",
                    "Phát huy đại đoàn kết, lấy dân làm gốc",
                    "Kết hợp sức mạnh dân tộc với sức mạnh thời đại",
                    "Đặt lợi ích kinh tế lên trên tất cả",
                    "Xây dựng Đảng vững mạnh",
                    "Đổi mới đồng bộ, có trọng tâm"
                ],
                CorrectIndex = 3 },

            new() { Id = Guid.NewGuid(), Type = QuestionType.Redemption, Source = "VK XIV T1 tr.48",
                Text = "Đâu là thành tựu DUY NHẤT chỉ VN đạt được trong khu vực ĐNA giai đoạn 40 năm?",
                Options = [
                    "Tăng trưởng kinh tế dương liên tục",
                    "Chuyển từ thu nhập thấp lên trung bình",
                    "Có ĐTCLTD với 5/5 P5 + 7/7 G7",
                    "Tham gia WTO",
                    "Phát triển công nghệ thông tin",
                    "Tăng tuổi thọ trung bình"
                ],
                CorrectIndex = 2 },
        };

        db.Questions.AddRange(questions);
        await db.SaveChangesAsync(ct);
    }
}