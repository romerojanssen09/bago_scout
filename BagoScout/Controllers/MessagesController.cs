using Microsoft.AspNetCore.Mvc;
using BagoScout.Data;
using BagoScout.Models;
using Microsoft.EntityFrameworkCore;

namespace BagoScout.Controllers
{
    public class MessagesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MessagesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Check if user is logged in
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Redirect("/?login=true");
            }

            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserId = userId.Value;
            ViewBag.UserType = HttpContext.Session.GetString("UserType");
            return View();
        }

        [HttpGet]
        [Route("api/messages/conversations")]
        public async Task<IActionResult> GetConversations()
        {
            try
            {
                var userId = await this.GetAuthorizedUserIdAsync(_context);
                if (userId == null)
                {
                    return Unauthorized(new { message = "User not logged in" });
                }

                // Get all unique conversations
                var conversations = await _context.Messages
                    .Where(m => m.SenderId == userId.Value || m.ReceiverId == userId.Value)
                    .GroupBy(m => m.SenderId == userId.Value ? m.ReceiverId : m.SenderId)
                    .Select(g => new
                    {
                        UserId = g.Key,
                        LastMessage = g.OrderByDescending(m => m.SentAt).First().MessageText,
                        LastMessageTime = g.OrderByDescending(m => m.SentAt).First().SentAt,
                        UnreadCount = g.Count(m => m.ReceiverId == userId.Value && !m.IsRead)
                    })
                    .ToListAsync();

                // Get user details for each conversation
                var conversationsWithUsers = new List<object>();
                foreach (var conv in conversations)
                {
                    var user = await _context.Users
                        .Where(u => u.UserId == conv.UserId)
                        .Select(u => new
                        {
                            u.UserId,
                            u.FirstName,
                            u.LastName,
                            SelfiePhotoPath = u.UserType == "Employer" ? u.CompanyLogoPath : u.SelfiePhotoPath,
                            u.UserType
                        })
                        .FirstOrDefaultAsync();

                    if (user != null)
                    {
                        conversationsWithUsers.Add(new
                        {
                            user.UserId,
                            user.FirstName,
                            user.LastName,
                            user.SelfiePhotoPath,
                            user.UserType,
                            conv.LastMessage,
                            conv.LastMessageTime,
                            conv.UnreadCount
                        });
                    }
                }

                return Ok(conversationsWithUsers.OrderByDescending(c => ((dynamic)c).LastMessageTime));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching conversations", error = ex.Message });
            }
        }

        [HttpGet]
        [Route("api/messages/{otherUserId}")]
        public async Task<IActionResult> GetMessages(int otherUserId)
        {
            try
            {
                var userId = await this.GetAuthorizedUserIdAsync(_context);
                if (userId == null)
                {
                    return Unauthorized(new { message = "User not logged in" });
                }

                var messages = await _context.Messages
                    .Where(m => (m.SenderId == userId.Value && m.ReceiverId == otherUserId) ||
                               (m.SenderId == otherUserId && m.ReceiverId == userId.Value))
                    .OrderBy(m => m.SentAt)
                    .ToListAsync();

                // Mark messages as read
                var unreadMessages = messages.Where(m => m.ReceiverId == userId.Value && !m.IsRead).ToList();
                foreach (var msg in unreadMessages)
                {
                    msg.IsRead = true;
                }
                if (unreadMessages.Any())
                {
                    await _context.SaveChangesAsync();
                }

                return Ok(messages);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching messages", error = ex.Message });
            }
        }

        [HttpPost]
        [Route("api/messages/send")]
        public async Task<IActionResult> SendMessage([FromBody] MessageRequest request)
        {
            try
            {
                var userId = await this.GetAuthorizedUserIdAsync(_context);
                if (userId == null)
                {
                    return Unauthorized(new { message = "User not logged in" });
                }

                var message = new Message
                {
                    SenderId = userId.Value,
                    ReceiverId = request.ReceiverId,
                    MessageText = request.MessageText,
                    SentAt = DateTime.UtcNow,
                    IsRead = false
                };

                _context.Messages.Add(message);
                await _context.SaveChangesAsync();

                // Send push notification to recipient
                var sender = await _context.Users.FindAsync(userId.Value);
                var receiver = await _context.Users.FindAsync(request.ReceiverId);
                if (receiver != null && !string.IsNullOrEmpty(receiver.FcmToken))
                {
                    var senderName = sender != null ? $"{sender.FirstName} {sender.LastName}".Trim() : "Bago Scout User";
                    var data = new Dictionary<string, string>
                    {
                        { "type", "chat" },
                        { "senderId", userId.Value.ToString() }
                    };
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await BagoScout.Services.PushNotificationHelper.SendNotificationAsync(receiver.FcmToken, senderName, request.MessageText, data);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error sending chat push notification: {ex.Message}");
                        }
                    });
                }

                return Ok(new { message = "Message sent successfully", messageId = message.MessageId, sentAt = message.SentAt });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error sending message", error = ex.Message });
            }
        }
    }

    public class MessageRequest
    {
        public int ReceiverId { get; set; }
        public string MessageText { get; set; } = string.Empty;
    }
}
