using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using cnpm.Models;

[Route("api/[controller]")]
[ApiController]
public class ChatController : Controller
{
    private readonly BiaContext _context;

    public ChatController(BiaContext context)
    {
        _context = context;
    }

    [HttpGet("getUserId")]
    public IActionResult GetUserId()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        return Ok(new { userId });
    }

    // API lấy danh sách tin nhắn của nhân viên (ReceiverId = EmployeeID)
    [HttpGet("employee/{employeeId}")]
    public async Task<IActionResult> GetMessagesForEmployee(int employeeId)
    {
        var messages = await _context.Chats
            .Where(c => c.ReceiverId == employeeId || c.SenderId == employeeId)

            .OrderByDescending(c => c.Timestamp)
            .Select(c => new
            {
                c.SenderId,
                c.SenderName,
                c.Message,
                c.Timestamp
            })
            .ToListAsync();

        return Json(messages);
    }
    [HttpGet("Support")]
    public IActionResult EmployeeSupport()
    {
        return View(); // View này sẽ hiển thị trang tư vấn khách hàng
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendMessageToDatabase([FromBody] Chat messageModel)
    {
        if (messageModel == null || string.IsNullOrEmpty(messageModel.Message))
        {
            return BadRequest(new { error = "Dữ liệu không hợp lệ." });
        }

        // Kiểm tra SenderId hợp lệ (không được null và phải lớn hơn 0)
        if (messageModel.SenderId <= 0)
        {
            return BadRequest(new { error = "❌ SenderId không hợp lệ!" });
        }

        var message = new Chat
        {
            SenderId = messageModel.SenderId, // ✅ Đảm bảo giá trị hợp lệ
            SenderName = messageModel.SenderName ?? "Khách hàng",
            ReceiverId = messageModel.ReceiverId,
            ReceiverName = messageModel.ReceiverName,
            Message = messageModel.Message,
            Timestamp = DateTime.Now
        };

        _context.Chats.Add(message);
        await _context.SaveChangesAsync();

        return Ok(new { success = true });
    }







}
