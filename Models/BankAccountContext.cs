using Microsoft.EntityFrameworkCore;

namespace BankAccounts.Models
{
    public class BankAccountsContext : DbContext
    {
        public BankAccountsContext (DbContextOptions<BankAccountsContext> options) : base(options) { }
        public DbSet<User> User {get;set;}
        public DbSet<Transaction> Transaction {get;set;}
    }
}
