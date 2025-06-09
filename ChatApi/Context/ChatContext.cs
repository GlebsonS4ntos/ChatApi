using ChatApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ChatApi.Context
{
    public class ChatContext : IdentityDbContext<User>
    {
        public ChatContext(DbContextOptions<ChatContext> options) : base(options)
        {

        }
    }
}
